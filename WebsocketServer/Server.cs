using System;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using OBRLibrary;
using System.Drawing.Imaging;
using System.Linq;

namespace WebsocketServer
{
    public class Server
    {
        const string URL = "http://127.0.0.1:8888/";

        private Canny _canny;
        private HttpListener _server;

        public Server(string ip, int port)
        {
            _canny = new Canny();
            _server = new HttpListener();
            _server.Prefixes.Add(URL);
            _server.Start();
            Console.WriteLine($"Сервер начал работу: {ip}:{port}");
        }

        public void Start()
        {
            try
            {
                CustomerExpectation();
            }
            catch
            {
                throw new Exception("Ошибка приема клиентов.");
            }
        }

        private void CustomerExpectation()
        {
            while (true)
            {
                HttpListenerContext context = _server.GetContext();
                Console.WriteLine("Клиент подключился.");

                if (context.Request.IsWebSocketRequest)
                {
                    ThreadPool.QueueUserWorkItem(async (state) =>
                    {
                        var contex = state as HttpListenerContext;
                        if (contex != null)
                        {
                            await ProcessWebSocketRequest(contex);
                        }
                    }, context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket websocket = webSocketContext.WebSocket;

            try
            {
                List<byte> imageDataChunks = new List<byte>();
                bool isFirstMessage = true;
                int totalSize = 0;

                while (websocket.State == WebSocketState.Open)
                {
                
                    byte[] buffer = new byte[4096];
                    WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Клиент оперативно ретировался.");
                        break;
                    }

                    if (isFirstMessage)
                    {
                        totalSize = BitConverter.ToInt32(buffer, 0); ;
                        imageDataChunks = new List<byte>(totalSize);
                        isFirstMessage = false;
                        continue;
                    }

                    imageDataChunks.AddRange(buffer.Take(result.Count));
                    if (imageDataChunks.Count == totalSize)
                    {
                        byte[] imageData = imageDataChunks.ToArray();

                        Console.WriteLine($"Получено байт от клиента: {imageData.Length} bytes");

                        (Bitmap output, double time) = OperatorCanny(imageData);
                        SendMessage(websocket, output, time);

                        buffer = null;
                        imageData = null;
                        imageDataChunks.Clear();
                        isFirstMessage = true;
                        totalSize = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                try
                {
                    await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                    Console.WriteLine("Соединение с клиентом успешно закрыто.");
                }
                catch (WebSocketException websocketEx)
                {
                    Console.WriteLine($"Соединение почему-то не закрылось: {websocketEx.Message}");
                }
                finally
                {
                    websocket.Dispose();
                }
            }
        }

        private void SendMessage(WebSocket websocket, Bitmap bitmap, double time)
        {
            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                imageData = ms.ToArray();
            }

            string base64Image = Convert.ToBase64String(imageData);
            string json = $"{{\"image\": \"{base64Image.ToString()}\", \"time\": \"{time.ToString()}\"}}";
            websocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);

            imageData = null;
            bitmap.Dispose();
        }

        private (Bitmap, double) OperatorCanny(byte[] imageData)
        {
            Bitmap bitmap = new Bitmap(new MemoryStream(imageData));
            TimeHelper timeHelper = new TimeHelper();

            timeHelper.Start();
            byte[,] result = _canny.CalculationMulty(bitmap, 10);
            //byte[,] result = _canny.Calculation(bitmap);
            double time = timeHelper.Stop();

            Bitmap output = ConvertBytesToBitmap(result);

            bitmap.Dispose();
            timeHelper = null;
            result = null;

            return (output, time);
        }

        private Bitmap ConvertBytesToBitmap(byte[,] imageData)
        {
            int width = imageData.GetLength(0);
            int height = imageData.GetLength(1);

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            Rectangle bitmapData = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bitmap.LockBits(bitmapData, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bitmapSize = width * height * 4;
            byte[] data = new byte[bitmapSize];

            int idx = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte value = imageData[x, y];
                    data[idx++] = value;
                    data[idx++] = value; 
                    data[idx++] = value; 
                    data[idx++] = 255; 
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, bitmapSize);

            bitmap.UnlockBits(bmpData);
            data = null;
            bmpData = null;

            return bitmap;
        }

        //Это от TcpListener рукопожатие
        private void Handshake(string data, NetworkStream stream)
        {
            Console.WriteLine(data);
            const string eol = "\r\n";
            byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
               + "Connection: Upgrade" + eol
               + "Upgrade: websocket" + eol
               + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                   System.Security.Cryptography.SHA1.Create().ComputeHash(
                       Encoding.UTF8.GetBytes(
                           new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                       )
                   )
            ) + eol
            + eol);

            stream.Write(response, 0, response.Length);
        }
    }
}

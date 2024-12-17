using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientEmulator
{
    internal class Client
    {
        private ClientWebSocket _client;
        public bool IsOpen => _client.State == WebSocketState.Open;
        public int ReceiveCount { get; private set; } = 0;

        public Client()
        {
            _client = new ClientWebSocket();
        }

        public async Task SendImage(Bitmap bitmap, bool isThread)
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("WebSocket connection is not open.");
            }

            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new EncoderParameters(1);
                encoder.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

                var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == ImageFormat.Png.Guid);
                bitmap.Save(memoryStream, codec, encoder);

                imageBytes = memoryStream.ToArray();
            }

            byte[] headerBytes = new byte[5];
            BitConverter.GetBytes(imageBytes.Length).CopyTo(headerBytes, 0);
            headerBytes[4] = isThread ? (byte)1 : (byte)0;
            var headerSegment = new ArraySegment<byte>(headerBytes);

            await _client.SendAsync(headerSegment, WebSocketMessageType.Binary, true, CancellationToken.None);


            int bufferSize = 1024 * 64;
            for (int i = 0; i < imageBytes.Length; i += bufferSize)
            {
                int blockSize = Math.Min(bufferSize, imageBytes.Length - i);
                var segment = new ArraySegment<byte>(imageBytes, i, blockSize);

                bool isLastSegment = (i + blockSize) >= imageBytes.Length;
                await _client.SendAsync(segment, WebSocketMessageType.Binary, isLastSegment, CancellationToken.None);
            }
        }

        public async Task ReceiveMessages(bool isOutput)
        {
            ReceiveCount = 0;

            if (!IsOpen)
            {
                throw new InvalidOperationException("WebSocket connection is not open.");
            }

            var buffer = new byte[1024];
            var stringBuilder = new StringBuilder();

            while (IsOpen)
            {
                try
                {
                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Соединение закрыто сервером.");
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                        if (result.EndOfMessage)
                        {
                            string jsonString = stringBuilder.ToString();
                            stringBuilder.Clear();

                            JsonObject response = JsonConvert.DeserializeObject<JsonObject>(jsonString);

                            if (response != null)
                            {
                                byte[] imageBytes = Convert.FromBase64String(response.Image);
                                Bitmap bitmap;

                                using (var ms = new MemoryStream(imageBytes))
                                {
                                    bitmap = new Bitmap(ms);
                                }

                                ReceiveCount++;
                               if(isOutput) ImageQueue.AddImage(bitmap, response.Time);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при приеме сообщения: {ex.Message}");
                    break;
                }
            }
        }

        public bool Connection(Uri uri)
        {
            try
            {
                if (_client == null)
                {
                    _client = new ClientWebSocket();
                }

                if(_client.State == WebSocketState.Open)
                {
                    return true;
                }

                _client.ConnectAsync(uri, CancellationToken.None);

                if (_client.State == WebSocketState.Connecting)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public void Disconnect()
        {
            try
            {
                if (_client != null)
                {
                    _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _client.Dispose();
                _client = null;
            }
        }

    }

    internal class JsonObject
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("time")]
        public float Time { get; set; }
    }
}

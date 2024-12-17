using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ClientEmulator
{
    public partial class Emulator : Form
    {
        private Client _client;
        private Random _rd;

        private string[] _imgPaths;
        private int _countTest = 1;
        private bool _isThread = false;

        private bool _isRunGetData = false;
        private bool _isOutputResult = false;

        private Stopwatch _stopwatch;

        private Uri _uri;

        public Emulator()
        {
            InitializeComponent();
            _rd = new Random();
            _client = new Client();
            _stopwatch = new Stopwatch();

            _isThread = checkBox1.Checked;
            _isOutputResult = outputCheckBox.Checked;
            errorText.Text = "";
            ipTextBox.Text = $"{GetLocalIPAddress()}:8888";
        }

        private void connectionButton_Click(object sender, EventArgs e)
        {
            errorText.Text = "";

            if (TryParseAddress(ipTextBox.Text, out string ipAddress, out int port))
            {
                _uri = new Uri($"ws://{ipAddress}:{port}/");
                if (_client.Connection(_uri))
                {
                    errorText.ForeColor = Color.Green;
                    errorText.Text = "Успех!";

                    connectionButton.Enabled = false;
                    textCount.Enabled = true;
                    addFilesButton.Enabled = true;
                }
                else
                {
                    errorText.ForeColor = Color.Red;
                    errorText.Text = "не удалось подключиться к серверу!";
                }

            }
            else
            {
                errorText.ForeColor = Color.Red;
                errorText.Text = "Неверный адрес сервера!";
            }
        }

        private bool TryParseAddress(string input, out string ipAddress, out int port)
        {
            ipAddress = null;
            port = 0;

            var parts = input.Split(':');
            if (parts.Length != 2)
                return false;

            if (!IPAddress.TryParse(parts[0], out _))
                return false;

            if (!int.TryParse(parts[1], out port) || port < 1 || port > 65535)
                return false;

            ipAddress = parts[0];
            return true;
        }
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (!IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }

            return "127.0.0.1";
        }

        private void addFilesButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите изображения";
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tiff";
                openFileDialog.Multiselect = true;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _imgPaths = openFileDialog.FileNames.Length > 1000 ? openFileDialog.FileNames.Take(1000).ToArray() : openFileDialog.FileNames;
                    fileCountString.Text = $"Количество загруженный файлов: {_imgPaths.Length}";
                    startTest.Enabled = true;
                }
            }
        }

        private void startTest_Click(object sender, EventArgs e)
        {
            if (_client.Connection(_uri))
            {
                errorText.ForeColor = Color.Green;
                errorText.Text = "Успех!";

                textCount.Enabled = false;
                addFilesButton.Enabled = false;
                startTest.Enabled = false;
                checkBox1.Enabled = false;
                outputCheckBox.Enabled = false;

                _countTest = (int)textCount.Value;
                _stopwatch.Restart();
                Testing();
            }
            else
            {
                errorText.ForeColor = Color.Red;
                errorText.Text = "не удалось подключиться к серверу!";
            }
        }

        private async void Testing()
        {
            _client.ReceiveMessages(_isOutputResult);
            if(_isOutputResult) Task.Run(GetData);


            Task.Run(GetResultCount);
            for (int i = 0; i < _countTest; i++)
            {
                using (Bitmap bitmap = new Bitmap(_imgPaths[_rd.Next(0, _imgPaths.Length)]))
                {
                    await _client.SendImage(bitmap, _isThread);
                }
            }
        }

        private void GetData()
        {
            _isRunGetData = true;
            while (_isRunGetData)
            {
                (_, float time) = ImageQueue.GetImage();

                if (time > 0)
                {
                    Invoke((Action)(() =>
                    {
                        var label = new Label
                        {
                            Text = $"Время обработки: {Math.Round(time, 0)} мс",
                            AutoSize = true,
                            Margin = new Padding(5)
                        };

                        flowLayoutPanel.Controls.Add(label);

                        flowLayoutPanel.ScrollControlIntoView(label);
                    }));

                    if(_client.ReceiveCount == _countTest)
                    {
                        break;
                    }
                }
            }
        }
        private void GetResultCount()
        {
            while (true)
            {
                completedText.Text = $"Выполнено: {_client.ReceiveCount}/{_countTest}";

                if (_client.ReceiveCount == _countTest)
                {
                    EndTest();
                    completedText.Text = $"Выполнено: {_client.ReceiveCount}/{_countTest}";
                    _stopwatch.Stop();
                    timeText.Text = $"{_stopwatch.ElapsedMilliseconds} мс";
                    break;
                }
            }
        }

        private void EndTest()
        {
            _isRunGetData = false;
            textCount.Enabled = true;
            addFilesButton.Enabled = true;
            checkBox1.Enabled = true;
            outputCheckBox.Enabled = true;
            _client.Disconnect();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _isThread = checkBox1.Checked;
        }

        private void outputCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _isOutputResult = outputCheckBox.Checked;
        }
    }
}

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;


namespace MinerClientProgramm
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
       
        //опрос майнера
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int port = 8888;
            string server = "127.0.0.1";

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(server, port);

                byte[] data = new byte[256];
                StringBuilder response = new StringBuilder();
                NetworkStream stream = client.GetStream();

                do
                {      
                    int bytes= stream.Read(data, 0, data.Length);
                    response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    MinerStatus mst = JsonSerializer.Deserialize<MinerStatus>(response.ToString());
                }
                while (stream.DataAvailable); // пока данные есть в потоке

                MinerStatus ms = JsonSerializer.Deserialize<MinerStatus>(response.ToString());
                textBoxPool.Text = ms.pool;
                textBoxUser.Text = ms.user;
                textBoxTempGPU.Text =(ms.GPUtemp).ToString();

                if (ms.running)
                     textBoxStat.Text = "Работает";
                else textBoxStat.Text = "Майнер выключен";

                // Закрываем потоки
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
               Console.WriteLine("Exception: {0}", ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string url = "https://"+textBox1.Text+":22333";
            Process.Start(url);
        }
    }
}

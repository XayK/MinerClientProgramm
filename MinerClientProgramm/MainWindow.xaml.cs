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
            int port = 9078;
            string server = textBox1.Text.ToString();

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


        /// <summary>
        /// //////через список
        private void Button_AskList(object sender, RoutedEventArgs e)
        {
            int port = 9078;
            string server = ListBox1.SelectedItem.ToString();

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(server, port);

                byte[] data = new byte[256];
                StringBuilder response = new StringBuilder();
                NetworkStream stream = client.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    MinerStatus mst = JsonSerializer.Deserialize<MinerStatus>(response.ToString());
                }
                while (stream.DataAvailable); // пока данные есть в потоке

                MinerStatus ms = JsonSerializer.Deserialize<MinerStatus>(response.ToString());
                textBoxPool.Text = ms.pool;
                textBoxUser.Text = ms.user;
                textBoxTempGPU.Text = (ms.GPUtemp).ToString();

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

        private void Button_Refresh(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(new ThreadStart(RefreshList));
            th.Start();
        }
        private static string LocalIPAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        private void RefreshList()
        {
            IPAddress remoteAddress = IPAddress.Parse("224.0.0.10");
            UdpClient senderB = new UdpClient(); // создаем UdpClient для отправки
            IPEndPoint endPoint = new IPEndPoint(remoteAddress, 9077);
            try
            {
                //while (true)
                {
                    string message = LocalIPAddress(); // сообщение для отправки
                    message = String.Format("{0}", message);
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    senderB.Send(data, data.Length, endPoint); // отправка
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            finally
            {
                senderB.Close();
            }
            GetIPsForWatcher();
        }
        private void GetIPsForWatcher()
        {
            DateTime startT = DateTime.Now;
            UdpClient receiver = new UdpClient(9077); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while ((startT-DateTime.Now).TotalSeconds<10)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    string message = Encoding.Unicode.GetString(data);
                    Dispatcher.Invoke(() =>
                        {
                            ListBox1.Items.Add(message);
                        });
                    //Console.WriteLine("Собеседник: {0}", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private void Button_WebList(object sender, RoutedEventArgs e)
        {
            string url = "https://" + ListBox1.SelectedItem.ToString() + ":22333";
            Process.Start(url);
        }
    }
}

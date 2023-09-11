using System;
using System.Windows;
using System.Windows.Controls;
using Client.Models;

namespace Client
{
    public partial class MainWindow : Window
    {
        private SocketClient socketClient;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSocketClient();
        }

        private void InitializeSocketClient()
        {
            string serverAddress = System.Configuration.ConfigurationManager.AppSettings["Address"];
            string portString = System.Configuration.ConfigurationManager.AppSettings["Port"];
            if(string.IsNullOrEmpty(serverAddress) ) 
            {
                MessageBox.Show("Не удалось получить адрес сервера из конфигурационного файла. Используется адрес по умолчанию localhost");
                serverAddress = "localhost";
            }
            int port;
            if (!int.TryParse(portString, out port))
            {
                MessageBox.Show("Не удалось получить порт из конфигурационного файла. Используется порт по умолчанию 8080");
                port = 8080;
            }
            IpTextBox.Text = serverAddress;
            PortTextBox.Text = port.ToString();
            socketClient = new SocketClient(serverAddress, port);
            socketClient.CounterUpdated += UpdateCounter;
            socketClient.RenameButton += RenameButton;
            socketClient.StartTimer();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!socketClient.IsConnected)
            {
                if (int.TryParse(PortTextBox.Text, out int serverPort))
                {
                    socketClient = new SocketClient(IpTextBox.Text, serverPort);
                    socketClient.CounterUpdated += UpdateCounter;
                    socketClient.RenameButton += RenameButton;
                    socketClient.StartTimer();

                    if (await socketClient.ConnectAsync())
                    {
                        ConnectButton.IsEnabled = false;
                        StartButton.IsEnabled = true;
                        StopContinueButton.IsEnabled = true;
                        ResetButton.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось подключиться к серверу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Некорректный порт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await socketClient.SendCommandAsync("START");
            StartButton.IsEnabled = false;
            StopContinueButton.IsEnabled = true;
        }

        private async void StopContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (socketClient.IsConnected)
            {
                if (!socketClient.IsStoped)
                {
                    StartButton.IsEnabled = false;
                    socketClient.IsStoped = true;
                    await socketClient.SendCommandAsync("STOP");
                }
                else
                {
                    StartButton.IsEnabled = false;
                    socketClient.IsStoped = false;
                    await socketClient.SendCommandAsync("CONTINUE");
                }
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            await socketClient.SendCommandAsync("RESET");
        }

        private void UpdateCounter(string response)
        {
            if (response.StartsWith("COUNT "))
            {
                int count = int.Parse(response.Substring(6));
                Dispatcher.Invoke(() => CounterText.Text = "Счетчик: " + count.ToString());
            }
        }



        private void RenameButton(string response)
        {
            Dispatcher.Invoke(() => StartButton.IsEnabled = false);
            if (response == "STOP")
            {
                Dispatcher.Invoke(() => StopContinueButton.Content = "Продолжить");
            }
            else
            {
                Dispatcher.Invoke(() => StopContinueButton.Content = "Стоп");
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            socketClient.Disconnect();
        }
    }
}

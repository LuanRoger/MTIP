using System;
using System.Net;
using System.Windows;
using System.Text;
using WatsonTcp;
using System.Windows.Input;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace MTIP
{
    public partial class MainWindow : Window
    {
        private WatsonTcpClient client;

        public MainWindow()
        {
            InitializeComponent();

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\assets\imgs\information.png");
            image.EndInit();
            imgBtnInformation.Source = image;
        }

        private void txtIp_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    txtIp.Text = ip.ToString();
                }
            }
        }
        private void txtMensagem_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    btnEnviarMensagem_Click(this, null);
                    break;
            }
        }

        private void btnConectar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client = new WatsonTcpClient(txtIp.Text, Convert.ToInt32(txtPorta.Text));
                client.Events.ServerConnected += Events_ServerConnected;
                client.Events.ServerDisconnected += Events_ServerDisconnected;
                client.Events.MessageReceived += Events_MessageReceived;
                client.Connect();
            }
            catch (Exception ex)
            {
                txtHistoricoMensagens.Text += $"Houve um error ao tentar se conectar: {ex.Message}{Environment.NewLine}";
            }
        }

        private void Events_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtHistoricoMensagens.Text += $"{e.IpPort}: {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
        }

        private void Events_ServerDisconnected(object sender, DisconnectionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtHistoricoMensagens.Text += $"{e.IpPort} disconectou-se.{Environment.NewLine}";
            });
        }

        private void Events_ServerConnected(object sender, ConnectionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtHistoricoMensagens.Text += $"{e.IpPort} conectou-se.{Environment.NewLine}";
            });
        }

        private void btnInformation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"MTIP Client v{Assembly.GetExecutingAssembly().GetName().Version}",
                "Informação",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnEnviarMensagem_Click(object sender, RoutedEventArgs e)
        {
            if (txtMensagem.Text == string.Empty) return;

            client.Send(txtMensagem.Text);
            txtHistoricoMensagens.Text += $"Eu: {txtMensagem.Text}{Environment.NewLine}";
            txtMensagem.Text = string.Empty;
        }
    }
}

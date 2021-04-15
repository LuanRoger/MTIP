using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WatsonTcp;

namespace MTIPServer
{
    public partial class MainWindow : Window
    {
        private WatsonTcpServer server;

        public MainWindow()
        {
            InitializeComponent();

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\assets\imgs\information.png");
            image.EndInit();
            imgBtnInformation.Source = image;
        }

        private void btnIniciarServidor_Click(object sender, RoutedEventArgs e)
        {
            server = new WatsonTcpServer(txtIp.Text, Convert.ToInt32(txtPorta.Text));
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.MessageReceived += Events_MessageReceived;
            server.Events.ServerStarted += Events_ServerStarted;

            server.Start();
        }
        private void txtIp_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    txtIp.Text = ip.ToString();
            }
        }

        #region ServerEvets
        private void Events_ServerStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => 
            txtHistoricoMensagens.Text += $"Servidor iniciado.{Environment.NewLine}");
        }

        private void Events_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Dispatcher.Invoke(() => 
            txtHistoricoMensagens.Text += $"{e.IpPort}: {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}");
        }

        private void Events_ClientDisconnected(object sender, DisconnectionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtHistoricoMensagens.Text += $"{e.IpPort} disconectou-se.{Environment.NewLine}";
                ltbIpsConectados.Items.Remove(e.IpPort);
            });
        }

        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtHistoricoMensagens.Text += $"{e.IpPort} conectou-se.{Environment.NewLine}";
                ltbIpsConectados.Items.Add(e.IpPort);
            });
        }
        #endregion

        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if(ltbIpsConectados.SelectedItem == null) 
                foreach (string ip in ltbIpsConectados.Items) 
                    server.Send(ip, txtMensagem.Text);
            else server.Send(ltbIpsConectados.SelectedItem.ToString(), txtMensagem.Text);

            txtHistoricoMensagens.Text += $"Eu: {txtMensagem.Text}{Environment.NewLine}";
            txtMensagem.Text = string.Empty;
        }

        //Shortcut SendMessage Keyboard
        private void txtMensagem_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    btnEnviar_Click(this, null);
                    break;
            }
        }

        private void btnInformation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"MTIP Server v{Assembly.GetExecutingAssembly().GetName().Version}",
                "Informação",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

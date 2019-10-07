using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Globalization;

namespace Interface
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void AddPeerButton_Click(object sender, RoutedEventArgs e)
        {
            var client = this.DataContext as Client;
            string ipAddress = peerAdressTextBox.Text;
            Thread th = new Thread(new ParameterizedThreadStart(client.ConnectToPeer));
            th.Start(ipAddress);
            peerAdressTextBox.Text = string.Empty;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var client = this.DataContext as Client;
            string message = messageTextBox.Text;
            messageTextBox.Text = string.Empty;
            Task.Run(() => client.BroadcastMyRumorAsync(message));
        }
    }
}

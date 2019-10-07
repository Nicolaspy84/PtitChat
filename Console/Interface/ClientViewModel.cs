using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Interface
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string str = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
        }

        public ClientViewModel () {}

        private Client client;

        public Client Client
        {
            get { return client; }

            set
            {
                if (value != client)
                {
                    client = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ICommand connexionCommand;

        public ICommand ConnexionCommand
        {
            get
            {
                if (connexionCommand == null)
                {
                    connexionCommand = new RelayCommand<object>((obj) =>
                    {
                        var param = (Tuple<object, object, object>) obj;
                        string username = ((System.Windows.Controls.TextBox)param.Item1).Text;
                        if (username == string.Empty)
                        {
                            ErrorWindow ewnd = new ErrorWindow("Le nom d'utilisateur ne peut pas être vide");
                            ewnd.Show();
                            return;
                        }
                        int port = -1;
                        try
                        {
                            port = Int32.Parse(((System.Windows.Controls.TextBox)param.Item2).Text);

                        }
                        catch (FormatException)
                        {
                            ErrorWindow ewnd = new ErrorWindow("Le port est invalide");
                            ewnd.Show();
                            return;
                        }
                        ConnexionWindow cwnd = (ConnexionWindow)param.Item3;
                        Client = new Client(username, port);
                        Thread thread = new Thread(new ThreadStart(Client.ListenForConnections));
                        thread.Start();
                        MainWindow wnd = new MainWindow();
                        wnd.Show();
                        wnd.DataContext = this;
                        cwnd.Close();
                    });
                }
                return connexionCommand;

            }
        }

        private ICommand addPeerCommand;

        public ICommand AddPeerCommand
        {
            get
            {
                if (addPeerCommand == null)
                {
                    addPeerCommand = new RelayCommand<object>((obj) =>
                    {
                        var ipAddressTextBox = (System.Windows.Controls.TextBox)obj;
                        string ipAddress = ipAddressTextBox.Text;
                        Thread th = new Thread(new ParameterizedThreadStart(Client.ConnectToPeer));
                        th.Start(ipAddress);
                        AllUsers.OnAllChanged(EventArgs.Empty);
                        ipAddressTextBox.Text = string.Empty;
                    });
                }
                return addPeerCommand;
            }
        }

        private ICommand sendMessageCommand;

        public ICommand SendMessageCommand
        {
            get
            {
                if (sendMessageCommand == null)
                {
                    sendMessageCommand = new RelayCommand<object>((obj) =>
                    {
                        var messageTextBox = (System.Windows.Controls.TextBox)obj;
                        string message = messageTextBox.Text;
                        Task.Run(() => Client.BroadcastMyRumorAsync(message));
                        messageTextBox.Text = string.Empty;
                    });
                }
                return sendMessageCommand;
            }
        }

    }
}

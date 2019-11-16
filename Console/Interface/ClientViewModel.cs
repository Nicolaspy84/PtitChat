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
    /// <summary>
    /// Class used to handle the interface
    /// </summary>
    public class ClientViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Event to handle property modifications for data binding
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method called when the properties are modified
        /// </summary>
        /// <param name="str"></param>
        public void NotifyPropertyChanged([CallerMemberName] string str = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
        }

        /// <summary>
        /// Constructor used to allow the use of the class as datacontext in the xaml code
        /// </summary>
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

        /// <summary>
        /// Observable collection for the menu to select conversation
        /// </summary>
        private ObservableCollection<string> usernames = new ObservableCollection<string>();

        public ObservableCollection<string> Usernames
        {
            get { return usernames; }

            set
            {
                if (value != usernames)
                {
                    usernames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Command for the connexion
        /// The arguments are the textboxes for username and port and the connexion window
        /// </summary>
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

        /// <summary>
        /// Command to add a peer to the connexions
        /// The argument is the textbox which contains the peer ip adress and port
        /// </summary>
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

        /// <summary>
        /// Command to send messages
        /// The first argument is the textbox in which the user writes his message
        /// Second argument is optionnal and contains the username of a user in the case where the user wants to send a private message
        /// </summary>
        private ICommand sendMessageCommand;

        public ICommand SendMessageCommand
        {
            get
            {
                if (sendMessageCommand == null)
                {
                    sendMessageCommand = new RelayCommand<object>((obj) =>
                    {
                        var parameters = (object[])obj;
                        var messageTextBox = (System.Windows.Controls.TextBox)parameters[0];
                        string message = messageTextBox.Text;
                        if (parameters[1] == null)
                        {
                            Task.Run(() => Client.BroadcastMyRumorAsync(message));
                        }
                        else
                        {
                            var destination = (string)parameters[1];
                            if (destination == "Tous")
                            {
                                Task.Run(() => Client.BroadcastMyRumorAsync(message));
                            }
                            else
                            {
                                Task.Run(() => Client.SendMyPMAsync(destination, message));
                            }
                        }
                        messageTextBox.Text = string.Empty;
                    });
                }
                return sendMessageCommand;
            }
        }

    }
}

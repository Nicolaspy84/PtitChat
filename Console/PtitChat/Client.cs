using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PtitChat
{
    /// <summary>
    /// Main client class to create a client instance
    /// </summary>
    public class Client
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="username">Unique username to represent us</param>
        /// <param name="port">Port for peers to connect to us</param>
        public Client(string username, int port)
        {

            // Setup variables
            Username = username;
            Port = port;

            // Add ourself to the user list
            lock (User.All)
            {
                User.All.Add(username, new User(username));
            }

            // Subscribe to events
            Peer.RumorReceivedEvent += BroadcastRumorAsync;
            Peer.StatusReceivedEvent += ProcessStatusAsync;

        }


        /// <summary>
        /// Our username shared with other clients
        /// </summary>
        public readonly string Username;


        /// <summary>
        /// Client port for other peers to connect to us
        /// </summary>
        public readonly int Port;


        /// <summary>
        /// List of connected peers
        /// </summary>
        public List<Peer> Peers = new List<Peer>();


        /// <summary>
        /// Returns our list of peers
        /// </summary>
        /// <returns>Description string</returns>
        public override string ToString()
        {
            string endStr = "";
            lock (Peers)
            {
                foreach (var peer in Peers)
                {
                    endStr += string.Format("{0}\n", peer);
                }
                if (Peers.Count > 0)
                {
                    endStr = endStr.Remove(endStr.Length - 1);
                }
            }
            return endStr;
        }


        /// <summary>
        /// Connects to the designated peer using its address
        /// </summary>
        /// <param name="ipAddress">string of format "ipaddress:port"</param>
        public void ConnectToPeer(object ipAddress)
        {

            // Parse information correctly
            IPAddress ip;
            int port;
            try
            {
                string[] ipPort = ((string)ipAddress).Split(':');
                ip = IPAddress.Parse(ipPort[0]);
                port = Int32.Parse(ipPort[1]);
            }
            catch (FormatException)
            {
                Console.WriteLine("Could not parse {0} as an IP/port address", ipAddress);
                return;
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Could not cast {0} as a string", ipAddress);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            // Connect to the requested peer
            Console.WriteLine("Trying to connect to {0}...", ip);

            TcpClient client = new TcpClient();

            try
            {
                client.Connect(ip, port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to {0} :", ipAddress);
                Console.WriteLine(e);
                return;
            }

            // Connection successful
            Console.WriteLine("Connected to {0}", ipAddress);
            Peer peer = new Peer(client);
            Task.Run(() => peer.ListenAsync());
            lock(Peers)
            {
                Peers.Add(peer);
            }
        }
        

        /// <summary>
        /// Once this method is called, the client will listen on its port for incoming connections
        /// </summary>
        public void ListenForConnections()
        {

            // Create a listener on our port
            TcpListener Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();


            // We loop indefinitely
            while (true)
            {

                // We wait for a client to connect to us
                TcpClient client = Listener.AcceptTcpClient();
                Console.WriteLine("{0} is initiating a connection with us...", client.Client.RemoteEndPoint);
                Peer peer = new Peer(client);
                Task.Run(() => peer.ListenAsync());
                lock(Peers)
                {
                    Peers.Add(peer);
                }

            }
        }


        /// <summary>
        /// Broadcasts our current status to all connected peers async.
        /// </summary>
        /// <param name="bounce">number of times the status packet has to be exchanged before discarding it</param>
        /// <returns>void Task</returns>
        public async Task BroadcastStatusAsync(int bounce)
        {

            // Create a status packet
            string packet = User.GetState(Username, bounce);

            // Broadcast it async.
            List<Task> listOfTasks = new List<Task>();
            lock (Peers)
            {
                int i = 0;
                List<int> removePeers = new List<int>();
                foreach (var peer in Peers)
                {
                    if (peer.Client == null)
                    {
                        removePeers.Add(i);
                    }
                    else
                    {
                        listOfTasks.Add(peer.SendPacketAsync(packet));
                    }
                    i++;
                }
                foreach (var removeIndex in removePeers)
                {
                    Peers.RemoveAt(removeIndex);
                }
            }
            await Task.WhenAll(listOfTasks);

        }


        /// <summary>
        /// Processes a received status request async.
        /// </summary>
        /// <param name="originPeer">Where the packet comes from</param>
        /// <param name="bounce">Number of times this message has to be broadcasted</param>
        /// <param name="user">User who originally sent this status request</param>
        /// <param name="status">Status content of the request</param>
        /// <returns>void Task</returns>
        public async Task ProcessStatusAsync(Peer originPeer, int bounce, string user, string status)
        {

        }


        /// <summary>
        /// Broadcasts a received rumor to other peers (all but originPeer) async.
        /// </summary>
        /// <param name="originPeer">origin of the message (who sent it to us, so we won't broadcast the message to him)</param>
        /// <param name="bounce">Number of times this message has to be broadcasted</param>
        /// <param name="user">User who originally sent this message</param>
        /// <param name="msgID">Unique message ID</param>
        /// <param name="date">Message date and time origin</param>
        /// <param name="msg">Message content</param>
        public async Task BroadcastRumorAsync(Peer originPeer, int bounce, string user, int msgID, DateTime date, string msg)
        {

            // Decrement our bounce value
            bounce--;
            if (bounce < 0)
            {
                return;
            }

            // Then we can broadcast to all other peers
            // Note : we will delete peers which have null TcpClients (they lost conenction)
            List<Task> listOfTasks = new List<Task>();
            lock (Peers)
            {
                int i = 0;
                List<int> removePeers = new List<int>();
                foreach (var peer in Peers)
                {
                    if (peer.Client == null)
                    {
                        removePeers.Add(i);
                    }
                    else if (peer != originPeer)
                    {
                        listOfTasks.Add(peer.SendRumorAsync(bounce, user, msgID, date, msg));
                    }
                    i++;
                }
                foreach (var removeIndex in removePeers)
                {
                    Peers.RemoveAt(removeIndex);
                }
            }
            await Task.WhenAll(listOfTasks);

        }


        /// <summary>
        /// Rumor to broadcast to all known peers async.
        /// </summary>
        /// <param name="message">rumor to broadcast</param>
        public async Task BroadcastMyRumorAsync(string message)
        {

            // First we create a new message (for ourself)
            int bounce = 20;
            string user = Username;
            int msgID;
            lock (User.All)
            {
                msgID = User.All[Username].NextExpectedMessageID;
            }
            DateTime date = DateTime.UtcNow;
            string msg = message;
            User.NewMessage(null, user, msgID, date, msg);


            // Then we can broadcast to all peers
            // Note : we will delete peers which have null TcpClients (they lost conenction)
            List<Task> listOfTasks = new List<Task>();
            lock(Peers)
            {
                int i = 0;
                List<int> removePeers = new List<int>();
                foreach (var peer in Peers)
                {
                    if (peer.Client == null)
                    {
                        removePeers.Add(i);
                    }
                    else
                    {
                        listOfTasks.Add(peer.SendRumorAsync(bounce, user, msgID, date, msg));
                    }
                    i++;
                }
                foreach (var removeIndex in removePeers)
                {
                    Peers.RemoveAt(removeIndex);
                }
            }
            await Task.WhenAll(listOfTasks);

        }
    }
}

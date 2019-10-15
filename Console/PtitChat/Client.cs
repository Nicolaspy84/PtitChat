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
        /// Our username
        /// </summary>
        public static string Username;

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
            lock (AllUsers.All)
            {
                AllUsers.All.Add(username, new User(username, true));
            }

            // Subscribe to events
            Peer.RumorReceivedEvent += BroadcastRumorAsync;
            Peer.PMReceivedEvent += SendPMAsync;

        }


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
            Task.Run(() => peer.RequestUpdateAsync());
            lock (Peers)
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
        /// Broadcasts a received rumor to other peers (all but originPeer) async.
        /// </summary>
        /// <param name="originPeer">origin of the message (who sent it to us, so we won't broadcast the message to him)</param>
        /// <param name="bounce">Number of times this message has been broadcasted</param>
        /// <param name="user">User who originally sent this message</param>
        /// <param name="msgID">Unique message ID</param>
        /// <param name="date">Message date and time origin</param>
        /// <param name="msg">Message content</param>
        public async Task BroadcastRumorAsync(Peer originPeer, int bounce, string user, int msgID, DateTime date, string msg)
        {

            // Increment our bounce value
            bounce++;

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
            int bounce = 1;
            string user = Username;
            int msgID;
            lock (AllUsers.All)
            {
                msgID = AllUsers.All[Username].NextExpectedMessageID;
            }
            DateTime date = DateTime.UtcNow;
            string msg = message;
            AllUsers.NewMessage(null, 0, user, msgID, date, msg);


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


        /// <summary>
        /// Sends a new PM we created to the correct destination
        /// </summary>
        /// <param name="destination">destination user</param>
        /// <param name="content">PM content</param>
        /// <returns>void Task</returns>
        public async Task SendMyPMAsync(string destination, string content)
        {

            // Return if we don't know this user
            if (destination == Username || AllUsers.All.ContainsKey(destination) == false)
            {
                Console.WriteLine("We do not know user {0}", destination);
                return;
            }

            // Return if we don't know the destination to this user
            if (AllUsers.All[destination].LatestPeer == null)
            {
                Console.WriteLine("We do not know a path to {0}", destination);
                return;
            }

            // Prepare data
            string origin = Username;
            DateTime date = DateTime.UtcNow;

            // Await for result
            await AllUsers.All[destination].LatestPeer.SendPMAsync(origin, destination, date, content);
        }


        /// <summary>
        /// Transmit a PM
        /// </summary>
        /// <param name="peer">who transmitted the PM to us</param>
        /// <param name="origin">origin of the PM</param>
        /// <param name="destination">destination of the PM</param>
        /// <param name="date">date and time of creation</param>
        /// <param name="content">content of PM</param>
        /// <returns>void Task</returns>
        public async Task SendPMAsync(Peer peer, string origin, string destination, DateTime date, string content)
        {

            // Return if we don't know this user
            if (destination == Username || AllUsers.All.ContainsKey(destination) == false)
            {
                return;
            }

            // Return if we don't know the destination to this user
            if (AllUsers.All[destination].LatestPeer == null)
            {
                return;
            }

            // Await for result
            await AllUsers.All[destination].LatestPeer.SendPMAsync(origin, destination, date, content);
        }
    }
}

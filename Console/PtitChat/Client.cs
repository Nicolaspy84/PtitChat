using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
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
            Username = username;
            Port = port;
            messages = new Messages(this);
        }


        /// <summary>
        /// Username shared with other clients (will be associated with our ip address)
        /// </summary>
        public readonly string Username;


        /// <summary>
        /// Client port for other peers to connect to us
        /// </summary>
        public readonly int Port;


        /// <summary>
        /// Dictionary to hold connected peers
        /// The key is the name of the peer
        /// </summary>
        public Dictionary<string, TcpClient> Peers = new Dictionary<string, TcpClient>();

        public Messages messages;


        /// <summary>
        /// Connects to the designated peer
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

            // Connection successful, we exchange usernames
            Console.WriteLine("Connected to {0}", ipAddress);
            UsernameExchange(client);

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


                // We create a thread to start listening at the channels
                Thread thread = new Thread(new ParameterizedThreadStart(UsernameExchange));
                thread.Start(client);

            }
        }


        /// <summary>
        /// This method takes care of the exchange of usernames
        /// </summary>
        /// <param name="oClient">TcpClient we are connecting to</param>
        public void UsernameExchange(object oClient)
        {
            TcpClient client;
            try
            {
                client = (TcpClient)oClient;
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e);
                return;
            }

            // Prepare streams for communication
            StreamReader sr = new StreamReader(client.GetStream());
            StreamWriter sw = new StreamWriter(client.GetStream());

            try
            {
                // Write our information and flush it
                sw.WriteLine("USERNAME:" + Username);
                sw.Flush();

                // Read peer's username
                string data;
                while (true)
                {
                    data = sr.ReadLine();
                    if (string.IsNullOrEmpty(data) == false) break;
                }
                Console.WriteLine("Received <{0}> from {1}", data, client.Client.RemoteEndPoint);
                string[] dataList = data.Split(':');

                // Check that we have USERNAME data
                if (dataList.Length == 2 && dataList[0] == "USERNAME")
                {
                    Console.WriteLine("Succesfully connected to {0} at {1}", dataList[1], client.Client.RemoteEndPoint);
                    AddPeer(dataList[1], client);
                    MessageExchange(dataList[1]);
                }
                else
                {
                    Console.WriteLine("Received {0} from {1} but we were expecting a USERNAME:<USERNAME> answer, closing communication with this client", data, client.Client.RemoteEndPoint);
                    client.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                client.Close();
                return;
            }
        }


        /// <summary>
        /// Method dealing with messages from the given peer
        /// </summary>
        /// <param name="username">string key corresponding to a TcpClient in the Peers dictionnary</param>
        public void MessageExchange(string username)
        {

            // Create a stream reader
            StreamReader sr;
            lock (Peers)
            {
                sr = new StreamReader(Peers[username].GetStream());
            }

            try
            {
                while (true)
                {

                    // Wait for data to arrive
                    string data;
                    while (true)
                    {
                        data = sr.ReadLine();
                        if (string.IsNullOrEmpty(data) == false) break;
                    }


                    // Switch over message type
                    string[] dataList = data.Split(':');
                    if (dataList.Length == 3 && dataList[0] == "BROADCAST")
                    {
                        Console.WriteLine("{0} : {1}", username, dataList[2]);

                        // display an error if we missed some of the messages
                        if ((messages.GetLastMessageNb(username) + 1).ToString() != dataList[1]) //look if lastIdReceived + 1 = idReceived
                        {
                            Console.WriteLine("We missed some messages of {0}", username);
                        }

                        // add received message to messages dictionary
                        messages.AddPeerMessage(dataList[0], dataList[1], dataList[2]);
                    }
                    else if (dataList.Length > 0 && dataList[0] == "QUIT")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Received <{0}> from {1}", data, username);
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            lock (Peers)
            {

                // Disconnect
                Console.WriteLine("Communication ended with {0}", username);
                if (Peers[username] != null)
                {
                    Peers[username].Close();
                    Peers[username].Dispose();
                    Peers[username] = null;
                }

            }
        }


        /// <summary>
        /// Message to broadcast to all known peers
        /// </summary>
        /// <param name="message">string to broadcast</param>
        public void BroadcastMessage(string message)
        {
            lock (Peers)
            {
                List<string> setToNull = new List<string>();
                foreach (var peer in Peers)
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(peer.Value.GetStream());
                        sw.WriteLine("BROADCAST:"+ messages.IdMessage.ToString() + ":" + message);
                        sw.Flush();
                    }
                    catch (Exception e)
                    {
                        if (peer.Value == null)
                        {
                            Console.WriteLine("({0} is disconnected)", peer.Key);
                        }
                        else if (peer.Value.Connected == false)
                        {
                            Console.WriteLine("User {0} appears to be disconnected, ending communication", peer.Key);
                            peer.Value.Dispose();
                            peer.Value.Close();
                            setToNull.Add(peer.Key);
                        }
                        else
                        {
                            Console.WriteLine(e);
                        }
                    }
                    // add our sended message to the messages dictionary
                    messages.AddMyMessage(message);
                }
                foreach (var key in setToNull)
                {
                    Peers[key] = null;
                }
            }
        }


        /// <summary>
        /// Adds a peer to our dictionnary (takes care of the lock)
        /// It will simply update the TcpClient reference if the key already exists
        /// </summary>
        /// <param name="username">string username of the client</param>
        /// <param name="client">TcpClient reference</param>
        public void AddPeer(string username, TcpClient client)
        {
            lock (Peers)
            {
                if (Peers.ContainsKey(username))
                {
                    Peers[username] = client;
                }
                else
                {
                    Peers.Add(username, client);
                }
            }
        }
    }
}

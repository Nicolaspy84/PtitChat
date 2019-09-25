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
                    lock (Peers)
                    {
                        Peers.Add(dataList[1], client);
                    }
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
            // Do some stuff...


            // Close communication
            lock (Peers)
            {
                Console.WriteLine("Communication ended with {0} at {1}", username, Peers[username].Client.RemoteEndPoint);
                Peers[username].Close();
                Peers.Remove(username);
            }
        }
    }
}

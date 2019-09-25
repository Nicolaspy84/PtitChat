using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        public Dictionary<string, TcpClient> Peers;

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

            // Connection successful, ask for username
            Console.WriteLine("Connected to {0}", ipAddress);



        }

        /// <summary>
        /// Once this method is called, the client will listen on its port for incoming connections
        /// </summary>
        public void ListenForConnections()
        {

            // Create a listener on our port
            TcpListener Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();


            // We loop infinitely
            while (true)
            {

                // We wait for a client to connect to us
                TcpClient client = Listener.AcceptTcpClient();


                // Once we register a client, we create a thread to send him a request
                lock (Peers) Peers.Add(count, client);
                Console.WriteLine("Someone connected!!");

                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
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
            }


        }
    }
}

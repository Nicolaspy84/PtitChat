using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace PtitChat
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            InitiateNetworking();
        }

        // This method starts the application's networking system (this is the entry point of the code)
        public static void InitiateNetworking()
        {

            // Ask for a unique username
            Console.WriteLine("Please enter a unique username :");
            string username = Console.ReadLine();


            // Ask for a port to communicate through
            int port = -1;
            while (port < 0)
            {
                Console.WriteLine("Please enter a valid port for your client :");
                try
                {
                    port = Int32.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.WriteLine("Couldn't parse string to int, please try again.");
                }
            }


            // We instanciate our custom client class
            Client MyClient = new Client(username, port);


            // Ask the user if he knows peer adresses and if he'd like to connect to them
            Console.WriteLine("Would you like to add peer addresses ? (O/N)");
            string answer = Console.ReadLine();


            // If we know peers, we try to connect to them
            if (answer == "O" || answer == "o")
            {
                Console.WriteLine("Specify addresses you would like to connect to (separate with spaces) :");
                string[] ipAddresses = Console.ReadLine().Split(' ');
                foreach (var ipAddress in ipAddresses)
                {
                    Thread thread = new Thread(MyClient.ConnectToPeer);
                    thread.Start(ipAddress);
                }
            }


            // We start listening to other peers

        }
    }
}

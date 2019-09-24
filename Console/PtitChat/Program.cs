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


            // We instantiate our custom client class
            Client MyClient = new Client(username);


            // 
            Console.WriteLine("Would you like to connect to add peer addresses ? (O/N)");
            string answer = Console.ReadLine();

            if (answer == "O" || answer == "o")
            {

            }
        }
    }
}

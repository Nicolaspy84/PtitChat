using System;
using System.Threading;
using System.Threading.Tasks;

namespace PtitChat
{
    class MainClass
    {
      
        /// <summary>
        /// Entry point of the program
        /// </summary>
        /// <param name="args">cmd line arguments (flags)</param>
        public static void Main(string[] args)
        {

            // Prepare arguments
            string usernameArg = null;
            string portArg = null;
            string peersArg = null;


            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-username":
                        if (i + 1 < args.Length)
                        {
                            usernameArg = args[i + 1];
                            i++;
                        }
                        break;
                    case "-port":
                        if (i + 1 < args.Length)
                        {
                            portArg = args[i + 1];
                            i++;
                        }
                        break;
                    case "-peers":
                        if (i + 1 < args.Length && args[i + 1][0] != '-')
                        {
                            peersArg = "";
                            while (i + 1 < args.Length && args[i + 1][0] != '-')
                            {
                                peersArg += args[i + 1] + " ";
                                i++;
                            }
                            peersArg = peersArg.Remove(peersArg.Length - 1);
                        }
                        break;
                    default:
                        Console.WriteLine("Did not recognize flag {0}", args[i]);
                        break;
                }
            }


            // Initiate networking
            InitiateNetworking(usernameArg, portArg, peersArg);
        }


        /// <summary>
        /// Creates and starts the client (initiates threads to read/write messages)
        /// </summary>
        /// <param name="usernameArg">default username</param>
        /// <param name="portArg">default port</param>
        /// <param name="peersArg">default peers' addresses to conenct to (separate with spaces)</param>
        public static void InitiateNetworking(string usernameArg, string portArg, string peersArg)
        {

            // Ask for a unique username
            Console.WriteLine("Please enter a unique username :");
            if (string.IsNullOrEmpty(usernameArg))
            {
                usernameArg = Console.ReadLine();
            }
            else
            {
                Console.WriteLine(usernameArg);
            }

            // Ask for a port to communicate through
            int port = -1;
            while (port < 0)
            {
                Console.WriteLine("Please enter a valid port for your client :");
                if (string.IsNullOrEmpty(portArg))
                {
                    portArg = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine(portArg);
                }
                try
                {
                    port = Int32.Parse(portArg);
                }
                catch (FormatException)
                {
                    portArg = null;
                    Console.WriteLine("Couldn't parse string to int, please try again.");
                }
            }


            // We instanciate our custom client class
            Client myClient = new Client(usernameArg, port);


            // Ask the user if he knows peer adresses and if he'd like to connect to them
            Console.WriteLine("Would you like to add peer addresses ? (O/N)");
            string answer = "O";
            if (string.IsNullOrEmpty(peersArg))
            {
                answer = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("O");
            }


            // If we know peers, we try to connect to them
            if (answer == "O" || answer == "o")
            {
                Console.WriteLine("Specify addresses you would like to connect to (separate with spaces) :");
                if (string.IsNullOrEmpty(peersArg))
                {
                    peersArg = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine(peersArg);
                }
                string[] ipAddresses = peersArg.Split(' ');

                foreach (var ipAddress in ipAddresses)
                {
                    Thread th = new Thread(new ParameterizedThreadStart(myClient.ConnectToPeer));
                    th.Start(ipAddress);
                }
            }


            // We start listening to other peers
            Thread thread = new Thread(new ThreadStart(myClient.ListenForConnections));
            thread.Start();


            // We wait for user inputs
            while (true)
            {
                string request = Console.ReadLine();
                switch (request)
                {
                    case "/peers":
                        Console.WriteLine(myClient);
                        break;
                    case "/users":
                        Console.WriteLine(User.AllToString());
                        break;
                    default:
                        Task.Run(() => myClient.BroadcastMyMessageAsync(request));
                        break;
                }
            }
        }
    }
}

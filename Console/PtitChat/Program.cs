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
            Console.WriteLine("Voulez vous vous connecter ? (O/N)");
            string reponse = Console.ReadLine();
            if (reponse == "o" || reponse == "O")
            {
                Console.WriteLine("Rentrez une adresse ip et un port");
                string IpAdress = Console.ReadLine();
                TcpClient serveur = new TcpClient(IpAdress, 1302);
            }
            if (reponse == "n" || reponse == "N")
                Console.WriteLine("ok");
            else
            {
                Console.WriteLine("Erreur");
                return;
            }
            Thread thread = new Thread(WaitConnexion);
            thread.Start();
                

        }

        public static void WaitConnexion()
        {
            TcpListener listener = new TcpListener(1302);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connecté");
        }
    }
}

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
            Thread thread = new Thread(WaitConnexion);
            thread.Start();
            Console.Write("Vous êtes connecté");
            Console.WriteLine("Voulez vous vous connecter à quelqu'un ? (O/N)");
            string reponse = Console.ReadLine();
            if (reponse == "o" || reponse == "O")
            {
                Console.WriteLine("Rentrez une adresse ip");
                string IpAdress = Console.ReadLine();
                TcpClient serveur = new TcpClient(IpAdress, 1302);
            }
            if (reponse == "n" || reponse == "N")
                Console.WriteLine("ok");
            else
            {
                Console.WriteLine("Il faut répondre o ou n boloss");
                return;
            }
        }

        public static void WaitConnexion()
        {
            TcpListener listener = new TcpListener(1302);
            listener.Start();
            Console.WriteLine("En attente de quelqu'un");
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Quelqu'un s'est connecté à vous !");
        }
    }
}

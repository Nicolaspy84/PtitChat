using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace PtitChat
{
    class MainClass
    {
        public static List<TcpClient> Client = new List<TcpClient>();
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
                Thread thread1 = new Thread(() => Ecouter(serveur));
                thread1.Start();
            }
            else if (reponse == "n" || reponse == "N")
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
            Client.Add(client);
            Console.WriteLine("Quelqu'un s'est connecté à vous !");
            Thread thread2 = new Thread(() => Ecouter(client));
            thread2.Start();
            var stream = client.GetStream();
            stream.Write(Encoding.UTF8.GetBytes("Hello"), 0, 5);
        }

        public static void Ecouter(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[8];
            while (true)
            {
                try
                {
                    stream.Read(buffer, 0, 5);
                    Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, 5));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        
    }
}

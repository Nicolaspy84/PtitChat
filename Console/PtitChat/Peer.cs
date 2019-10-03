using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PtitChat
{

    /// <summary>
    /// A peer is another node connected to the network (it is NOT a user)
    /// </summary>
    public class Peer
    {

        /// <summary>
        /// Delegate to handle received messages events
        /// </summary>
        /// <param name="peer">origin of the message (who sent it to us)</param>
        /// <param name="bounce">Number of times this message has to be broadcasted</param>
        /// <param name="user">User who originally sent this message</param>
        /// <param name="msgID">Unique message ID</param>
        /// <param name="date">Message date and time origin</param>
        /// <param name="msg">Message content</param>
        public delegate Task MessageReceived(Peer peer, int bounce, string user, int msgID, DateTime date, string msg);


        /// <summary>
        /// Event called when a message is received by any of our Peers
        /// </summary>
        public static event MessageReceived MessageReceivedEvent;


        /// <summary>
        /// A peer has a client (it should already be connected)
        /// </summary>
        /// <param name="client">Connected TcpClient</param>
        public Peer(TcpClient client)
        {
            Client = client;

            try
            {
                sr = new StreamReader(Client.GetStream());
                sw = new StreamWriter(Client.GetStream());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        /// <summary>
        /// Closes the network streams, the client, disposes the client and sets it to null
        /// </summary>
        public void Dispose()
        {
            if (Client != null)
            {
                sr.Close();
                sw.Close();
                Client.Close();
                Client.Dispose();
                Client = null;
            }
        }


        /// <summary>
        /// Returns a description of this Peer instance
        /// </summary>
        /// <returns>string description</returns>
        public override string ToString()
        {
            if (Client == null)
            {
                return "Peer address <null>";
            }
            return string.Format("Peer address <{0}>", Client.Client.RemoteEndPoint);
        }


        /// <summary>
        /// The main tcp client
        /// </summary>
        public TcpClient Client;


        /// <summary>
        /// Network stream reader
        /// </summary>
        private readonly StreamReader sr;


        /// <summary>
        /// Network stream writer
        /// </summary>
        private readonly StreamWriter sw;


        /// <summary>
        /// Broadcasts async. a message to this peer
        /// </summary>
        /// <param name="bounce">Number of times this message has to broadcast</param>
        /// <param name="user">User who originally sent this message</param>
        /// <param name="msgID">Unique message ID</param>
        /// <param name="date">Message date and time origin</param>
        /// <param name="msg">Message content</param>
        /// <returns></returns>
        public async Task BroadcastAsync(int bounce, string user, int msgID, DateTime date, string msg)
        {
            try
            {
                string toSend = string.Format("{0}#{1}#{2}#{3}#{4}#{5}", "BROADCAST", bounce, user, msgID, date, msg);
                await sw.WriteLineAsync(toSend);
                await sw.FlushAsync();
            }
            catch (Exception)
            {
                Dispose();
            }
        }


        /// <summary>
        /// Execute this method to start listening to this peer
        /// </summary>
        /// <returns>empty task</returns>
        public async Task ListenAsync()
        {

            // Loop indefinitely to take care of all data
            while (true)
            {

                // Await for data to arrive
                string data;
                try
                {
                    data = await sr.ReadLineAsync();
                }
                catch (Exception)
                {
                    break;
                }

                // Split data
                string[] dataList = data.Split('#');

                // Broadcast message of type BROADCAST:BOUNCE:USERNAME:ID:DATE:CONTENT
                if (dataList.Length == 6 && dataList[0] == "BROADCAST")
                {

                    // Parse data
                    int bounce;
                    string user;
                    int msgID;
                    DateTime date;
                    string msg;
                    try
                    {
                        bounce = Int32.Parse(dataList[1]);
                        user = dataList[2];
                        msgID = Int32.Parse(dataList[3]);
                        date = DateTime.Parse(dataList[4]);
                        msg = dataList[5];
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could not parse <{0}> from {1}", data, Client.Client.RemoteEndPoint);
                        continue;
                    }

                    // Add data info to the user list and print info
                    User.NewMessage(this, user, msgID, date, msg);
                    Console.WriteLine("{0}({1}) @<{2}> : {3}", user, msgID, date, msg);

                    // Notify our message arrival
                    MessageReceivedEvent(this, bounce, user, msgID, date, msg);
                }

                // Disconnect instruction
                else if (dataList.Length == 1 && dataList[0] == "DISCONNECT")
                {
                    break;
                }

                // Unknown instruction
                else
                {
                    Console.WriteLine("Could not process <{0}> from {1}", data, Client.Client.RemoteEndPoint);
                }
            }


            // Dispose of client
            Console.WriteLine("Communication ended with {0}", Client.Client.RemoteEndPoint);
            Dispose();
        }
    }
}

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

        public const string RUMOR = "RUMOR";
        public const string UPDATE = "UPDATE";
        public const string DISCONNECT = "DISC";
        public const string PRIVATEMSG = "PM";


        /// <summary>
        /// Delegate to handle received (new) rumors events
        /// </summary>
        /// <param name="peer">Origin of the message (who sent it to us)</param>
        /// <param name="bounce">Number of times this message has been broadcasted</param>
        /// <param name="user">User who originally sent this message</param>
        /// <param name="msgID">Unique message ID</param>
        /// <param name="date">Message date and time origin</param>
        /// <param name="msg">Message content</param>
        /// <returns>void Task</returns>
        public delegate Task RumorReceived(Peer peer, int bounce, string user, int msgID, DateTime date, string msg);


        /// <summary>
        /// Event called when a rumor is received by any of our Peers
        /// </summary>
        public static event RumorReceived RumorReceivedEvent;


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
                Console.WriteLine("Disposing of {0}", Client.Client.RemoteEndPoint);
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
        /// Send the given packet to this peer
        /// </summary>
        /// <param name="packet">The complete string content (packet)</param>
        /// <returns>void Task</returns>
        public async Task SendPacketAsync(string packet)
        {
            await sw.WriteLineAsync(packet);
            await sw.FlushAsync();
        }


        /// <summary>
        /// Requests a full update from this peer
        /// </summary>
        /// <returns>void Task</returns>
        public async Task RequestUpdateAsync()
        {
            await SendPacketAsync(UPDATE);
        }


        /// <summary>
        /// Sends a rumor to this peer async.
        /// </summary>
        /// <param name="bounce">Number of times this rumor has been broadcasted</param>
        /// <param name="user">User who originally sent this rumor</param>
        /// <param name="msgID">Unique rumor ID</param>
        /// <param name="date">Rumor date and time origin</param>
        /// <param name="msg">Rumor content</param>
        /// <returns></returns>
        public async Task SendRumorAsync(int bounce, string user, int msgID, DateTime date, string msg)
        {
            string toSend = string.Format("{0}#{1}#{2}#{3}#{4}#{5}", RUMOR, bounce, user, msgID, date, msg);
            await SendPacketAsync(toSend);
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
                    // Console.WriteLine("IN => {0}", data);
                }
                catch (Exception)
                {
                    break;
                }

                // Split data
                string[] dataList = data.Split('#');

                // Rumor message
                if (dataList.Length == 6 && dataList[0] == RUMOR)
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

                    // Add data info to the user list
                    bool newMsg = User.NewMessage(this, bounce, user, msgID, date, msg);
                    if (newMsg)
                    {

                        // Print new message arrival
                        Console.WriteLine("{0}({1}) @<{2}> : {3}", user, msgID, date, msg);

                        // Notify our new message arrival
                        RumorReceivedEvent(this, bounce, user, msgID, date, msg);

                    }

                }

                // Request update
                else if (dataList.Length == 1 && dataList[0] == UPDATE)
                {

                    // Loop to send every single rumor we know so far
                    foreach (var packet in User.GetRumorList())
                    {
                        await SendPacketAsync(packet);
                    }

                }

                // Disconnect request
                else if (dataList.Length == 1 && dataList[0] == DISCONNECT)
                {
                    break;
                }

                // Unknown request
                else
                {
                    Console.WriteLine("Could not process <{0}> from {1}", data, Client.Client.RemoteEndPoint);
                }
            }


            // Dispose of client
            Console.WriteLine("Communication ended with {0}, can't listen async.", Client.Client.RemoteEndPoint);
            Dispose();
        }
    }
}

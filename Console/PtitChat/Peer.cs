using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
        public const string FILETRANSFER = "FT";


        public const int CHUNKSIZE = 8192;
        public const int DELAY = 5;


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
        /// Delegate to handle received PMs events
        /// </summary>
        /// <param name="peer">origin of the PM (who transmitted it to us)</param>
        /// <param name="origin">origin of the PM</param>
        /// <param name="destination">destination of the PM</param>
        /// <param name="date">PM date and time</param>
        /// <param name="content">content of the PM</param>
        /// <returns>void Task</returns>
        public delegate Task PMReceived(Peer peer, string origin, string destination, DateTime date, string content);


        /// <summary>
        /// Delegate to handle received file chunks received events
        /// </summary>
        /// <param name="peer">origin of the message on the network</param>
        /// <param name="origin">origin of the file chunk</param>
        /// <param name="destination">destination of the file chunk</param>
        /// <param name="fileName">file name</param>
        /// <param name="bufferSize">data packet buffer size in bytes</param>
        /// <param name="chunkID">chunk ID</param>
        /// <param name="nbChunks">total number of chunks for this file</param>
        /// <param name="chunkData">byte array containing data</param>
        /// <returns>void Task</returns>
        public delegate Task FileChunkReceived(Peer peer, string origin, string destination, string fileName, int bufferSize, long chunkID, long nbChunks, byte[] chunkData);


        /// <summary>
        /// Event called when a rumor is received by any of our Peers
        /// </summary>
        public static event RumorReceived RumorReceivedEvent;


        /// <summary>
        /// Event called whenever we receive a PM
        /// </summary>
        public static event PMReceived PMReceivedEvent;


        /// <summary>
        /// Eveent called whenever we receive a file chunk
        /// </summary>
        public static event FileChunkReceived FileChunkReceivedEvent;


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
        /// Send the given packet to this peer
        /// </summary>
        /// <param name="packet">The complete string content (packet)</param>
        /// <returns>void Task</returns>
        public async Task SendPacketAsync(string packet)
        {
            byte[] packetArray = Encoding.UTF8.GetBytes(packet + "\n");
            await Client.GetStream().WriteAsync(packetArray, 0, packetArray.Length);
            await Client.GetStream().FlushAsync();
        }


        /// <summary>
        /// Send the given packet with header and chunk to this peer
        /// </summary>
        /// <param name="header">Header</param>
        /// <param name="chunk">Array of bytes (a chunk) to send as a packet</param>
        /// <returns>void Task</returns>
        public async Task SendPacketAsync(string header, byte[] chunk)
        {
            // Send header
            await SendPacketAsync(header);

            // Pause before we send the chunk
            System.Threading.Thread.Sleep(DELAY);

            // Send chunk
            await Client.GetStream().WriteAsync(chunk, 0, chunk.Length);
            await Client.GetStream().FlushAsync();

            // Notify
            Console.WriteLine("SENT {0} with chunk of size {1} bytes", header, chunk.Length);
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
        /// Sends the provided file to the destination through this peer (we assume the file exists on the disk)
        /// </summary>
        /// <param name="origin">origin of the file</param>
        /// <param name="destination">destination of the file</param>
        /// <param name="filePath">complete file path with file name and extension</param>
        /// <returns>void Task</returns>
        public async Task SendFileAsync(string origin, string destination, string filePath)
        {
            // First we get the name of the file
            string[] filePathSplit = filePath.Split('/');
            string fileName = filePathSplit[filePathSplit.Length - 1];

            // Current chunk ID
            long chunkID = 0;

            // Open the file as a file stream and put our pointer at the beginning
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            long fsPointer = 0;
            long nbChunks = (int)Math.Ceiling((float)fs.Length / (float)CHUNKSIZE);

            // For as as long as we are not at the end
            while (fsPointer != fs.Length)
            {
                // Compute the remaining buffer size
                int bufferSize = (int)(fs.Length - fsPointer < CHUNKSIZE ? fs.Length - fsPointer : CHUNKSIZE);

                // Create a buffer at the right size
                byte[] buffer = new byte[bufferSize];
                fs.Seek(fsPointer, SeekOrigin.Begin);
                fs.Read(buffer, 0, bufferSize);

                // Finally we can send data
                await SendFileChunkAsync(origin, destination, fileName, bufferSize, chunkID, nbChunks, buffer);

                // Pause before we send the next chunk
                System.Threading.Thread.Sleep(DELAY);

                // Update fsPointer and chunk ID
                fsPointer += bufferSize;
                chunkID++;
            }
        }


        /// <summary>
        /// Sends the file chunk to destination through this peer async.
        /// </summary>
        /// <param name="origin">origin of the file chunk</param>
        /// <param name="destination">destination of the file chunk</param>
        /// <param name="fileName">file name</param>
        /// <param name="bufferSize">data packet buffer size in bytes</param>
        /// <param name="chunkID">chunk ID</param>
        /// <param name="nbChunks">total number of chunks for this file</param>
        /// <param name="chunkData">byte array containing data</param>
        /// <returns></returns>
        public async Task SendFileChunkAsync(string origin, string destination, string fileName, int bufferSize, long chunkID, long nbChunks, byte[] chunkData)
        {
            // Now we can prepare our header
            string headerToSend = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}", FILETRANSFER, origin, destination, fileName, bufferSize, chunkID, nbChunks);

            // Finally we can send data
            await SendPacketAsync(headerToSend, chunkData);
        }


        /// <summary>
        /// Sends a PM from origin to destination through this peer async.
        /// </summary>
        /// <param name="origin">origin of the message</param>
        /// <param name="destination">destination of the message</param>
        /// <param name="date">date of creation of the message</param>
        /// <param name="content">content of the message</param>
        /// <returns></returns>
        public async Task SendPMAsync(string origin, string destination, DateTime date, string content)
        {
            string toSend = string.Format("{0}#{1}#{2}#{3}#{4}", PRIVATEMSG, origin, destination, date, content);
            await SendPacketAsync(toSend);
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
                    bool newMsg = AllUsers.NewMessage(this, bounce, user, msgID, date, msg);
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
                    foreach (var packet in AllUsers.GetRumorList())
                    {
                        await SendPacketAsync(packet);
                    }

                }

                // PM
                else if (dataList.Length == 5 && dataList[0] == PRIVATEMSG)
                {

                    // Parse data
                    string origin = dataList[1];
                    string destination = dataList[2];
                    DateTime date;
                    string content = dataList[4];
                    try
                    {
                        date = DateTime.Parse(dataList[3]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could not parse <{0}> from {1}", data, Client.Client.RemoteEndPoint);
                        continue;
                    }

                    // Check if we are the destination of the PM
                    if (destination == PtitChat.Client.Username)
                    {
                        // Notify the arrival
                        Console.WriteLine("{0}(PM) @<{1}> : {2}", origin, date, content);

                        // Add this user
                        AllUsers.AddPotentialNewUser(this, origin);

                        // Add this new PM to our PM dict
                        AllUsers.All[PtitChat.Client.Username].AddPrivateMessage(origin, date, content);
                    }

                    // PM receive event
                    PMReceivedEvent(this, origin, destination, date, content);
                    
                }

                // File chunk
                else if (dataList.Length == 7 && dataList[0] == FILETRANSFER)
                {
                    // Parse data
                    string origin = dataList[1];
                    string destination = dataList[2];
                    string fileName = dataList[3];
                    int bufferSize;
                    long chunkID;
                    long nbChunks;
                    try
                    {
                        bufferSize = int.Parse(dataList[4]);
                        chunkID = long.Parse(dataList[5]);
                        nbChunks = long.Parse(dataList[6]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could not parse <{0}> from {1}", data, Client.Client.RemoteEndPoint);
                        continue;
                    }

                    // Now we can wait for the data chunk
                    byte[] chunkData = new byte[bufferSize];
                    Client.GetStream().Read(chunkData, 0, bufferSize);

                    // Check if we are the destination of the file transfer
                    if (destination == PtitChat.Client.Username)
                    {
                        // Notify the arrival
                        Console.WriteLine("{0}(ID:{1}) {2}", origin, chunkID, fileName);

                        // If we recontruct the file properly, notify it
                        if (AllFiles.NewChunk(origin, fileName, nbChunks, chunkID, chunkData))
                        {
                            Console.WriteLine("Reconstructed {0} properly", fileName);
                        }
                    }

                    // Transmit the message
                    FileChunkReceivedEvent(this, origin, destination, fileName, bufferSize, chunkID, nbChunks, chunkData);

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

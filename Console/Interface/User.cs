using System;
using System.Collections.Generic;

namespace Interface
{

    /// <summary>
    /// A user of our network (has a unique username and holds a list of messages exchanged with us)
    /// </summary>
    public class User
    {

        /// <summary>
        /// The default constructor requires a username
        /// </summary>
        /// <param name="username">unique username of the user</param>
        /// <param name="initPMs">set it to true to initialize reception of PMs (default is false)</param>
        public User(string username, bool initPMs = false)
        {
            Username = username;
            NextExpectedMessageID = 0;
            if (initPMs)
            {
                PrivateMessages = new Dictionary<string, List<Tuple<DateTime, string>>>();
            }
        }


        /// <summary>
        /// Returns all messages received from this user
        /// </summary>
        /// <returns>Descriptive string</returns>
        public override string ToString()
        {
            string endStr = "";
            endStr += string.Format("From {0} via {1} at {2} nodes away :\n", Username, LatestPeerIPString(), Distance);
            foreach (var msg in Messages)
            {
                endStr += string.Format("   ({0}) @<{1}> -> {2}\n", msg.Key, msg.Value.Item1, msg.Value.Item2);
            }
            endStr += string.Format("End from {0}", Username);
            return endStr;
        }


        /// <summary>
        /// Unique username in the network (unexpected results if 2 users have the same username)
        /// </summary>
        public string Username;


        /// <summary>
        /// Dictionary holding all the messages broadcasted by this user,
        /// together with the initial broadcast date and time
        /// (the key corresponds to the unique ID of the message)
        /// </summary>
        public Dictionary<int, Tuple<DateTime, string>> Messages = new Dictionary<int, Tuple<DateTime, string>>();


        /// <summary>
        /// Dictionary only initialized for ourself (where Username == Client.Username).
        /// The key is the username of the user who sent us this pm.
        /// The value is a list of tuples of date time and string to store every pm.
        /// </summary>
        public Dictionary<string, List<Tuple<DateTime, string>>> PrivateMessages;


        /// <summary>
        /// This holds the next message ID we are expecting from this user.
        /// If we have message 0, 1, 2, 3, 6, 7 and 10, NextExpectedMessageID=4
        /// </summary>
        public int NextExpectedMessageID;


        /// <summary>
        /// Latest peer which transmitted a message from this user.
        /// Contact this peer to send a direct message to this user.
        /// </summary>
        public Peer LatestPeer;


        /// <summary>
        /// Returns the IP address of our LatestPeer as a string
        /// </summary>
        /// <returns>Returns null if no LatestPeer is known (or it is disconnected)</returns>
        public string LatestPeerIPString()
        {
            string returnStr;
            try
            {
                returnStr = LatestPeer.Client.Client.RemoteEndPoint.ToString();
            }
            catch (Exception)
            {
                returnStr = "null";
            }
            return returnStr;
        }


        /// <summary>
        /// This holds the distance to the peer
        /// </summary>
        public int Distance;


        /// <summary>
        /// Call this method when a new message is received from this user
        /// </summary>
        /// <param name="messageID">Unique message ID of this message</param>
        /// <param name="dateTime">Date and time of original broadcasting of the message</param>
        /// <param name="message">Content of the message</param>
        /// <returns>true if the message is a new one</returns>
        public bool AddMessage(int messageID, DateTime dateTime, string message)
        {
            if (Messages.ContainsKey(messageID) == false)
            {
                Tuple<DateTime, string> data = new Tuple<DateTime, string>(dateTime, message);
                Messages.Add(messageID, data);
                AllUsers.OnAllChanged(EventArgs.Empty);
                AllUsers.NbMessages++;
                while (true)
                {
                    if (Messages.ContainsKey(NextExpectedMessageID))
                    {
                        NextExpectedMessageID++;
                    }
                    else
                    {
                        break;
                    }
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Call this method when we receive a new PM
        /// </summary>
        /// <param name="user">the origin of the PM</param>
        /// <param name="dateTime">the date and time of the creation of the message</param>
        /// <param name="content">PM content</param>
        public void AddPrivateMessage(string user, DateTime dateTime, string content)
        {
            // Check that our dictionary has been init properly
            if (PrivateMessages == null)
            {
                Console.WriteLine("ERROR : when receiving a new PM, user {0} does not have an initialized PM dictionary", Username);
                return;
            }

            // Create a new list if the key does not exist
            if (PrivateMessages.ContainsKey(user) == false)
            {
                PrivateMessages.Add(user, new List<Tuple<DateTime, string>>());
            }

            // Now we can safely add this PM to our list
            PrivateMessages[user].Add(new Tuple<DateTime, string>(dateTime, content));
        }
    }
}

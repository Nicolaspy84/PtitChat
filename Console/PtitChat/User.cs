using System;
using System.Collections.Generic;

namespace PtitChat
{

    /// <summary>
    /// A user of our network (has a unique username and holds a list of messages exchanged with us)
    /// </summary>
    public class User
    {

        /// <summary>
        /// Static dictionary holding all instances of the User class
        /// </summary>
        public static Dictionary<string, User> All = new Dictionary<string, User>();


        /// <summary>
        /// Total number of messages received/saved
        /// </summary>
        public static int NbMessages;


        /// <summary>
        /// Dispatches the message to the correct User instance
        /// </summary>
        /// <param name="origin">Peer origin of the message (if known)</param>
        /// <param name="bounce">Number of times this message has been broadcasted to arrive to us</param>
        /// <param name="user">Username</param>
        /// <param name="msgID">Unique message ID</param>
        /// <param name="date">Date and time when the message was created</param>
        /// <param name="msg">Message content</param>
        /// <returns>true if this is an unseen message</returns>
        public static bool NewMessage(Peer origin, int bounce, string user, int msgID, DateTime date, string msg)
        {
            lock(All)
            {

                // Create a new user if this one is unknown
                if (All.ContainsKey(user) == false)
                {
                    All[user] = new User(user);
                }

                // Add the message to our dictionary, and if this message is a new one, update our LatestPeer
                if (All[user].AddMessage(msgID, date, msg) && origin != null)
                {
                    All[user].LatestPeer = origin;
                    All[user].Distance = bounce;
                    return true;
                }

            }

            return false;
        }


        /// <summary>
        /// Returns a string describing all user data
        /// </summary>
        /// <returns>Description string</returns>
        public static string AllToString()
        {
            string endStr = "";
            lock (All)
            {
                foreach (var user in All)
                {
                    endStr += string.Format("{0}\n", user.Value);
                }
                if (All.Count > 0)
                {
                    endStr = endStr.Remove(endStr.Length - 1);
                }
            }
            return endStr;
        }


        /// <summary>
        /// Returns all the messages we've received so far (correctly formatted to braodcast them).
        /// Note : no lock used here (we don't know if it works yet)
        /// </summary>
        /// <returns>The list of strings</returns>
        public static List<string> GetRumorList()
        {
            List<string> rumorList = new List<string>();
            foreach (var user in All)
            {
                foreach (var msg in user.Value.Messages)
                {
                    rumorList.Add(string.Format("{0}#{1}#{2}#{3}#{4}#{5}", Peer.RUMOR, user.Value.Distance, user.Key, msg.Key, msg.Value.Item1, msg.Value.Item2));
                }
            }
            return rumorList;
        }


        /// <summary>
        /// The default constructor requires a username
        /// </summary>
        /// <param name="username">unique username of the user</param>
        public User(string username)
        {
            Username = username;
            NextExpectedMessageID = 0;
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
                NbMessages++;
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
    }
}

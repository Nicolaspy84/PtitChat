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
        /// <param name="user">Username</param>
        /// <param name="msgID">Unique message ID</param>
        /// <param name="date">Date and time when the message was created</param>
        /// <param name="msg">Message content</param>
        /// <returns>true if this is an unseen message</returns>
        public static bool NewMessage(Peer origin, string user, int msgID, DateTime date, string msg)
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
        /// Returns a string representing the current state of the Users
        /// STATE#USERNAME#BOUNCE#USER1#ID#USER2#ID2#...#USERN#IDN with :
        /// USERNAME our client's username
        /// BOUNCE the number of times the message has to be exchanged
        /// USERI the username of the i'th user
        /// IDI the next expected message ID from USERI
        /// </summary>
        /// <param name="clientUsername">Our own client's username</param>
        /// <param name="bounce">Number of times the message has to be exchanged</param>
        /// <returns>The state string</returns>
        public static string GetState(string clientUsername, int bounce)
        {
            string endStr = string.Format("{0}#{1}#{2}#", Peer.STATUS, clientUsername, bounce);
            lock (All)
            {
                foreach (var user in All)
                {
                    endStr += string.Format("{0}:{1}&", user.Key, user.Value.NextExpectedMessageID);
                }
                if (All.Count > 0)
                {
                    endStr = endStr.Remove(endStr.Length);
                }

            }
            return endStr;
        }


        /// <summary>
        /// Returns all missing message IDs in the provided status compared to our status
        /// </summary>
        /// <param name="status">Status string we are comparing our own status to</param>
        /// <returns>A dictionary for every missing user and the corresponding next missing message ID interval</returns>
        public static Dictionary<string, Tuple<int, int>> GetMissingMessages(string status)
        {

            // Create the list to hold missing messages
            Dictionary<string, Tuple<int, int>> endList = new Dictionary<string, Tuple<int, int>>();

            // Lock users to process
            lock (All)
            {

                // Put all user information in the dictionnary
                foreach (var user in All)
                {
                    if (user.Value.NextExpectedMessageID > 0)
                    {
                        endList.Add(user.Key, Tuple.Create(0, user.Value.NextExpectedMessageID - 1));
                    }
                }

            }

            // If status is not empty, we update our array
            if (status != "")
            {
                string[] statusUsers = status.Split('&');
                foreach (var statusUser in statusUsers)
                {
                    string[] statusSplit = statusUser.Split(':');
                    string user;
                    int nextExpectedMessage;
                    try
                    {
                        user = statusSplit[0];
                        nextExpectedMessage = Int32.Parse(statusSplit[1]);
                    }
                    catch (Exception)
                    {
                        endList.Clear();
                        return endList;
                    }
                    if (endList.ContainsKey(user) && endList[user].Item2 > nextExpectedMessage)
                    {
                        endList[user] = Tuple.Create(nextExpectedMessage, endList[user].Item2);
                    }
                    else if (endList.ContainsKey(user) && endList[user].Item2 <= nextExpectedMessage)
                    {
                        endList.Remove(user);
                    }
                }
            }
            
            return endList;
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
            endStr += string.Format("From {0} :\n", Username);
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

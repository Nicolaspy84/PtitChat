using System;
using System.Collections.Generic;

namespace Interface
{
    public static class AllUsers
    {

        /// <summary>
        /// Static dictionary holding all instances of the User class
        /// </summary>
        private static Dictionary<string, User> all = new Dictionary<string, User>();

        public static Dictionary<string, User> All
        {
           get { return all; }

            set
            {
                all = value;
                OnAllChanged(EventArgs.Empty);
            }
        }

        public static event EventHandler AllChanged;

        public static void OnAllChanged(EventArgs e)
        {
            AllChanged?.Invoke(null, e);
        }


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
            lock (All)
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
        /// Adds a potential new user we heard of by the origin Peer
        /// </summary>
        /// <param name="origin">the Peer who told us about the existence of this user</param>
        /// <param name="user">user string name</param>
        public static void AddPotentialNewUser(Peer origin, string user)
        {
            lock (All)
            {
                // Create a new user if this one is unknown
                if (All.ContainsKey(user) == false)
                {
                    All[user] = new User(user) { LatestPeer = origin };
                }
            }
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
    }

}



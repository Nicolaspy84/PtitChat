using System;
using System.Collections.Generic;

namespace PtitChat
{
    public class Messages
    {
        public Dictionary<string, string> MessagesDic = new Dictionary<string, string>();
        Dictionary<string, int> LastMessagesDic = new Dictionary<string, int>();
        public int IdMessage;
        public Client user;

        public Messages(Client User)
        {
            user = User;
        }

        // when receiving a message, add message to dictionary and update LastMessagesDic
        public void AddPeerMessage(string peer_username, string peer_message_id, string message)
        {
            // peer is a string with username and Id of message : <name:ID>
            MessagesDic.Add(peer_username + ':' + peer_message_id, message);
            UpdateLastMessageDic();
        }

        // when sending a message, add message to dictionary
        public void AddMyMessage(string message)
        {
            string key = user.Username + ":" + IdMessage.ToString();
            MessagesDic.Add(key, message);
            IdMessage++;
        }

        // update LastMessagesDic
        public void UpdateLastMessageDic()
        {
            // for each different user, give id of last message received, when all the ids below have also been received
            //Dictionary<string, int> lastMessages = new Dictionary<string, int>();
            foreach (string peer in user.Peers.Keys) //gives peers name
            {
                int nb = 0;
                while (user.Peers.ContainsKey(peer + ":" + nb.ToString()))
                {
                    nb++;
                }
                LastMessagesDic[peer] = nb;
            }
        }

        public Dictionary<string, int> GetLastMessagesDic()
        {
            return (LastMessagesDic);
        }
    }
}

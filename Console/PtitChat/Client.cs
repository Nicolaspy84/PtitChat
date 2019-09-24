using System;
namespace PtitChat
{
    public class Client
    {
        // Default constructor requires a client with a username
        public Client(string username)
        {
            Username = username;
        }

        public string Username;
    }
}

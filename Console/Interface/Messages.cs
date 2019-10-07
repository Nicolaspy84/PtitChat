using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace Interface
{
    public class Messages : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private string username;

        public string Username
        {
            get {
                if (username == null)
                {
                    username = string.Empty;
                }
                return username; }

            set
            {
                if (value != username)
                {
                    username = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Username"));
                }
            }
        }

        private string message;

        public string Message
        {
            get {
                if (message == null)
                {
                    message = string.Empty;
                }
                return message; }

            set
            {
                if (value != message)
                {
                    message = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Message"));
                }
            }
        }

        public Messages(string name, string text)
        {
            username = name;
            message = text;
        }
    }
}

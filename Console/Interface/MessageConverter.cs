using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Interface
{
    /// <summary>
    /// Converter used to display both private and non private messages
    /// The first argument is the dictionnary All
    /// The second argument is the conversation selected (it can be either "Tous" for the 
    /// global chat or the name of a user for private messages)
    /// </summary>
    class MessageConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var all = value[0] as Dictionary<string, User>;
            // If there is no conversation selected or if the global conversation is selected,
            // we display the global chat 
            if (value[1] == null || (string)value[1] == "Tous")
            {
                string result = string.Empty;
                List<Messages> messages = new List<Messages>();
                foreach (KeyValuePair<string, User> user in all)
                {
                    Dictionary<int, Tuple<DateTime, string>> userMessages = user.Value.Messages;
                    string username = string.Empty;
                    if (user.Key == Client.Username)
                    {
                        username = "Moi";
                    }
                    else
                    {
                        username = user.Key;
                    }
                    foreach (KeyValuePair<int, Tuple<DateTime, string>> message in userMessages)
                    {
                        messages.Add(new Messages(username, message.Value.Item2, message.Value.Item1));
                    }
                }
                messages.Sort();
                foreach (Messages message in messages)
                {
                    result += message.ToString();
                }
                return result;
            }
            else
            {
                // If we are in a private conversation, we display the private messages between
                // the user and the other user
                string origin = (string)value[1];
                string messages = string.Empty;
                List<Messages> privateMessages = new List<Messages>();
                foreach(KeyValuePair<string, User> user in all)
                {
                    if (user.Key == Client.Username)
                    {
                        User currentUser = user.Value;
                        foreach(KeyValuePair<string, List<Tuple<DateTime, string>>> messagesFromUser in currentUser.PrivateMessages)
                        {
                            if (messagesFromUser.Key == origin)
                            {
                                foreach(Tuple<DateTime, string> privateMessage in messagesFromUser.Value)
                                {
                                    privateMessages.Add(new Messages(origin, privateMessage.Item2, privateMessage.Item1));
                                }
                            }
                        }
                        foreach(KeyValuePair<string, List<Tuple<DateTime, string>>> privateMessagesSent in currentUser.PrivateMessagesSent)
                        {
                            if (privateMessagesSent.Key == origin)
                            {
                                foreach(Tuple<DateTime, string> privateMessageSent in privateMessagesSent.Value)
                                {
                                    privateMessages.Add(new Messages("Moi", privateMessageSent.Item2, privateMessageSent.Item1));
                                }
                            }
                        }
                        privateMessages.Sort();
                        foreach(Messages message in privateMessages)
                        {
                            messages += message.ToString();
                        }
                    }
                }
                return messages;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

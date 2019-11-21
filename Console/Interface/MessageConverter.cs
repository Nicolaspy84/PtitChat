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
                List<Tuple<DateTime, string>> messages = new List<Tuple<DateTime, string>>();
                foreach (KeyValuePair<string, User> user in all)
                {
                    Dictionary<int, Tuple<DateTime, string>> userMessages = user.Value.Messages;
                    foreach (KeyValuePair<int, Tuple<DateTime, string>> message in userMessages)
                    {
                        (DateTime date, string messageString) = message.Value;
                        messages.Add(new Tuple<DateTime, string>(date, (user.Key + " : " + messageString)));
                    }
                }
                foreach (Tuple<DateTime, string> t in messages)
                {
                    result += t.Item1.ToString() + "  " + t.Item2 + "\n";
                }
                return result;
            }
            else
            {
                // If we are in a private conversation, we display the private messages between
                // the user and the other user
                string origin = (string)value[1];
                string messages = string.Empty;
                List<Tuple<DateTime, string>> privateMessages = new List<Tuple<DateTime, string>>();
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
                                    privateMessages.Add(privateMessage);
                                }
                            }
                        }
                        foreach(KeyValuePair<string, List<Tuple<DateTime, string>>> privateMessagesSent in currentUser.PrivateMessagesSent)
                        {
                            if (privateMessagesSent.Key == origin)
                            {
                                foreach(Tuple<DateTime, string> privateMessageSent in privateMessagesSent.Value)
                                {
                                    privateMessages.Add(privateMessageSent);
                                }
                            }
                        }
                        foreach(Tuple<DateTime, string> privateMessage in privateMessages)
                        {
                            messages += privateMessage.Item1.ToString() + "  " + privateMessage.Item2 + "\n";
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

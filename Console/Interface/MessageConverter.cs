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
    /// </summary>
    class MessageConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            var all = value[0] as Dictionary<string, User>;
            List<Tuple<DateTime, string>> messages = new List<Tuple<DateTime, string>>();
            foreach(KeyValuePair<string, User> user in all)
            {
                Dictionary<int, Tuple<DateTime, string>> userMessages = user.Value.Messages;
                foreach(KeyValuePair<int, Tuple<DateTime, string>> message in userMessages)
                {
                    (DateTime date, string messageString) = message.Value;
                    messages.Add(new Tuple<DateTime, string>(date, (user.Key + " : " + messageString)));
                }
            }
            foreach(Tuple<DateTime, string> t in messages)
            {
                result += t.Item1.ToString() + "  " + t.Item2 + "\n";
            }
            if (value[1] == null)
            {
                return result;
            }
            else
            {
                string origin = (string)value[1];
                if (origin == "Tous")
                {
                    return result;
                }
                else
                {
                    string privateMessages = string.Empty;
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
                                        privateMessages += privateMessage.Item1.ToString() + "  " + privateMessage.Item2 + "\n";
                                    }
                                }
                            }
                        }
                    }
                    return privateMessages;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

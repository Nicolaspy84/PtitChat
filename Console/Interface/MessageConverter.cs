using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Interface
{
    class MessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            var all = value as Dictionary<string, User>;
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
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

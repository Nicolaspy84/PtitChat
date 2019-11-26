using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Interface
{
    /// <summary>
    /// Converter used to display the list of destinations available for a user
    /// </summary>
    class UserConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<string> conversations = new ObservableCollection<string>
            {
                "Tous",
                "Fichiers"
            };
            var all = value as Dictionary<string, User>;
            foreach(KeyValuePair<string, User> user in all)
            {
                if (user.Key != Client.Username)
                {
                    conversations.Add(user.Key);
                }
            }
            return conversations;


            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

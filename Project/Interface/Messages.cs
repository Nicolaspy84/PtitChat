using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace Interface
{
    public class Messages : IComparable
    {

        public string username;

        public string content;

        public DateTime date;

        public Messages(string name, string message, DateTime messageDate)
        {
            username = name;
            content = message;
            date = messageDate;
        }

        public int CompareTo(object obj)
        {
            return date.CompareTo(((Messages)obj).date);

        }

        public override string ToString()
        {
            return this.date.ToString("HH:mm") + " " + this.username + " : " + this.content + "\n";
        }
    }
}

using System;
using System.Collections.Generic;

namespace Data.Helpers
{
    public class Logger
    {
        public List<Tuple<DateTime, string>> Messages = new List<Tuple<DateTime, string>>();

        public delegate void MessageAddedEventHandler(object sender, MessageAddedEventArgs e);

        public class MessageAddedEventArgs : EventArgs
        {
            public DateTime Date;
            public string Message;
            public MessageAddedEventArgs(DateTime date, string msg)
            {
                Date = date;
                Message = msg;
            }
        }
        public event MessageAddedEventHandler MessageAdded;

        public void AddMessage(string message)
        {
            var item = new Tuple<DateTime, string>(DateTime.Now, message);
            Messages.Add(item);
            MessageAdded?.Invoke(this, new MessageAddedEventArgs(item.Item1, item.Item2));
        }
    }
}

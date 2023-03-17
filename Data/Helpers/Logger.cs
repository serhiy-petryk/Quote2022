using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Data.Helpers
{
    public static class Logger
    {
        public delegate void MessageAddedEventHandler(object sender, MessageAddedEventArgs e);
        public static event MessageAddedEventHandler MessageAdded;

        private static readonly List<Tuple<DateTime, string, string>> Messages = new List<Tuple<DateTime, string, string>>();

        public static void AddMessage(string message)
        {
            var sf = new StackFrame(1);
            var method = sf.GetMethod();
            var callMethodName = method.DeclaringType?.Name + "." + method.Name;

            var item = new Tuple<DateTime, string, string>(DateTime.Now, message, callMethodName);
            Messages.Add(item);
            MessageAdded?.Invoke(null, new MessageAddedEventArgs(item.Item1, item.Item2, item.Item3));
        }

        public class MessageAddedEventArgs : EventArgs
        {
            public DateTime Date;
            public string MethodName;
            public string Message;
            public string FullMessage => $"{MethodName}. {Message}";
            public MessageAddedEventArgs(DateTime date, string msg, string methodName)
            {
                Date = date;
                Message = msg;
                MethodName = methodName;
            }
        }
    }
}

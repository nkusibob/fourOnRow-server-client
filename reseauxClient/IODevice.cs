using System;

namespace reseauxClient
{
    public class IODevice
    {
        public static IODevice CreateConsoleDevice() { return new ConsoleDevice(); }
        private IODevice()
        {
        }

        public virtual void Output(string message, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public virtual object Request(string message, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public virtual void Accept(string message, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        private class ConsoleDevice : IODevice
        {
            public override void Output(string message, params object[] arguments)
            {
                Console.WriteLine(string.Format(message, arguments));
            }

            public override object Request(string message, params object[] arguments)
            {
                Console.Write(string.Format(message, arguments));
                return Console.ReadLine();
            }

            public override void Accept(string message, params object[] arguments)
            {
                Console.Write(string.Format(message, arguments));
                Console.ReadKey();
            }
        }
    }
}

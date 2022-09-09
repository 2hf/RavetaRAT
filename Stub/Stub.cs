using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stub
{

    static class Program
    {

        public static void _Client()
        {
            checkserver:

            if (!Client.CheckIfServerOnline())
            {
                Thread.Sleep(200);
                goto checkserver;
            }
            
 

            Client.StartClient();

            Task.Run(() => Client.Loop());

            while (true)
            {
                if (Client.AwaitingServer)
                {
                    Client.DisposeListener();
                    goto checkserver;
                }
            }

        }
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);
        static void Main()
        {
#if DEBUG
            AttachConsole(-1);
#else
 
#endif

            _Client();
        }
    }
}

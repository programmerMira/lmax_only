using System;
using WebSocketSharp.Server;

namespace websocket_server
{
    class Program
    {
        public static void Main()
        {
            Server.StartThreads();
            
            var wssv = new WebSocketServer ("ws://0.0.0.0:5001");
            //wssv.AddWebSocketService<Server> ("/Server");
            wssv.Start();
            Console.ReadKey (true);
            wssv.Stop();
        }
    }
}

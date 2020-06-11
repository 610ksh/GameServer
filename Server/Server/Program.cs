using System;
using System.Net; // for EndPoint
using System.Threading; // for Thread
using ServerCore;

namespace Server
{
    
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            PacketManager.Instance.Register();

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 세션을 Lambda 형태로 만들어줌. 일단 GameSession 형태로 만듦.
            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("Listening. . .");

            while (true)
            {
                // 멈추지 않게 진행중
            }
        }

    }
}

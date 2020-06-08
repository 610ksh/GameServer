using System;
using System.Net; // for EndPoint
using System.Net.Sockets; // for Socket
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    class Program
    {
        // Client side
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = iPHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector(); // ServerCore 라이브러리 사용
            
            // 아래 연결하는 GameSession은 클라쪽에서 생성한 GameSession임. 서버x
            connector.Connect(endPoint, () => { return new ServerSession(); });

            while (true)
            {
                try
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(100); // ms (1000분의 1초, m : 10^-3)
            }
        }
    }
}

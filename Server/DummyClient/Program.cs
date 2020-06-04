using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // for Dns
using System.Net.Sockets; // for Socket
using System.Text; // for Encoding
using System.Threading; // for Thread
using System.Threading.Tasks;

/*
 * 임시 클라이언트 역할.
 * 
 *  Copyright 2020. SungHoon all rights reserved.
 */

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

            while (true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 문지기한테 입장 문의
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    for (int i = 0; i < 5; i++)
                    {
                        // 보낸다
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello Server {i}");
                        int sendBytes = socket.Send(sendBuff);
                    }

                    // 받는다
                    byte[] recvBuff = new Byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    // 나간다.
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(1000); // ms (1000분의 1초, m : 10^-3)
            }
        }
    }
}

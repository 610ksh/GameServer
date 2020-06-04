using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // for DNS
using System.Net.Sockets; // for Socket
using System.Text; // for Encoding
using System.Threading; // for Thread
using System.Threading.Tasks; // for Task

/*
 *  Copyright 2020. SungHoon all rights reserved.
 */

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            // 여기까지오면 Connect -> Accept 되어 대리인 소켓인 clientSocket 생성됨.
            try
            {
                Session session = new Session(); // Session 생성
                session.Start(clientSocket); // Receive from client

                // Send to client
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hi Client, Welcome to MMORPG Server !");
                session.Send(sendBuff);

                Thread.Sleep(1000); // 1초 대기

                // close to server
                session.Disconnect();
                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = iPHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 문지기에게 명령함. 혹시 어떤 요청이 들어오면 OnAcceptHandler라는 곳으로 알려줘
            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening. . .");

            while (true)
            {
                // 프로그램 종료되지 않게만 넣어주자.
            }
        }
    }
}
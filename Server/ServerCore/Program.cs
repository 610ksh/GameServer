﻿using System;
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
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected is failed : {endPoint}");
            
            byte[] sendBuff = Encoding.UTF8.GetBytes("Hi Client, Welcome to MMORPG Server !");
            Send(sendBuff); // Send to client
            Thread.Sleep(1000); // 1초 대기
            Disconnect(); // close to server
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected is failed : {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }


    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = iPHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 문지기에게 명령함. 혹시 어떤 요청이 들어오면 OnAcceptHandler라는 곳으로 알려줘
            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening. . .");

            while (true)
            {
                // 프로그램 종료되지 않게만 넣어주자.
            }
        }
    }
}
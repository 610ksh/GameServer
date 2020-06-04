﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // for Dns(IPEndPoint)
using System.Net.Sockets; // for Socket
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory; // Accept가 완료되면 어떻게 처리할지에 대해

        // Initialize
        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _sessionFactory += sessionFactory;

            // 문지기 교육
            _listenSocket.Bind(endPoint);
            // 영업 시작
            _listenSocket.Listen(10);
            for (int i = 0; i < 10; ++i)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs(); // 이벤트 생성
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); // 이벤트 구독
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null; // 새로 초기화. 안넣으면 크러쉬가 남

            bool pending = _listenSocket.AcceptAsync(args); // 성공하든 아니든 일단 return함
            if (pending == false) // false = 비동기로 호출했지만, 바로 완료되었다는 의미
                OnAcceptCompleted(null, args); // 직접 호출
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success) // 잘 처리 됐다는 의미.
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket); // Receive from client
                session.OnConnected(args.AcceptSocket.RemoteEndPoint); // TODO
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args); // 다시 리스닝 하러감.
        }

        // 기존 블로킹 방식. 사용 X
        public Socket Accept()
        {
            // 블로킹 계열 함수를 사용한다는게 문제임.
            // 게임을 만들때는 블로킹 계열 함수는 피해야함.
            return _listenSocket.Accept();
        }
    }
}
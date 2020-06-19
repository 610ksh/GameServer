using System;
using System.Collections.Generic;
using System.Net; // for IPEndPoint
using System.Net.Sockets; // for Socket
using System.Text;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory; // Dummy Client의 GameSession을 호출하기 위해

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            // Client 휴대폰 설정 
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            // 소켓 비동기 방식 이벤트 객체 생성
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted; // 이벤트 연결
            args.RemoteEndPoint = endPoint; // args에 클라리언트 주소
            args.UserToken = socket; // 소켓정보도 args에 넣어줌.

            RegisterConnect(args);
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket; // 소켓 생성
            if (socket == null) // 혹시라도 소켓이 제대로 들어가지 않았다면
                return;

            bool pending = socket.ConnectAsync(args); // 비동기 연결 시도
            if (pending == false)
                OnConnectCompleted(null, args);
        }

        void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke(); // Dummy Client의 GameSession 호출
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted is failed : {args.SocketError}");
            }
        }
    }
}

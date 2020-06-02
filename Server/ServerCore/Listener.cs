using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // for Dns(IPEndPoint)
using System.Net.Sockets; // for Socket
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler; // Accept가 완료되면 어떻게 처리할지에 대해

        // Initialize
        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _onAcceptHandler += onAcceptHandler;

            // 문지기 교육
            _listenSocket.Bind(endPoint);
            // 영업 시작
            _listenSocket.Listen(10);

            // 이벤트를 만듦 (args = 아규먼트, 일종의 요정 역할, 일꾼)
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        // ★★ 논블로킹 방식 (비동기 방식)으로 만듦.
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null; // 새로 초기화. 안넣으면 크러쉬가 남

            // Async: 비동기를 뜻함(에이싱크). 일단 예약만.
            bool pending = _listenSocket.AcceptAsync(args); // 성공하든 아니든 일단 return함

            if (pending == false) // false = 비동기로 호출했지만, 바로 완료되었다는 의미
                OnAcceptCompleted(null, args); // 직접 호출
        }

        // Accept 되었을때 처리 부분
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 소켓 에러를 체크
            if (args.SocketError == SocketError.Success) // 잘 처리 됐다는 의미.
            {
                // args.AcceptSocket의 역할이 Socket clientSocket = _listener.Accept() 와 같다.
                _onAcceptHandler.Invoke(args.AcceptSocket); // 대리인 소켓 생성(세션)
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
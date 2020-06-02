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
            // 문지기 (문지기 휴대폰 만들기)
            /*
             *  // .AddressFamily : ip4를 쓸지 6를 쓸지 결정하는건데, Dns에서 자동으로 결정했기에 따라가면됨.
             *  // TCP를 사용할지 UDP를 사용할지 결정 
             *  // 우린 TCP를 사용할거, SocketType을 Stream으로 하고, Tcp로 프로토콜 설정해줌.
             * */
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _onAcceptHandler += onAcceptHandler; //

            // 문지기 교육 (식당 주소와 포트번호까지 기입)
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기수 (동시 다발적으로 접근했을때, 제한수 결정)
            _listenSocket.Listen(10); // 라이브에서 보통 조절

            // 이벤트를 만듦 (args = 아규먼트, 일종의 요정 역할, 일꾼)
            SocketAsyncEventArgs args = new SocketAsyncEventArgs(); // 한번만 만들면 계속 재사용가능.
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); // 이벤트를 추가
            RegisterAccept(args); // 맨 처음은 직접 등록함. 
            // 이 상태에서 만약 클라가 어떤 요청을 했다면, 콜백 방식으로 OnAcceptCompleted를 호출됨.
        }


        // ★★ 논블로킹 방식 (비동기 방식)으로 만듦.
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 기존의 잔재는 지워줘야함. 안그러면 에러가 남. 다시 초기화시켜줘야함.
            args.AcceptSocket = null; // 새로 초기화 // 안넣으면 크러쉬가 남

            // Async: 비동기를 뜻함(에이싱크). 일단 예약만.
            bool pending = _listenSocket.AcceptAsync(args); // 성공하든 아니든 일단 return함
            // pending (C++과 다름)

            // false = 비동기로 호출했지만, 바로 완료되었다는 의미
            if (pending == false)
                OnAcceptCompleted(null, args); // 이건 직접 호출,
            // 나중에라도 요청이 들어오면, 위의 이벤트 핸들러 방식의 콜백 방식으로 호출하게 됨.
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 소켓 에러를 체크. 진짜 에러가 있는 경우도 있고, 그냥 성공했다는 에러도 있다.
            if (args.SocketError == SocketError.Success) // 잘 처리 됐다는 의미.
            {
                // TODO
                // args.AcceptSocket의 역할이 Socket clientSocket = _listener.Accept() 와 같다
                _onAcceptHandler.Invoke(args.AcceptSocket); // 요청온것을 보고 소켓을 전달해줌. 그걸 다시 onAcceptHandler로 전달

            }
            else
                Console.WriteLine(args.SocketError.ToString());

            // 여기까지 왔으면 모든 일이 끝났으니, 다른 요청을 받으러감.
            RegisterAccept(args); // 재사용함.
        }

        // 블로킹 방법
        public Socket Accept()
        {
            // 블로킹 계열 함수를 사용한다는게 문제임.
            // 게임을 만들때는 블로킹 계열 함수는 피해야함.
            return _listenSocket.Accept();
        }
    }
}
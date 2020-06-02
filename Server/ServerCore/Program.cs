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
            try
            {
                // 받는다
                byte[] recvBuff = new byte[1024]; // 간단하게 만들어보자. 몇개를 보낼지 모름.
                int recvBytes = clientSocket.Receive(recvBuff); // 보내준 데이터는 recvBuff에 저장됨. 몇바이트를 받은지를 뱉어줌.
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes); // 문자열 인코딩 규약. 나중에 설명할예정. 버퍼로 받은 내용을 string으로 반환해줌. 시작인덱스, 길이
                Console.WriteLine($"[From Client] {recvData}"); // 클라가 보낸 문자르 출력 (일단 여기서는 문자열을 보낸다고 가정하고 간단히 코딩해보자. 실제 서버는 다르다)

                // 보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hi Client, Welcome to MMORPG Server !");
                clientSocket.Send(sendBuff); // 상대방이 안받으면 대기를 하게 됨.
                                             // ★ Accept(), Receive(), Send() 와 같은 모든 입출력 계열의 함수들은 무조건 비동기(논블로킹)으로 채택해야함

                // 쫓아낸다.
                clientSocket.Shutdown(SocketShutdown.Both); // 듣기도, 말하기도 싫다.
                clientSocket.Close(); // 실제로 세션을 닫음.
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // 172.1.2.3 이라는 실제 주소 대신에 www.naver.com 과 같은 이름으로 사용.
            // www.naver.com은 자체적으로 주소를 찾게됨. (172.1.2.3)
            string host = Dns.GetHostName(); // 로컬 호스트의 이름을 가져옴
            IPHostEntry iPHost = Dns.GetHostEntry(host); // DNS서버라는 놈이 우리 네트워크망에 있어서 IP주소를 찾아줌.(매우 복잡함)
            IPAddress ipAddr = iPHost.AddressList[0]; // 첫번째 IP주소를 가져옴(어차피 1개라서 그걸 가져오는거임)

            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // endPoint : 최종 주소. 7777 : 포트(식당 위치,문의 번호), ipAddr : 식당주소
                                                                // 참고로 클라가 7777포트가 아닌곳으로 요청, 접근을 시도하면, 입장을 못함. 클라도 포트를 맞춰줘야함.

            // 문지기에게 명령함. 혹시 어떤 요청이 들어오면, 나에게 OnAcceptHandler라는 곳으로 알려줘
            _listener.Init(endPoint,OnAcceptHandler);
            Console.WriteLine("Listening. . .");

            // 손님을 한번만 받고 끝낼게 아니기 떄문에, 무한루프
            while (true)
            {
               // 프로그램 종료되지 않게만 넣어주자.
            }


        }
    }
}

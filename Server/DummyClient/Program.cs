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
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // 172.1.2.3 이라는 실제 주소 대신에 www.naver.com 과 같은 이름으로 사용.
            // www.naver.com은 자체적으로 주소를 찾게됨. (172.1.2.3)
            string host = Dns.GetHostName(); // 로컬 호스트의 이름을 가져옴
            IPHostEntry iPHost = Dns.GetHostEntry(host); // DNS서버라는 놈이 우리 네트워크망에 있어서 IP주소를 찾아줌.(매우 복잡함)
            IPAddress ipAddr = iPHost.AddressList[0]; // 첫번째 IP주소를 가져옴(어차피 1개라서 그걸 가져오는거임)
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while (true)
            {
                // 휴대폰 설정
                /// TCP는 stream과 Tcp가 세트처럼 묶여서 사용된다. 외워도 될정도.
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 문지기한테 입장 문의
                    // 고객이 들고 있던 휴대폰으로 문지기한테 연락함. (입장가능한지 물어봄)
                    socket.Connect(endPoint); // 상대방 주소를 넣어줌. 입장 가능하면 아래 진행, 아니면 대기.
                                              // 참고로 MMO에서는 이런 블로킹이 위험함
                                              // 문제점 : 만약 Connect를 했는데, 서버가 못받으면 상당히 오랜시간 여기서 대기를 타기 때문.

                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    // 보낸다 (서버와 반대임)
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello Server, I'm Client");
                    int sendBytes = socket.Send(sendBuff); // 문제점 : 만약 서버에서 안받으면 좀 오래 대기하게 된다.

                    // 받는다
                    byte[] recvBuff = new Byte[1024];
                    int recvBytes = socket.Receive(recvBuff); // 몇바이트를 받는지 뱉어줌 // 문제점 : 만약 서버에서 아무 데이터도 안보내주면 좀 오래 대기하게 된다.
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes); // 서버의 내용 string으로 해석
                    Console.WriteLine($"[From Server] {recvData}"); // 서버가 보낸것을 출력함

                    // 나간다.
                    socket.Shutdown(SocketShutdown.Both); //
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(500); // 0.1초
            }
            
        }
    }
}

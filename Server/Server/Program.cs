using System;
using System.Net; // for EndPoint
using System.Threading; // for Thread
using ServerCore;

namespace Server
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected(Server) : {endPoint}"); // Session 소켓의 정보임.
            Console.WriteLine("OnConnected 성공함. 이전 함수 Session.Start() 수행하고 실행됨.");
            //Packet packet = new Packet() { size = 100, packetId = 10 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size); // knight 객체의 hp 데이터를 바이트로 저장.
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            //Send(sendBuff); // Send to client


            Thread.Sleep(1000); // 1초 대기
            Disconnect(); // close to server
        }

        // sealed 되어서 OnRecv는 사용x 대신 아래것을 사용.
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2); // 2바이트 이후
            Console.WriteLine($"RecvPacketId : {id}, Size {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected is failed : {endPoint}");
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
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 세션을 Lambda 형태로 만들어줌. 일단 GameSession 형태로 만듦.
            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening. . .");

            while (true)
            {
                // 멈추지 않게 진행중
            }
        }

    }
}

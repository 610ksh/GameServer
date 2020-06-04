using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets; // for Socket;
using System.Text;
using System.Threading; // for Interlocked (for thread)
using System.Threading.Tasks;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0; // flag for Interlocked
        // ServerCore의 Receive()를 구현하면 됨.

        public void Start(Socket socket)
        {
            _socket = socket; // socket 연결
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024); // 버퍼생성

            RegisterRecv(recvArgs); // 최초 실행
        }

        public void Send(byte[] sendBuff)
        {
            // _socket.Send(sendBuff);
            SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region Receive from client

        // SendAsync()
        void RegisterSend(SocketAsyncEventArgs args)
        {
            bool pending = _socket.SendAsync(args);
            if (pending == false)
                OnSendCompleted(null, args);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnSendCompleted is Failed {e}");
                    throw;
                }
            }
            else
            {
                Disconnect();
            }
        }
        

        // 1.이벤트 등록
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args); // Receive를 하는건 socket임.
            if (pending == false)
                OnRecvCompleted(null, args);
        }

        // 2.이벤트 실행
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.Count);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Faild {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
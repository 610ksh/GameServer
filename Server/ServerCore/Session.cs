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

        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;

        object _lock = new object(); // for lock

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // 재사용

        public void Start(Socket socket)
        {
            _socket = socket; // socket 연결
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024); // 버퍼생성

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs); // 최초 실행
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                // Critical Section
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 서버 통신

        // SendAsync()
        void RegisterSend()
        {
            _pending = true;
            byte[] buff = _sendQueue.Dequeue(); // 버퍼 꺼내기
            _sendArgs.SetBuffer(buff, 0, buff.Length); // 버퍼설정

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            // Critical section (cuz _pending)
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if (_sendQueue.Count > 0) // 큐가 빌때까지
                            RegisterSend(); // 비동기 실행 (낚싯대 던지기)
                        else
                            _pending = false; // 끝났음을 표시.
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
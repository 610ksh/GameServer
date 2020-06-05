using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // for EndPoint
using System.Net.Sockets; // for Socket;
using System.Text;
using System.Threading; // for Interlocked (for thread)
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0; // flag for Interlocked

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();

        object _lock = new object(); // for lock

        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // 재사용
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>(); // for bufferList

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        // interface
        public abstract void OnConnected(EndPoint endpoint); // 클라가 접속
        public abstract int OnRecv(ArraySegment<byte> buffer); // 패킷 받기 (from Client)
        public abstract void OnSend(int numOfBytes); // 패킷 보내기 (to Client)
        public abstract void OnDisconnected(EndPoint endPoint);
        //


        public void Start(Socket socket)
        {
            _socket = socket; // socket 연결

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(); // 최초 실행
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                // Critical Section
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        //////// SendAsync

        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue(); // 버퍼 꺼내기
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

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
                        _sendArgs.BufferList = null; // 버퍼 초기화
                        _pendingList.Clear(); // 버퍼리스트를 위한 초기화

                        OnSend(_sendArgs.BytesTransferred);


                        if (_sendQueue.Count > 0) // 큐가 빌때까지
                            RegisterSend(); // 비동기 실행 (낚싯대 던지기)
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted is Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        //////// ReceiveAsync

        void RegisterRecv()
        {
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs); // Receive를 하는건 socket임.
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동.
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠쪽에서 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동.
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();

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
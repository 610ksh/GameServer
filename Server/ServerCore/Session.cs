using System;
using System.Collections.Generic;
using System.Net; // for EndPoint
using System.Net.Sockets; // for Socket
using System.Text;
using System.Threading; // for Interlocked

namespace ServerCore
{
    // 패킷 형태
    public abstract class PacketSession : Session
    { 
        // 헤더는 2바이트라고 가정하자.
        public static readonly int HeaderSize = 2;

        // [size(2)][packeId(2)][...] [size(2)][packeId(2)][...] // sealed : 다른 클래스가 상속했을때 overide 차단
        public sealed override int OnRecv(ArraySegment<byte> buffer) // 패킷 받기 (from Client)
        {
            int processLen = 0; // 현재 처리한 바이트 수.

            // 주어진 패킷을 처리할 수 있을때까지 무한 반복
            while (true)
            {
                // 최소한 헤더를 파싱할 수 있는지 (2바이트보다 작으면)
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 왔는지 확인 (헤더 패킷을 까봐야 알수있음. 내용을 봐야암)
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset); //ushort 크기 뱉음
                // 패킷이 완전체가 아니라 부분적으로 왔을떄
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔으면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                // 버퍼를 옮겨줘야함.
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }
            return processLen;
        }
        // PacketSession을 상속받는 애들은 OnRecv 인터페이스가 아니라 OnRecvPacket이라는 인터페이스로 받아라.
        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        // Member variable
        Socket _socket; // Session Socket (대리인)
        int _disconnected = 0; // flag for Interlocked

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>(); // for send

        object _lock = new object(); // for lock

        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // 재사용
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>(); // for bufferList

        RecvBuffer _recvBuffer = new RecvBuffer(1024); // 1024 / 4096

        // interface
        public abstract void OnConnected(EndPoint endpoint); // 클라가 접속
        public abstract int OnRecv(ArraySegment<byte> buffer); // 패킷 받기 (from Client)
        public abstract void OnSend(int numOfBytes); // 패킷 보내기 (to Client)
        public abstract void OnDisconnected(EndPoint endPoint);
        //

        // Initialize
        public void Start(Socket socket)
        {
            _socket = socket; // socket 연결

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(); // 최초 실행
        }

        // if you want to Send, Call Send Function
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
            _sendArgs.BufferList = _pendingList; // BufferList 사용

            bool pending = _socket.SendAsync(_sendArgs); // BufferList의 경우 보낼떄 한꺼번에 전달된다.
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

            bool pending = _socket.ReceiveAsync(_recvArgs); // Async Receive 실행
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

                    RegisterRecv(); // 다시 낚싯대를 던짐
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

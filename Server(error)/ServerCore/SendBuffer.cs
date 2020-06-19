using System;
using System.Collections.Generic;
using System.Text;
using System.Threading; // for ThreadLocal

namespace ServerCore
{
    // SendBuffer를 사용하기 쉽게 도와주는 클래스
    public class SendBufferHelper
    {
        // SendBuffer를 한번만 만들어주고 고갈될때까지 사용하고, 멀티쓰레딩을 고려함 (by ThreadLocal)
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
        /// <summary>
        ///  전역은 전역인데, 자신의 쓰레드 영역에서만 사용할 수 있는 전역.
        ///  람다 함수의 의미 : 처음에 만들때 무엇을할지를 의미. 아무것도 하지 않고 null 반환.
        /// </summary>

        public static int ChunkSize { get; set; } = 4096 * 100; // 처음에는 크게 잡아줌.


        /// Wrapping 해서 편하게 사용.
        public static ArraySegment<byte> Open(int reserveSize)
        {
            // SendBuffer를 한번도 사용하지 않음.
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            // 이미 만들어진 SendBuffer가 있기는 한데, 너가 요구한것을 충족하지 못하면
            if (CurrentBuffer.Value.FreeSize < reserveSize) // C++에서는 좀더 효율적으로 가능.
                CurrentBuffer.Value = new SendBuffer(ChunkSize); // 다시 만듦.

            // 여기까지 오면, 현재 버퍼는 공간이 있는 상태로 만들어져있다는 의미.
            // 버퍼를 사용함.
            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            // Wrapping해서 넘겨줌.
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        // [ ][ ][ ][u] [ ][ ][ ][ ]
        byte[] _buffer;
        int _usedSize = 0;

        // 사용 가능한 버퍼 크기
        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        // 생성자
        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        // 얼마만큼의 사이즈를 최대로 사용할지 설정
        public ArraySegment<byte> Open(int reserveSize)
        {
            // 사용 가능 크기보다 예약하고자 하는 사이즈가 더 클 경우
            if (reserveSize > FreeSize)
                return ArraySegment<byte>.Empty; // null로하면 안됨.

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        // 버퍼 다 사용후 반환할때
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;

        }
    }
}

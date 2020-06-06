using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class RecvBuffer
    {
        // [ ][r][ ][w] [ ][ ][ ][ ]
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);

        }

        // 실제로 사용한 버퍼의 사이즈
        public int DataSize { get { return _writePos - _readPos; } }
        // 여유 공간이 있는 버퍼 사이즈
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        // 현재 사용중인 버퍼
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            // r과 w의 위치가 같을때
            if(dataSize==0)
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 찌끄레기가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        // Read 옮기기
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;
            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            // 사용할 수 있는 공간보다 더 크다면
            if (numOfBytes > FreeSize)
                return false;
            _writePos += numOfBytes;
            return true;
        }

    }
}

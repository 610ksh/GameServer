using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    // 구현 정책 >
    // 재귀적 락을 허용할지 (No)
    // 만약 재귀적 락을 허용한다면 (YES) : WriteLock->WriteLock OK, WriteLock->ReadLock OK, ReadLock->WriteLock No
    // 스핀락 정책 (5000번 이후 Yield) 
    class Lock
    {
        // 비트 flag, 맨왼쪽 비트는 사용하지 않는다.
        const int EMPTY_FLAG = 0x0000000; // 0
        const int WRITE_MASK = 0x7FFF0000; // 15비트 사용
        const int READ_MASK = 0x0000FFFF; // 16비트 사용
        const int MAX_SPIN_COUNT = 5000; // for 스핀락 정책

        // [UnUsed(1)] : 부호가 뒤집히는것을 방지
        // [WriteThreadId(15)] : WriteLock은 한번에 한 쓰레드만 잡을 수 있는데, 그 쓰레드가 누군지 기록.
        // -> 사실 bool로 해도 될것 같은데, 재귀적 락을 허용할 경우 누가 락을 잡고 있는지 알아야 하기 때문에 필요.
        // [ReadCount(16)] : Read Lock을 획득했을때 여러 쓰레드들이 동시에 Read를 잡을때 카운팅하는것.
        int _flag = EMPTY_FLAG;

        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인한다.
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if(Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            // 1부터 늘어나는 숫자인데, 쓰레드 아이디. UnUsed와 ReadCount부분이 모두 0으로 채워짐
                int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            
            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을때,
            // 경합하여 소유권을 얻는다.
            while(true)
            {
                for(int i=0;i<MAX_SPIN_COUNT;++i)
                {
                    /*
                    // 시도해서 성공하면 return
                    if (_flag == EMPTY_FLAG)
                        _flag = desired;
                    // 위의처럼 구현하면 race condition이 발생. 두 부분으로 나눠지기 때문. 해결책은 아래.
                    */

                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }
                Thread.Yield();
            }
        }

        // Write 한 쓰레드만 WriteUnlock을 할 수 있다.
        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if(lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG); // flag를 EMPTY_FLAG로 초기화
        }


        public void ReadLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인한다.
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for(int i=0;i<MAX_SPIN_COUNT;++i)
                {
                    /*
                    // 아무도 WriteLock을 획득하지 않는다면
                    if((_flag & WRITE_MASK) == 0)
                    {
                        _flag = _flag + 1;
                        return;
                    }
                    // 위 방법도 같은 맥락으로 race condition이 발생할 수 있음
                    */

                    // 내가 예상한 값, 원하는값은 WriteLock을 획득하지 않은 상태를 말한거라서.
                    int expected = (_flag & READ_MASK);

                    // 아래가 성공했다는 것이면 아무도 WriteLock을 획득하지 않았으므로 +1을 한다는것. (lock-free의 기초)
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }
                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag); // 숫자를 줄여줌
        }
    }
}

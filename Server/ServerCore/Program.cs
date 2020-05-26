using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading; // for Thread
using System.Threading.Tasks; // for Task

/*
 * 
 * 
 *  Copyright 2020. SungHoon all rights reserved.
 */

namespace ServerCore
{
    // 메모리 배리어
    // 1. 코드 재배치 억제 : Thread.MemoryBarrier();
    // => Full memory barrier (ASM(어셈블리) MFENCE, C# Thread.MemoryBarrier) : Store & Load 둘다 막음.
    // => Store Memory barrier (ASM에서 명령어 SFENCE) : Store만 막는다 (반쪽짜리도 가능)
    // => Load Memory barrier (ASM에서 명령어 LFENCE) : Load만 막는다 (반쪽짜리도 가능)
    // 참고로 store, load만 막는건 쓸일이 거의 없다. 그냥 있다는 정도만 알아두자.

    // 2. 가시성

    // 유명한 메모리 배리어 예제임.
    // 두 쓰레드가 동시에 실행될때.
    // 1,2,3,4 배리어를 해야만 우리가 예상하는 답이 나옴.
    // 만약 배리어를 안하면, 순서가 뒤바뀌거나 가시성에도 문제가 생길 수 있다.
    class Program
    {
        int _answer;
        bool _complete;

        void A()
        {
            _answer = 123;
            Thread.MemoryBarrier(); // 1, _answer를 메모리 실제 저장
            _complete = true;
            Thread.MemoryBarrier(); // 2, _complete를 메모리 실제 저장
        }

        void B()
        {
            Thread.MemoryBarrier(); // 3, _complete를 읽기전에 확보
            if(_complete)
            {
                Thread.MemoryBarrier(); // 4, _answer를 읽기전에 확보
                Console.WriteLine(_answer);
            }
        }

        static void Main(string[] args)
        {
           
        }
    }
}

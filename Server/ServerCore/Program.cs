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
    /*
     * 
     * 
     * */
    class SpinLock
    {
        volatile int _locked = 0;

        // Monitor.Enter()
        public void Acquire()
        {
            while (true)
            {
                #region
                /*
                // 넣기 전의 값을 return함. 그건 아래 용도때문
                int original = Interlocked.Exchange(ref _locked, 1);

                // 기존 값이 0이면 성공. 만약 1이면 다른 누군가가 동시에 접근했다는 의미라서 다시 반복.
                if (original == 0)
                    break;
                */
                #endregion

                // CAS Compare-And-Swap
                int expected = 0;
                int desired = 1;

                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;
            }
        }

        // Monitor.Exit()
        public void Release()
        {
            _locked = 0;
        }
    }

    class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }
}

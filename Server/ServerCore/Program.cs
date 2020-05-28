using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading; // for Thread
using System.Threading.Tasks; // for Task

/*
 *  Copyright 2020. SungHoon all rights reserved.
 */

namespace ServerCore
{
    class Program
    {
        static volatile int count = 0;
        static Lock _lock = new Lock();

        static void Main(string[] args)
        {
            Task t1 = new Task(delegate ()
            {
                for(int i=0;i<100000;++i)
                {
                    // 재귀 허용 test
                    _lock.WriteLock();
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnlock();
                    _lock.WriteUnlock();
                }
            });

            Task t2 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; ++i)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });

            t1.Start();
            t2.Start();

            Task.WaitAll(t1,t2);

            Console.WriteLine(count);
        }
    }
}

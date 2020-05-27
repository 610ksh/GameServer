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

    class Program
    {
        static int num = 0;
        static object _obj = new object(); // 실제 데이터를 저장하는 용도는 아님. static이 있어야 안에서 사용가능하므로 static을 사용했음.

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; ++i)
            {
                lock(_obj)
                {
                    num++;
                }
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; ++i)
            {
                lock (_obj)
                {
                    num--;
                }
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2); // t1, t2쓰레드가 모두 끝날때까지 main 쓰레드는 기달

            Console.WriteLine(num);
        }
    }
}

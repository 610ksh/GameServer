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

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; ++i)
            {
                int before = num;
                int afterValue = Interlocked.Increment(ref num); // CPU단에서 이 연산을 원자적으로 처리함.
                int next = num;


                // 하지만 그렇다고 이제부터 항상 ++ 안쓰고 interlocked를 사용하는건 무리.
                // 단점 : 성능상 손해를 많이봄. (연산 속도)
                // num++;
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; ++i)
            {
                Interlocked.Decrement(ref num);
                //num--;
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

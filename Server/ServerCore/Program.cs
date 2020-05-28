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

        // 각 쓰레드마다 자신만의 영역을 가짐. 서로 영향주지 않는것.
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(()=> { return $"My Name Is {Thread.CurrentThread.ManagedThreadId}"; }); // TLS 방식

        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if(repeat)
                Console.WriteLine(ThreadName.Value + "(repeat)");
            else
                Console.WriteLine(ThreadName.Value);
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);

            // 패럴러 인보크 : 넣어주는 액션만큼을 Task를 만들어서 실행해줌.
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);
        }
    }
}

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
        volatile static bool _stop = false;

        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작");

            while(_stop == false)
            {
                // 누군가가 stop 신호를 해주기를 기다린다.
            }

            Console.WriteLine("쓰레드 종료");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);// 쓰레드가 쓰레드 풀에 있던 쓰레드를 이용해서 일감 하나를 분배받아서 실행.
            t.Start();

            // 1초동안 대기.
            Thread.Sleep(1000); // ms 단위 10^-3초. 즉 1초 = 1000ms

            _stop = true; // 1초후 stop이 true로 바꿔줌

            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");
            t.Wait(); // 쓰레드 클래스에서 join과 같은역할임. // 쓰레드가 끝났는지 알아옴.(t 쓰레드 끝날때까지 기다림)
            Console.WriteLine("종료 성공");
        }
    }
}

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
        // state를 반드시 사용할 필요는 없음. // static으로 하는 이유는 메인함수가 static이기 때문에 static 함수가 아니면 메인함수에서 사용할수 없기 때문.
        static void MainThread(object state)
        {
            for (int i = 0; i < 5; ++i)
                Console.WriteLine("Hello Thread!");
        }

        // 메인 직원.
        static void Main(string[] args)
        {
            // C#에서는 Thread를 직접 관리하는 일이 적다. 웬만해서 ThreadPool을 통해 최대한 이용함. 혹은 Task로 만들어서 처리하는편.
            ThreadPool.SetMinThreads(1, 1); // 첫번째 인자 : 일을 할 대상 , 두번째 인자 : IO관련
            ThreadPool.SetMaxThreads(5, 5); // 최소 1, 최대 5

            for (int i=0; i<5;++i)
            {
                // 옵션으로 LongRunning을 넣으면 쓰레드 풀에 들어가도, 엄청 오래 걸릴 일이라는걸 미리 알림. 결론 : 별도 처리 해줌. Thread, ThreadPool의 각각의 장점을 가져온 느낌임.
                Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning); // 직원이 할 일감.
                t.Start();
            }

            for (int i = 0; i < 4; ++i)
                // 람다형태
                ThreadPool.QueueUserWorkItem((obj) => { while (true) { } }); // 영영 돌아오지 않는 일감.

            // 단기알바 개념. 오브젝트 풀링과 비슷한 개념으로 직원들의 대기 집합소 같은 느낌임.
            ThreadPool.QueueUserWorkItem(MainThread); // 최대로 돌릴 수 있는 전체 쓰레드 수를 자체적으로 제한함. 함수도 max, min이 있음.

            /*
             * 쓰레드 숫자와 CPU 코어수를 맞춰주는게 중요하다.
             * 만약 쓰레드를 너무 많이 사용할 경우, 코어가 여러 쓰레드들을 왔다갔다하는 시간이 더 오래걸릴수 있음(오버헤드)
             * 실제 실행시간보다 이동(빙의)하는 시간이 더 걸릴 수 있음. 동시에 실행되게끔 보이려고 함.
             * 
             */
            // 쓰레드를 만드는건 매우 큰 부담.
            // 한명의 직원을 추가로 고용하는 형태
            Thread t2 = new Thread(MainThread); // 쓰레드를 연결함. 하는일 지정.

            // 쓰레드 이름지정
            t2.Name = "Test Thread";

            // background 기준일때 메인함수가 종료되면 남아있는 일이 있어도 종료됨
            t2.IsBackground = true; // 기본적으로 C# 쓰레드는 forground thread라서 메인함수가 끝나도 남아있는 쓰레드가 있으면 계속 진행함. background = false 상태임.

            t2.Start(); // 별도의 쓰레드가 시작됨.

            Console.WriteLine("Waiting for Thread!");
            t2.Join(); // c++도 join임. // 끝날때까지 기다린다는 의미. 끝나고 아래 순서 실행함.

            Console.WriteLine("Hello World!");
            
            while(true)
            {

            }
        }
    }
}

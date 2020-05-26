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
       
        static void Main(string[] args)
        {
            int[,] arr = new int[10000, 10000];

            {
                // 시간 기록함수
                long now = DateTime.Now.Ticks;

                for (int y = 0; y < 10000; ++y)
                    for (int x = 0; x < 10000; ++x)
                        arr[y, x] = 1;

                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y,x) 순서 걸린 시간 = {end-now}");
            }
            // 기존의 예상이라면, 똑같은 시간이 걸려야할 것이다.
            {
                long now = DateTime.Now.Ticks;

                for (int y = 0; y < 10000; ++y)
                    for (int x = 0; x < 10000; ++x)
                        arr[x, y] = 1;

                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(x,y) 순서 걸린 시간 = {end - now}");
            }
        }
    }
}

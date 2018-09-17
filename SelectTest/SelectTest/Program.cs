using System;
using System.Collections.Generic;
using System.Linq;

namespace SelectTest
{
    class A {
        int i;
        public string a;
        public A(int i, string a) {
            this.i = i;
            this.a = a;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //List<int> Scores = new List<int>();
            //for (var i = 0; i < 50; i++) {
            //    Scores.Add(i);
            //}
            //IEnumerable<int> SelectHigh = Scores.Select(q=>q*q);
            //foreach (var i in SelectHigh) {
            //    Console.WriteLine(i);
            //}


            List<A> Scores = new List<A>();
            for (var i = 0; i < 50; i++)
            {
                Scores.Add(new A(i, "i" + i));
            }
            IEnumerable<string> SelectHigh = Scores.Select(q => q.a);
            foreach (var i in SelectHigh)
            {
                Console.WriteLine(i);
            }
            Console.ReadKey();
        }
    }
}

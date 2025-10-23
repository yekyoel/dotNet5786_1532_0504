using System;

namespace Stage0
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Welcome1532();
            Welcome0504();
            Console.ReadKey();
        }

        private static void Welcome1532()
        {
            Console.WriteLine("Enter your name: ");
            string name = Console.ReadLine();
            Console.WriteLine("{0}, welcome to my first console application", name);
        }

        static partial void Welcome0504();
    }
}
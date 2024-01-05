using System;

namespace HelloWorld
{
     class Hello : SuperClass, Superinterface {

     private string privateField;
     int lol;
     public Hello(int x, int y)
         {
             Class1 varrr = new Class1("test");
         }
         ~Hello()
             {
                 // Cleanup code
             }
        public static void Main(string[] args)
        {
            Console.WriteLine( a + 5 * 33, new MyClass("hi",55));
            Class1 varrr = new Class1("test");

        }
        int test(int a,  int b){
            int k;
            a = 2;
            a++;
            a+= 5;
            int t = 4 + 33 / 18;
            t = 6 - 7 * 8 / a;
            int time = 22;
            while (time < 5)
            {
              Console.WriteLine(i);
            }
        for (int i = 1; i <= 5; i++)
        {
            Console.WriteLine(i);
            if (True)
                continue;
        }
        foreach (int number in numbers)
                {
                    Console.WriteLine(number);
                }
            if (time < 10 && b == 2)
            {
              Console.WriteLine("Good morning.");
            }
            else if (time < 20)
            {
              Console.WriteLine("Good day.");
            }
            else
            {
              Console.WriteLine("Good evening.");
            }
            switch (day)
            {
              case 1:
                Console.WriteLine("Monday");
                x = x+1;
                break;
              case 2:
                Console.WriteLine("Tuesday");
              default:
                break;
            }
            return t;
        }
    }
    abstract class testttAbstract {
            public abstract void doWork();

        }
    public interface Superinterface
            {
                double CalculateArea();
                //string ShapeName { get; }
            }
}
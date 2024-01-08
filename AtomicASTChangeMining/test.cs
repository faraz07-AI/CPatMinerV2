using System;
using System.Reflection;

namespace HelloWorld
{
public enum DaysOfWeek
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }
     class Hello : SuperClass, Superinterface {

     private string privateField;
     int lol;

     public Hello(int x, int y)
         {
             Level myVar = Level.Medium;

             try
                     {
                         throw new DivideByZeroException("oh no.");
                     }
                     catch (DivideByZeroException ex)
                     {
                         Console.WriteLine("lol" + ex.Message);
                     }
                     finally
                     {
                         Console.WriteLine("This block will always be executed.");
                     }
             using (StreamReader sr = new StreamReader("TestFile.txt")) {
             //do smthg
             }
         }
         ~Hello()
             {
                 // Cleanup code
             }
        public static unsafe void main()
        {
            Console.WriteLine( a + 5 * 33, new MyClass("hi",55));
            Class1 varrr = new Class1("test");
            unsafe {
            a = a+ b
            // unsafe code
            }

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
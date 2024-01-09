using System;
using System.Reflection;

// parenthesis - ex.lol.test....
delegate int NumberChanger(int n);
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

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
    [Author("Jane Programmer", Version = 2), IsTested()]
     class Hello : SuperClass, Superinterface {

     private string privateField;
     int lol;
     public event SampleEventHandler SampleEvent;
     public double Hours {
         get;
         set { seconds = value * 3600; }
     }


     public Hello(int x, int y)
         {
             Level myVar = Level.Medium;
             checked {
                 int i3 = 2147483647 + ten;
                 Console.WriteLine(i3);
             }
            ;
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
             fixed (int* p = &pt.x) {
                 //*p = 1;
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
            }
            lock (thisLock) {
            a = a+ b
            }

        }
        int test(int a,  int b){
            int k;
            a = k ? x : y;
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
            do {
                y = test( x );
            } while ( x > 0 );

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
              goto stop;
            }
            switch (day)
            {
              case 1:
                Console.WriteLine("Monday");
                x = x+1;
                break;
              case 2:
                Console.WriteLine("Tuesday");
                goto case 1;
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
            }
}
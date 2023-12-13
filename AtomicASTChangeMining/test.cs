using System;
using System.Threading.Tasks;

namespace MyNamespace
{
    class Program
    {
        private const int MaxIterations = 5;

        static async Task Main()
        {
            Console.WriteLine("Hello, World!");

            MyClass myObject = new MyClass();
            myObject.DisplayMessage();

            // For loop with const
            for (int i = 0; i < MaxIterations; i++)
            {
                Console.WriteLine($"Iteration {i + 1}");
            }

            // While loop
            int counter = 0;
            counter = 1;
            while (counter < 3)
            {
                Console.WriteLine($"While loop iteration {counter + 1}");
                counter++;
            }

            // If statement
            int number = 10;
            if (number > 5)
            {
                Console.WriteLine("The number is greater than 5.");
            }
            else if (number == 5)
            {
                Console.WriteLine("The number is equal to 5.");
            }
            else
            {
                Console.WriteLine("The number is less than 5.");
            }

            // Async method
            await PerformAsyncOperation();
        }

        internal class MyClass
        {
            private int myField;

            public MyClass()
            {
                myField = 42;
            }

            public void DisplayMessage(string s, int k)
            {
            int l = 5;
                k = k+ 1;
                Console.WriteLine($"My field value is: {myField}");
            }
        }

        internal static async Task PerformAsyncOperation(int a, int b)
        {
            Console.WriteLine("Async operation in progress...");
            await Task.Delay(2000); // Simulating an asynchronous operation
            Console.WriteLine("Async operation completed.");
        }
    }
}

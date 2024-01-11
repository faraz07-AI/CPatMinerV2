using System;
using System.Reflection;

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

    interface IContravariant<in A> { }
    class Conversion<out A> { }
    class GenericList<T> where T : Employee { }
    class Foo<T> where T : new() { }
    class Foo<T> where T : class {
    Foo<T> foo<X>(X x) where X : struct {
     var scoreQuery =
         from score in scores
         join prod in products on category.ID equals prod.CategoryID
         let words = word.ToLower()
         where score > 80 || w[0] == 'e'
         orderby a descending
         group student by student.Last into g
         select score;
     }

}


}
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            dynamic d1 = new ExpandoObject();
            d1.name = "Bob";
            d1.country = "Serbia";
            d1.code = 10;

            dynamic d2 = new ExpandoObject();
            d2.name = "Serguei";
            d2.country = "Latvia";
            d2.code = 11;

            dynamic d3 = new ExpandoObject();
            d3.name = "Antoinette";
            d3.country = "Gabon";
            d3.code = 12;

            var data = new List<dynamic>() { d1, d2, d3 };

            //var GetName = GetGetMemberOnObjectLambda("name");
            //foreach (var d in data)
            //{
            //    Console.WriteLine(GetName(d));
            //}

            int value = 11;
            string attrName = "code";
            //Console.WriteLine("b: " + b);
            var MemberEqualsValue = GetMemberEqualsValueLambda(attrName, value);
            foreach (var d in data)
            {
                Console.WriteLine(MemberEqualsValue(d));
            }

            //var name = Console.ReadLine();
            //while (name != null)
            //{
            //    var result = data.Where(d => d.name == name);

            //    if (result.Any())
            //    {
            //        Console.WriteLine(result.First().country);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Not found");
            //    }

            //    name = Console.ReadLine();
            //}

        }

        static Func<dynamic, dynamic> GetGetMemberOnObjectLambda(string attrName)
        {
            var binder = Binder.GetMember(
                CSharpBinderFlags.None,
                attrName,
                typeof(Program), // or this.GetType() 
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var par = Expression.Parameter(typeof(object));

            Func<dynamic, dynamic> f = Expression.Lambda<Func<dynamic, dynamic>>(
                Expression.Dynamic(binder, typeof(object), par),
                par)
                .Compile();

            return f;
        }

        static Func<dynamic, bool> GetMemberEqualsValueLambda(string attrName, object attrValue)
        {
            var binder = Binder.GetMember(
                CSharpBinderFlags.None,
                attrName,
                typeof(Program), // or this.GetType() 
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var par = Expression.Parameter(typeof(object));

            var member = Expression.Dynamic(binder, typeof(int), par);
            var value = Expression.Constant(attrValue);
            Expression equal = Expression.Equal(member, value);

            Func<dynamic, bool> f = Expression.Lambda<Func<dynamic, bool>>(
                equal,
                par)
                .Compile();

            return f;
        }

    }
}

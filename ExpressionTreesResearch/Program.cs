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

            // 1 - Lambda on dynamic objects are possible
            var name = "Bob";
            //var name = Console.ReadLine();
            //while (name != null)
            {
                var result = data.Where(d => d.name == name);
                if (result.Any())
                {
                    Console.WriteLine($"Found object with name: {result.First().country}");
                }
                else
                {
                    Console.WriteLine("Not found");
                }

            //    name = Console.ReadLine();
            }

            // 2 - Get member is possible
            var GetName = BuildGetMemberOnObjectLambda("name");
            foreach (var d in data)
            {
                Console.WriteLine($"Object name: { GetName(d)}");
            }

            // 3 - Are comparison lambdas possible on dynamic objects?
            // 3.1 on strings
            var bob = "Bo";
            bob += "b"; // Ensuring we get a separate reference.
            Console.WriteLine($"Are refs equals: {Object.ReferenceEquals(bob, d1.name)}");

            var NameIsBob = BuildMemberEqualsValueLambda("name", bob);
            Console.WriteLine("Found Bob? " + data.Any(d => NameIsBob(d)));

            // 3.2 on ints
            var CodeIs11 = BuildMemberEqualsValueLambda("code", 11);
            Console.WriteLine("Found Code 11? " + data.Any(d => CodeIs11(d)));
        }

        static Func<dynamic, dynamic> BuildGetMemberOnObjectLambda(string attrName)
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

        static Func<dynamic, bool> BuildMemberEqualsValueLambda(string attrName, object attrValue)
        {
            var param = Expression.Parameter(typeof(object));

            var binder = Binder.GetMember(
                CSharpBinderFlags.None,
                attrName,
                typeof(Program), // or this.GetType() 
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var member = Expression.Dynamic(
                binder, 
                typeof(object), // Putting return type attrValue.GetType() produces: System.InvalidOperationException: 'The result type 'System.Object' of the binder 'Microsoft.CSharp.RuntimeBinder.CSharpGetMemberBinder' is not compatible with the result type 'System.String' expected by the call site.'
                param); 

            var value = Expression.Constant(attrValue);
            
            var memberConverted = Expression.Convert(member, value.Type);

            Expression equal = Expression.Equal(memberConverted, value);

            Func<dynamic, bool> f = Expression.Lambda<Func<dynamic, bool>>(
                equal,
                param)
                .Compile();

            return f;
        }

    }
}

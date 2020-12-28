using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionTreesResearch
{
    public static class LambdasOnDynamicObjectsFactory
    {
        public static Func<dynamic, dynamic> BuildGetMemberOnObjectLambda(string attrName)
        {
            var binder = Binder.GetMember(
                CSharpBinderFlags.None,
                attrName,
                typeof(ConsoleApp1.Program), // or this.GetType() 
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var par = Expression.Parameter(typeof(object));

            Func<dynamic, dynamic> f = Expression.Lambda<Func<dynamic, dynamic>>(
                Expression.Dynamic(binder, typeof(object), par),
                par)
                .Compile();

            return f;
        }

        public static Func<dynamic, bool> BuildMemberEqualsValueLambda(string attrName, object attrValue)
        {
            var param = Expression.Parameter(typeof(object));

            var binder = Binder.GetMember(
                CSharpBinderFlags.None,
                attrName,
                typeof(ConsoleApp1.Program), // or this.GetType() 
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

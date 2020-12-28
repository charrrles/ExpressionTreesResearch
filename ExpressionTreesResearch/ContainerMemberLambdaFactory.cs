using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionTreesResearch
{
    public static class ContainerMemberLambdaFactory
    {
        static Expression BuildAny<TSource>(ParameterExpression param, Expression<Func<TSource, bool>> predicate)
        {
            var overload = typeof(Queryable).GetMethods()
                                      .Single(mi => mi.Name == "Any" && mi.GetParameters().Length == 2);
            var call = Expression.Call(
                overload,
                Expression.PropertyOrField(param, "Attributes"),
                predicate);

            return call;
        }

        static Expression<Func<IEnumerable<TSource>, bool>> CreateLambdaForAny<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            var source = Expression.Parameter(
                typeof(IEnumerable<TSource>), "source");

            var call = Expression.Call(
                typeof(Enumerable), "Any", new Type[] { typeof(TSource) }, new Expression[] { source, predicate });

            return Expression.Lambda<Func<IEnumerable<TSource>, bool>>(call, source);
        }

        public static Func<IEnumerable<IAttribute>, bool> BuildContainerHoldsAttributeLambda<TSource>(IAttribute targetAttr) where TSource : IAttribute
        {
            var anyLambdaExpr = CreateLambdaForAny<IAttribute>(p => p.Value == targetAttr.Value && p.Name == targetAttr.Name);
            return anyLambdaExpr.Compile();
        }

        public static Func<TSource, bool> BuildContainerMemberHoldsAttributeLambda<TSource>(string fieldName, IAttribute targetAttr)
        {
            var EntityParam = Expression.Parameter(typeof(TSource), "entity");

            var fieldExpression = Expression.PropertyOrField(EntityParam, fieldName);

            Func<TSource, IEnumerable<IAttribute>> accessAttrsMember =
                Expression.Lambda<Func<TSource, IEnumerable<IAttribute>>>(fieldExpression, EntityParam).Compile();
            Func<IEnumerable<IAttribute>, bool> anyLambda =
                CreateLambdaForAny<IAttribute>(p => p.Value == targetAttr.Value && p.Name == targetAttr.Name).Compile();

            return T => anyLambda(accessAttrsMember(T));
        }
    }
}

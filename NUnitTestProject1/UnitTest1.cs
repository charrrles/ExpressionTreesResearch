using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NUnitTestProject1
{
    public interface IAttribute
    { 
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TestAttr : IAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TestObject
    {
        public string Name { get; set; }
        public List<TestAttr> Attributes { get; set; }
    }

    public class Tests
    {
        IQueryable<TestObject> TestData { get; set; }

        [SetUp]
        public void Setup()
        {
            var o1 = new TestObject() { Name = "MyTestData1" };
            o1.Attributes = new List<TestAttr>()
            {
                new TestAttr { Name = "A1", Value = "33"},
                new TestAttr { Name = "A2", Value = "31"},
                new TestAttr { Name = "A3", Value = "32"}
            };
            var o2 = new TestObject() { Name = "MyTestData2" };
            o2.Attributes = new List<TestAttr>()
            {
                new TestAttr { Name = "A1", Value = "43"},
                new TestAttr { Name = "A2", Value = "41"},
                new TestAttr { Name = "A3", Value = "42"},
                new TestAttr { Name = "A4", Value = "44"},
            };
            var x = new List<TestObject> { o1, o2 };

            TestData = x.AsQueryable();
        }

        static Expression BuildAny<TSource>(ParameterExpression param, Expression<Func<TSource, bool>> predicate)
        {
            var overload = typeof(Queryable).GetMethods()
                                      .Single(mi => mi.Name == "Any" && mi.GetParameters().Count() == 2);
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

        Func<IEnumerable<IAttribute>, bool> BuildContainerHoldsAttributeLambda<TSource>( IAttribute targetAttr)
        {
            var anyLambdaExpr = CreateLambdaForAny<IAttribute>(p => p.Value == targetAttr.Value && p.Name == targetAttr.Name);
            return anyLambdaExpr.Compile();
        }

        static Func<TSource, bool> BuildContainerMemberHoldsAttributeLambda<TSource>(string fieldName, IAttribute targetAttr)
        {
            var EntityParam = Expression.Parameter(typeof(TSource), "entity");

            var fieldExpression = Expression.PropertyOrField(EntityParam, fieldName);

            Func<TSource, IEnumerable<IAttribute>> accessAttrsMember = 
                Expression.Lambda<Func<TSource, IEnumerable<IAttribute>>>(fieldExpression, EntityParam).Compile();
            Func<IEnumerable<IAttribute>, bool> anyLambda = 
                CreateLambdaForAny<IAttribute>(p => p.Value == targetAttr.Value && p.Name == targetAttr.Name).Compile();

            return T => anyLambda(accessAttrsMember(T));
        }

        [Test]
        public void ContainerHoldsAttribute()
        {
            TestAttr targetAttr = new TestAttr() { Name = "A1", Value = "33" };
            var containerHolds_A1_33 = BuildContainerHoldsAttributeLambda<TestAttr>(targetAttr);

            int hits = 0;
            foreach (var t in TestData)
            {
                Console.WriteLine("Manual: " + t.Attributes.Any(a => a.Name == targetAttr.Name && a.Value == targetAttr.Value));

                var result = containerHolds_A1_33(t.Attributes);
                Console.WriteLine("Expression: " + result);
                if (result)
                {
                    hits++;
                }
            }

            Assert.AreEqual(1, hits);
        }

        [Test]
        public void ContainerMemberHoldsAttribute()
        {
            TestAttr targetAttr = new TestAttr() { Name = "A1", Value = "33" };
            var toEmulate = TestData.Where(td => td.Attributes.Any(a => a.Name == targetAttr.Name && a.Value == targetAttr.Value));

            var EntityHasAttribute_A1_33 = BuildContainerMemberHoldsAttributeLambda<TestObject>("Attributes", targetAttr);

            var result = TestData.Where(e => EntityHasAttribute_A1_33(e));

            Console.WriteLine("Found object with targetAttr? " + result.Any());

            Assert.IsTrue(result.Count() == 1);
        }
    }
}
using ExpressionTreesResearch;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NUnitTestProject1
{
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

    public class ObjectHoldsKeyValueLambdasTests
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

        /// <summary>
        /// Test that we can build a lambda that mimics 
        /// testObject.Attributes.Any(a => a.Name == targetAttr.Name && a.Value == targetAttr.Value)
        /// </summary>
        [Test]
        public void ContainerHoldsAttribute()
        {
            // Arrange:
            // This simple class has a name and a value (implements IAttribute)
            TestAttr targetAttr = new TestAttr() { Name = "A1", Value = "33" };

            /// Act:
            /// We build a lambda expression that has the ability to verify that the given IAttribute 
            /// (TestAttr in this case), can be found in a linq container (e.g. an IList) that will be
            /// passed when evaluating the lambda.
            var containerHolds_A1_33 = ContainerMemberLambdaFactory.BuildContainerHoldsAttributeLambda<TestAttr>(targetAttr);

            /// Assert:
            /// Only one of the two TestObjects in TestData has the TestAttr with Name = A1 and Value = 33.
            /// We run our lambda on both those objects and assert there is indeed only one.
            int hits = 0;
            foreach (var t in TestData)
            {
                Console.WriteLine("To emulate: " + t.Attributes.Any(a => a.Name == targetAttr.Name && a.Value == targetAttr.Value));

                var result = containerHolds_A1_33(t.Attributes);
                Console.WriteLine("Result from expression: " + result);
                if (result)
                {
                    hits++;
                }
            }
            Assert.AreEqual(1, hits);
        }

        /// <summary>
        /// Test that we can build a lambda that looks for a key-value pair in an object, i.e. that mimics:
        /// TestData.Where(td => td.Attributes.Any(a => a.Name == targetAttr.Name && a.Value == targetAttr.Value)
        /// </summary>
        [Test]
        public void ContainerMemberHoldsAttribute()
        {
            /// Arrange
            /// This simple class has a name and a value (implements IAttribute)
            TestAttr targetAttr = new TestAttr() { Name = "A1", Value = "33" };
            Console.WriteLine("To emulate: " + TestData.Where(td => td.Attributes.Any(a => a.Name == targetAttr.Name && a.Value == targetAttr.Value)));

            /// Act:
            /// We build a lambda expression that has the ability to verify that the objects that will be given to it 
            /// have an attribute that is a container and holds the given targetAttr.
            var EntityHasAttribute_A1_33 = ContainerMemberLambdaFactory.BuildContainerMemberHoldsAttributeLambda<TestObject>("Attributes", targetAttr);
            var result = TestData.Where(e => EntityHasAttribute_A1_33(e));

            Console.WriteLine("Found object with targetAttr? " + result.Any());

            // Assert we found only one such TestObject.
            Assert.IsTrue(result.Count() == 1);
        }
    }
}
using ExpressionTreesResearch;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace NUnitTestProject1
{
    class DynamicObjectsLambdasTests
    {
        List<dynamic> Data;
        dynamic dynamicBob;

        [SetUp]
        public void Setup()
        {
            dynamicBob = new ExpandoObject();
            dynamicBob.name = "Bob";
            dynamicBob.country = "Serbia";
            dynamicBob.code = 10;

            dynamic d2 = new ExpandoObject();
            d2.name = "Serguei";
            d2.country = "Latvia";
            d2.code = 11;

            dynamic d3 = new ExpandoObject();
            d3.name = "Antoinette";
            d3.country = "Gabon";
            d3.code = 12;

            Data = new List<dynamic>() { dynamicBob, d2, d3 };
        }

        [Test]
        public void LambdaOnDynamicObjects_ArePossible()
        {
            var name = "Bob";
            var result = Data.Where(d => d.name == name);
            Assert.IsTrue(result.Any());

            Console.WriteLine($"Found object with name Bob, its country is {result.First().country}.");
        }

        [Test]
        public void GetMemberLambdaOnDynamic_IsPossible()
        {
            var GetName = LambdasOnDynamicObjectsFactory.BuildGetMemberOnObjectLambda("name");
            foreach (var d in Data)
            {
                var name = GetName(d);
                Assert.IsFalse(string.IsNullOrEmpty(name));

                Console.WriteLine($"Object name: { GetName(d)}");
            }
        }

        [Test]
        public void ComparisonLambdasOnDynamicObjectsStringMembers_ArePossible()
        {
            // Make sure we don't fall into compiler optimisation. Ensure we get a separate reference. The first assert fails if we don't do this.
            var bob = "Bo";
            bob += "b";             
            Assert.IsFalse(Object.ReferenceEquals(bob, dynamicBob.name));

            var NameIsBob = LambdasOnDynamicObjectsFactory.BuildMemberEqualsValueLambda("name", bob);
            Assert.IsTrue(Data.Any(d => NameIsBob(d)));
        }

        [Test]
        public void ComparisonLambdasOnDynamicObjectsIntMembers_ArePossible()
        {
            var CodeIs11 = LambdasOnDynamicObjectsFactory.BuildMemberEqualsValueLambda("code", 11);
            Assert.IsTrue(Data.Any(d => CodeIs11(d)));
        }
    }
}

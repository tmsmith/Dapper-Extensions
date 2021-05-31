using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class PredicatesFixture
    {
        public abstract class PredicatesFixtureBase
        {
            protected Mock<ISqlDialect> @SqlDialect;
            protected Mock<ISqlGenerator> Generator;
            protected Mock<IDapperExtensionsConfiguration> Configuration;

            protected virtual object GetParameterValue(object parameter)
            {
                if (parameter is Parameter)
                {
                    return (parameter as Parameter).Value;
                }
                else if (parameter is Func<object>)
                {
                    return (parameter as Func<object>)();
                }
                else
                {
                    return parameter;
                }
            }

            [SetUp]
            public void Setup()
            {
                @SqlDialect = new Mock<ISqlDialect>();
                Generator = new Mock<ISqlGenerator>();
                Configuration = new Mock<IDapperExtensionsConfiguration>();

                @SqlDialect.SetupGet(c => c.ParameterPrefix).Returns('@');
                Configuration.SetupGet(c => c.Dialect).Returns(@SqlDialect.Object);
                Generator.SetupGet(c => c.Configuration).Returns(Configuration.Object);
            }
        }

        [TestFixture]
        public class PredicatesTests : PredicatesFixtureBase
        {
            [Test]
            public void Field_ReturnsSetupPredicate()
            {
                var predicate = Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Like, "Lead", true);
                Assert.AreEqual("Name", predicate.PropertyName);
                Assert.AreEqual(Operator.Like, predicate.Operator);
                Assert.AreEqual("Lead", predicate.Value);
                Assert.AreEqual(true, predicate.Not);
            }

            [Test]
            public void Property_ReturnsSetupPredicate()
            {
                var predicate = Predicates.Property<PredicateTestEntity, PredicateTestEntity2>(f => f.Name, Operator.Le, f => f.Value, true);
                Assert.AreEqual("Name", predicate.PropertyName);
                Assert.AreEqual(Operator.Le, predicate.Operator);
                Assert.AreEqual("Value", predicate.PropertyName2);
                Assert.AreEqual(true, predicate.Not);
            }

            [Test]
            public void Group_ReturnsSetupPredicate()
            {
                Mock<IPredicate> subPredicate = new Mock<IPredicate>();
                var predicate = Predicates.Group(GroupOperator.Or, subPredicate.Object);
                Assert.AreEqual(GroupOperator.Or, predicate.Operator);
                Assert.AreEqual(1, predicate.Predicates.Count);
                Assert.AreEqual(subPredicate.Object, predicate.Predicates[0]);
            }

            [Test]
            public void Exists_ReturnsSetupPredicate()
            {
                Mock<IPredicate> subPredicate = new Mock<IPredicate>();
                var predicate = Predicates.Exists<PredicateTestEntity2>(subPredicate.Object, true);
                Assert.AreEqual(subPredicate.Object, predicate.Predicate);
                Assert.AreEqual(true, predicate.Not);
            }

            [Test]
            public void Between_ReturnsSetupPredicate()
            {
                BetweenValues values = new BetweenValues();
                var predicate = Predicates.Between<PredicateTestEntity>(f => f.Name, values, true);
                Assert.AreEqual("Name", predicate.PropertyName);
                Assert.AreEqual(values, predicate.Value);
                Assert.AreEqual(true, predicate.Not);
            }

            [Test]
            public void Sort__ReturnsSetupPredicate()
            {
                var predicate = Predicates.Sort<PredicateTestEntity>(f => f.Name, false);
                Assert.AreEqual("Name", predicate.PropertyName);
                Assert.AreEqual(false, predicate.Ascending);
            }
        }

        [TestFixture]
        public class BasePredicateTests : PredicatesFixtureBase
        {
            [Test]
            public void GetColumnName_WhenMapNotFound_ThrowsException()
            {
                Mock<BasePredicate> predicate = new Mock<BasePredicate>() { CallBase = true };

                predicate.Protected()
                    .Setup<string>("GetColumnName", ItExpr.IsAny<Type>(), ItExpr.IsAny<ISqlGenerator>(), ItExpr.IsAny<string>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
                    .Throws(new NullReferenceException($"Map was not found for  { It.IsAny<Type>()}"))
                    .Verifiable();

                var ex = Assert.Throws<NullReferenceException>(() => predicate.Object.TestProtected().RunMethod<string>("GetColumnName", typeof(PredicateTestEntity), Generator.Object, "Name", false, true));

                predicate.Verify();

                StringAssert.StartsWith("Map was not found", ex.Message);
            }

            [Test]
            public void GetColumnName_WhenPropertyNotFound_ThrowsException()
            {
                Mock<BasePredicate> predicate = new Mock<BasePredicate>() { CallBase = true };
                const string propertyName = "Name";

                predicate.Protected()
                    .Setup<string>("GetColumnName", ItExpr.IsAny<Type>(), ItExpr.IsAny<ISqlGenerator>(), ItExpr.IsAny<string>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
                    .Throws(new NullReferenceException($"{propertyName} was not found for {It.IsAny<Type>()}"))
                    .Verifiable();

                var ex = Assert.Throws<NullReferenceException>(() => predicate.Object.TestProtected().RunMethod<string>("GetColumnName", typeof(PredicateTestEntity), Generator.Object, propertyName, false, true));

                predicate.Verify();

                StringAssert.StartsWith($"{propertyName} was not found", ex.Message);
            }

            [Test]
            public void GetColumnName_GetsColumnName()
            {
                Mock<BasePredicate> predicate = new Mock<BasePredicate>
                {
                    CallBase = true
                };

                predicate.Protected()
                    .Setup<string>("GetColumnName", ItExpr.IsAny<Type>(), ItExpr.IsAny<ISqlGenerator>(), ItExpr.IsAny<string>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
                    .Returns("foo")
                    .Verifiable();

                var result = predicate.Object.TestProtected().RunMethod<string>("GetColumnName", typeof(PredicateTestEntity), Generator.Object, "Name", false, true);

                predicate.Verify();

                StringAssert.StartsWith("foo", result);
            }
        }

        [TestFixture]
        public class ComparePredicateTests : PredicatesFixtureBase
        {
            [Test]
            public void GetOperatorString_ReturnsOperatorStrings()
            {
                Assert.AreEqual("=", Setup(Operator.Eq, false).Object.GetOperatorString());
                Assert.AreEqual("<>", Setup(Operator.Eq, true).Object.GetOperatorString());
                Assert.AreEqual(">", Setup(Operator.Gt, false).Object.GetOperatorString());
                Assert.AreEqual("<=", Setup(Operator.Gt, true).Object.GetOperatorString());
                Assert.AreEqual(">=", Setup(Operator.Ge, false).Object.GetOperatorString());
                Assert.AreEqual("<", Setup(Operator.Ge, true).Object.GetOperatorString());
                Assert.AreEqual("<", Setup(Operator.Lt, false).Object.GetOperatorString());
                Assert.AreEqual(">=", Setup(Operator.Lt, true).Object.GetOperatorString());
                Assert.AreEqual("<=", Setup(Operator.Le, false).Object.GetOperatorString());
                Assert.AreEqual(">", Setup(Operator.Le, true).Object.GetOperatorString());
                Assert.AreEqual("LIKE", Setup(Operator.Like, false).Object.GetOperatorString());
                Assert.AreEqual("NOT LIKE", Setup(Operator.Like, true).Object.GetOperatorString());
            }

            protected static Mock<ComparePredicate> Setup(Operator op, bool not)
            {
                Mock<ComparePredicate> predicate = new Mock<ComparePredicate>();
                predicate.Object.Operator = op;
                predicate.Object.Not = not;
                predicate.CallBase = true;
                return predicate;
            }
        }

        [TestFixture]
        public class FieldPredicateTests : PredicatesFixtureBase
        {
            [Test]
            public void GetSql_NullValue_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, null, false);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(fooCol IS NULL)", sql);
            }

            [Test]
            public void GetSql_NullValue_Not_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, null, true);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(fooCol IS NOT NULL)", sql);
            }

            [Test]
            public void GetSql_Enumerable_NotEqOperator_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Le, new[] { "foo", "bar" }, false);
                var parameters = new Dictionary<string, object>();

                var ex = Assert.Throws<ArgumentException>(() => predicate.Object.GetSql(Generator.Object, parameters, false));

                predicate.Verify();

                StringAssert.StartsWith("Operator must be set to Eq for Enumerable types", ex.Message);
            }

            [Test]
            public void GetSql_Enumerable_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, new[] { "foo", "bar" }, false);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual("foo", GetParameterValue(parameters["@Name_0"]));
                Assert.AreEqual("bar", GetParameterValue(parameters["@Name_1"]));
                Assert.AreEqual("(fooCol IN (@Name_0, @Name_1))", sql);
            }

            [Test]
            public void GetSql_Enumerable_Not_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, new[] { "foo", "bar" }, true);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual("foo", GetParameterValue(parameters["@Name_0"]));
                Assert.AreEqual("bar", GetParameterValue(parameters["@Name_1"]));
                Assert.AreEqual("(fooCol NOT IN (@Name_0, @Name_1))", sql);
            }

            [Test]
            public void GetSql_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, 12, true);
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(1, GetParameterValue(parameters.Count));
                Assert.AreEqual(12, GetParameterValue(parameters["@Name_0"]));
                Assert.AreEqual("(fooCol ** @Name_0)", sql);
            }

            protected Mock<FieldPredicate<T>> Setup<T>(string propertyName, Operator op, object value, bool not) where T : class
            {
                Configuration.Setup(m => m.GetMap(It.IsAny<Type>())).Returns(new AutoClassMapper<T>());

                Mock<FieldPredicate<T>> predicate = new Mock<FieldPredicate<T>>();
                predicate.Object.PropertyName = propertyName;
                predicate.Object.Operator = op;
                predicate.Object.Not = not;
                predicate.Object.Value = value;
                predicate.CallBase = true;
                predicate.Protected().Setup<string>("GetColumnName", typeof(T), Generator.Object, propertyName, ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns("fooCol").Verifiable();
                return predicate;
            }
        }

        [TestFixture]
        public class PropertyPredicateTests : PredicatesFixtureBase
        {
            [Test]
            public void GetSql_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity, PredicateTestEntity2>("Name", Operator.Eq, "Value", false);
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(fooCol ** fooCol2)", sql);
            }

            protected Mock<PropertyPredicate<T, T2>> Setup<T, T2>(string propertyName, Operator op, string propertyName2, bool not)
                where T : class
                where T2 : class
            {
                Mock<PropertyPredicate<T, T2>> predicate = new Mock<PropertyPredicate<T, T2>>();
                predicate.Object.PropertyName = propertyName;
                predicate.Object.PropertyName2 = propertyName2;
                predicate.Object.Operator = op;
                predicate.Object.Not = not;
                predicate.CallBase = true;
                predicate.Protected().Setup<string>("GetColumnName", typeof(T), Generator.Object, propertyName, ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns("fooCol").Verifiable();
                predicate.Protected().Setup<string>("GetColumnName", typeof(T2), Generator.Object, propertyName2, ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns("fooCol2").Verifiable();
                return predicate;
            }
        }

        [TestFixture]
        public class BetweenPredicateTests : PredicatesFixtureBase
        {

            [Test]
            public void GetSql_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", 12, 20, false);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(12, GetParameterValue(parameters["@Name_0"]));
                Assert.AreEqual(20, GetParameterValue(parameters["@Name_1"]));
                Assert.AreEqual("(fooCol BETWEEN @Name_0 AND @Name_1)", sql);
            }

            [Test]
            public void GetSql_Not_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", 12, 20, true);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(12, GetParameterValue(parameters["@Name_0"]));
                Assert.AreEqual(20, GetParameterValue(parameters["@Name_1"]));
                Assert.AreEqual("(fooCol NOT BETWEEN @Name_0 AND @Name_1)", sql);
            }

            protected Mock<BetweenPredicate<T>> Setup<T>(string propertyName, object value1, object value2, bool not)
                where T : class
            {
                Configuration.Setup(m => m.GetMap(It.IsAny<Type>())).Returns(new AutoClassMapper<T>());

                Mock<BetweenPredicate<T>> predicate = new Mock<BetweenPredicate<T>>();
                predicate.Object.PropertyName = propertyName;
                predicate.Object.Value = new BetweenValues { Value1 = value1, Value2 = value2 };
                predicate.Object.Not = not;
                predicate.CallBase = true;
                predicate.Protected()
                    .Setup<string>("GetColumnName", typeof(T), Generator.Object, propertyName, ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
                    .Returns("fooCol")
                    .Verifiable();
                return predicate;
            }
        }

        [TestFixture]
        public class PredicateGroupTests : PredicatesFixtureBase
        {
            [Test]
            public void EmptyPredicate__CreatesNoOp_And_ReturnsProperSql()
            {
                Mock<IPredicate> subPredicate1 = new Mock<IPredicate>();
                @SqlDialect.SetupGet(s => s.EmptyExpression).Returns("1=1").Verifiable();

                var subPredicates = new List<IPredicate> { subPredicate1.Object, subPredicate1.Object };
                var predicate = Setup(GroupOperator.And, subPredicates);
                var parameters = new Dictionary<string, object>();

                subPredicate1.Setup(s => s.GetSql(Generator.Object, parameters, false)).Returns("").Verifiable();
                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();
                @SqlDialect.Verify();
                subPredicate1.Verify(s => s.GetSql(Generator.Object, parameters, false), Times.AtMost(2));

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(1=1)", sql);
            }

            [Test]
            public void GetSql_And_ReturnsProperSql()
            {
                Mock<IPredicate> subPredicate1 = new Mock<IPredicate>();
                var subPredicates = new List<IPredicate> { subPredicate1.Object, subPredicate1.Object };
                var predicate = Setup(GroupOperator.And, subPredicates);
                var parameters = new Dictionary<string, object>();

                subPredicate1.Setup(s => s.GetSql(Generator.Object, parameters, false)).Returns("subSql").Verifiable();
                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();
                subPredicate1.Verify(s => s.GetSql(Generator.Object, parameters, false), Times.AtMost(2));

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(subSql AND subSql)", sql);
            }

            [Test]
            public void GetSql_Or_ReturnsProperSql()
            {
                Mock<IPredicate> subPredicate1 = new Mock<IPredicate>();
                var subPredicates = new List<IPredicate> { subPredicate1.Object, subPredicate1.Object };
                var predicate = Setup(GroupOperator.Or, subPredicates);
                var parameters = new Dictionary<string, object>();

                subPredicate1.Setup(s => s.GetSql(Generator.Object, parameters, false)).Returns("subSql").Verifiable();
                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();
                subPredicate1.Verify(s => s.GetSql(Generator.Object, parameters, false), Times.AtMost(2));

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(subSql OR subSql)", sql);
            }

            protected static Mock<PredicateGroup> Setup(GroupOperator op, IList<IPredicate> predicates)
            {
                Mock<PredicateGroup> predicate = new Mock<PredicateGroup>();
                predicate.Object.Operator = op;
                predicate.Object.Predicates = predicates;
                predicate.CallBase = true;
                return predicate;
            }
        }

        [TestFixture]
        public class ExistsPredicateTests : PredicatesFixtureBase
        {
            [Test]
            public void GetSql_WithoutNot_ReturnsProperSql()
            {
                Mock<IPredicate> subPredicate = new Mock<IPredicate>();
                Mock<IClassMapper> subMap = new Mock<IClassMapper>();
                var predicate = Setup<PredicateTestEntity2>(subPredicate.Object, subMap.Object, false);
                Generator.Setup(g => g.GetTableName(subMap.Object, It.IsAny<bool>())).Returns("subTable").Verifiable();

                var parameters = new Dictionary<string, object>();

                subPredicate.Setup(s => s.GetSql(Generator.Object, parameters, false)).Returns("subSql").Verifiable();
                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();
                subPredicate.Verify();
                Generator.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(EXISTS (SELECT 1 FROM subTable WHERE subSql))", sql);
            }

            [Test]
            public void GetSql_WithNot_ReturnsProperSql()
            {
                Mock<IPredicate> subPredicate = new Mock<IPredicate>();
                Mock<IClassMapper> subMap = new Mock<IClassMapper>();
                var predicate = Setup<PredicateTestEntity2>(subPredicate.Object, subMap.Object, true);
                Generator.Setup(g => g.GetTableName(subMap.Object, It.IsAny<bool>())).Returns("subTable").Verifiable();

                var parameters = new Dictionary<string, object>();

                subPredicate.Setup(s => s.GetSql(Generator.Object, parameters, false)).Returns("subSql").Verifiable();
                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();
                subPredicate.Verify();
                Generator.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(NOT EXISTS (SELECT 1 FROM subTable WHERE subSql))", sql);
            }

            [Test]
            public void GetClassMapper_NoMapFound_ThrowsException()
            {
                var predicate = new Mock<ExistsPredicate<PredicateTestEntity>>
                {
                    CallBase = true
                };

                Configuration.Setup(c => c.GetMap(typeof(PredicateTestEntity2))).Returns(() => null).Verifiable();

                var ex = Assert.Throws<NullReferenceException>(() => predicate.Object.TestProtected().RunMethod<IClassMapper>("GetClassMapper", typeof(PredicateTestEntity2), Configuration.Object));

                Configuration.Verify();

                StringAssert.StartsWith("Map was not found", ex.Message);
            }

            [Test]
            public void GetClassMapper_ReturnsMap()
            {
                var classMap = new Mock<IClassMapper>();
                var predicate = new Mock<ExistsPredicate<PredicateTestEntity>>
                {
                    CallBase = true
                };

                Configuration.Setup(c => c.GetMap(typeof(PredicateTestEntity2))).Returns(classMap.Object).Verifiable();

                var result = predicate.Object.TestProtected().RunMethod<IClassMapper>("GetClassMapper", typeof(PredicateTestEntity2), Configuration.Object);

                Configuration.Verify();

                Assert.AreEqual(classMap.Object, result);
            }

            protected Mock<ExistsPredicate<T>> Setup<T>(IPredicate predicate, IClassMapper classMap, bool not) where T : class
            {
                Configuration.Setup(m => m.GetMap(It.IsAny<Type>())).Returns(new AutoClassMapper<T>());

                Mock<ExistsPredicate<T>> result = new Mock<ExistsPredicate<T>>();
                result.Object.Predicate = predicate;
                result.Object.Not = not;
                result.Protected().Setup<IClassMapper>("GetClassMapper", typeof(T), Configuration.Object).Returns(classMap).Verifiable();
                result.CallBase = true;
                return result;
            }
        }

        public class PredicateTestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class PredicateTestEntity2
        {
            public int Key { get; set; }
            public string Value { get; set; }
        }
    }
}
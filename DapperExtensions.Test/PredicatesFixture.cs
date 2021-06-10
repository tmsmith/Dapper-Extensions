using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DapperExtensions.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class PredicatesFixture
    {
        [ExcludeFromCodeCoverage]
        public class PredicateTestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [ExcludeFromCodeCoverage]
        public class PredicateTestEntity2
        {
            public int Key { get; set; }
            public string Value { get; set; }
        }

        [ExcludeFromCodeCoverage]
        public class PredicateTestEntity3
        {
            public Guid Id { get; set; }
            public int TestEntity1Id { get; set; }
            public int TestEntity2Key { get; set; }
            public PredicateTestEntity TestEntity1 { get; set; }
            public PredicateTestEntity2 TestEntity2 { get; set; }
        }

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

                @SqlDialect
                    .Setup(s => s.GetDatabaseFunctionString(It.IsAny<DatabaseFunction>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns((DatabaseFunction dbFunc, string columnName, string dbFuncParams) => dbFunc == DatabaseFunction.NullValue ?
                        $"IsNull({columnName}, {dbFuncParams})" : dbFunc == DatabaseFunction.Truncate ?
                        $"Truncate({columnName})" : columnName);
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
            public void Sort_ReturnsSetupPredicate()
            {
                var predicate = Predicates.Sort<PredicateTestEntity>(f => f.Name, false);
                Assert.AreEqual("Name", predicate.PropertyName);
                Assert.AreEqual(false, predicate.Ascending);
            }

            [Test]
            public void In_ReturnsSetupPredicate()
            {
                ICollection values = new List<long> { 1, 2 };
                var predicate = Predicates.In<PredicateTestEntity>(f => f.Id, values);
                Assert.AreEqual("Id", predicate.PropertyName);
                Assert.AreEqual(values, predicate.Collection);
                Assert.AreEqual(false, predicate.Not);
            }
        }

        [TestFixture]
        public class BasePredicateTests : PredicatesFixtureBase
        {
            [Test]
            public void GetColumnName_WhenMapNotFound_ThrowsException()
            {
                Mock<BasePredicate> predicate = new Mock<BasePredicate>() { CallBase = true };

                Configuration.Setup(c => c.GetMap(It.IsAny<Type>())).Returns(() => null).Verifiable();

                Action action = () => predicate.Object.TestProtected().RunMethod<string>("GetColumnName", typeof(PredicateTestEntity), Generator.Object, "Name", false, true);

                action.Should()
                    .Throw<NullReferenceException>()
                    .WithMessage("Map was not found*");

                Configuration.Verify();
            }

            [Test]
            public void GetColumnName_WhenPropertyNotFound_ThrowsException()
            {
                Mock<BasePredicate> predicate = new Mock<BasePredicate>() { CallBase = true };

                Configuration.Setup(c => c.GetMap(It.IsAny<Type>())).Returns(() => new ClassMapper<PredicateTestEntity>()).Verifiable();

                const string propertyName = "Name";

                Action action = () => predicate.Object.TestProtected().RunMethod<string>("GetColumnName", typeof(PredicateTestEntity), Generator.Object, propertyName, false, true);

                action.Should()
                    .Throw<NullReferenceException>()
                    .WithMessage($"{propertyName} was not found*");

                Configuration.Verify();
            }

            [Test]
            public void GetColumnName_GetsColumnName()
            {
                Mock<BasePredicate> predicate = new Mock<BasePredicate> { CallBase = true };

                Configuration.Setup(c => c.GetMap(It.IsAny<Type>())).Returns(() => new AutoClassMapper<PredicateTestEntity>());

                const string propertyName = "Name";

                Generator.Setup(g => g.GetColumnName(It.IsAny<IClassMapper>(), It.IsAny<IMemberMap>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns("foo").Verifiable();

                var result = predicate.Object.TestProtected().RunMethod<string>("GetColumnName", typeof(PredicateTestEntity), Generator.Object, propertyName, false, true);

                Generator.Verify();

                result.Should().Be("foo");
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
                Assert.AreEqual("IN", Setup(Operator.Contains, false).Object.GetOperatorString());
                Assert.AreEqual("NOT IN", Setup(Operator.Contains, true).Object.GetOperatorString());
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

                Generator.Verify();

                parameters.Should().HaveCount(0);
                sql.Should().Be("(Name IS NULL)");
            }

            [Test]
            public void GetSql_NullValue_Not_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, null, true);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                Generator.Verify();

                parameters.Should().HaveCount(0);
                sql.Should().Be("(Name IS NOT NULL)");
            }

            [Test]
            public void GetSql_Enumerable_NotEqOperator_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Le, new[] { "foo", "bar" }, false);
                var parameters = new Dictionary<string, object>();

                Action action = () => predicate.Object.GetSql(Generator.Object, parameters, false);

                action.Should().Throw<ArgumentException>()
                    .WithMessage("Operator must be set to Eq for Enumerable types*");

                Generator.Verify();
            }

            [Test]
            public void GetSql_Enumerable_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, new[] { "foo", "bar" }, false);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                Generator.Verify();

                parameters.Should().HaveCount(2);
                GetParameterValue(parameters["@Name_0"]).Should().Be("foo");
                GetParameterValue(parameters["@Name_1"]).Should().Be("bar");
                sql.Should().Be("(Name IN (@Name_0, @Name_1))");
            }

            [Test]
            public void GetSql_Enumerable_Not_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, new[] { "foo", "bar" }, true);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                Generator.Verify();

                parameters.Should().HaveCount(2);
                GetParameterValue(parameters["@Name_0"]).Should().Be("foo");
                GetParameterValue(parameters["@Name_1"]).Should().Be("bar");
                sql.Should().Be("(Name NOT IN (@Name_0, @Name_1))");
            }

            [Test]
            public void GetSql_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, 12, true);
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                Generator.Verify();

                parameters.Should().HaveCount(1);
                GetParameterValue(parameters["@Name_0"]).Should().Be(12);
                sql.Should().Be("(Name ** @Name_0)");
            }

            [Test]
            public void GetSql_DatabaseFunctionTruncate_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Id", Operator.Contains, 10, false, DatabaseFunction.Truncate, string.Empty);
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                Generator.Verify();

                parameters.Should().HaveCount(1);
                GetParameterValue(parameters["@Id_0"]).Should().Be(10);
                sql.Should().Be("(Truncate(Id) ** @Id_0)");
            }

            [Test]
            public void GetSql_DatabaseFunctionNullValue_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Id", Operator.Contains, 10, false, DatabaseFunction.NullValue, "0");
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                Generator.Verify();

                parameters.Should().HaveCount(1);
                GetParameterValue(parameters["@Id_0"]).Should().Be(10);
                sql.Should().Be("(IsNull(Id, 0) ** @Id_0)");
            }

            [Test]
            public void GetSql_ChainedProperty_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity3>("TestEntity1.Id", Operator.Eq, 10, false);
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                Generator.Setup(m => m.GetColumnName(It.IsAny<IClassMapper>(), It.IsAny<IMemberMap>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns("y2.Id").Verifiable();

                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                Generator.Verify();

                parameters.Should().HaveCount(1);
                GetParameterValue(parameters["@Id_0"]).Should().Be(10);
                sql.Should().Be("(y2.Id ** @Id_0)");
            }

            protected Mock<FieldPredicate<T>> Setup<T>(string propertyName, Operator op, object value, bool not, DatabaseFunction databaseFunction = DatabaseFunction.None, string functionParameters = null) where T : class
            {
                Configuration.Setup(m => m.GetMap(It.IsAny<Type>())).Returns(new AutoClassMapper<T>());

                var properties = ReflectionHelper.GetNestedProperties<T>(propertyName, '.', out string propertyInfoName);
                var columnName = properties.Last().Name;

                Mock<FieldPredicate<T>> predicate = new Mock<FieldPredicate<T>>();
                predicate.Object.PropertyName = propertyInfoName;
                predicate.Object.Operator = op;
                predicate.Object.Not = not;
                predicate.Object.Value = value;
                predicate.Object.DatabaseFunction = databaseFunction;
                predicate.Object.DatabaseFunctionParameters = functionParameters;
                predicate.Object.Properties = properties;
                predicate.CallBase = true;

                Generator.Setup(m => m.GetColumnName(It.IsAny<IClassMapper>(), It.IsAny<IMemberMap>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(columnName).Verifiable();

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

            [Test]
            public void GetSql_LeftPropertyWithDatabaseFunction_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity, PredicateTestEntity2>("Name", Operator.Eq, "Value", false, DatabaseFunction.NullValue, "Empty");
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(IsNull(fooCol, Empty) ** fooCol2)", sql);
            }

            [Test]
            public void GetSql_RightPropertyWithDatabaseFunction_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity, PredicateTestEntity2>("Name", Operator.Eq, "Value", false, 
                    rightDatabaseFunction: DatabaseFunction.NullValue, rightFunctionParameters: "Empty");
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(fooCol ** IsNull(fooCol2, Empty))", sql);
            }

            [Test]
            public void GetSql_BothtPropertyWithDatabaseFunction_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity, PredicateTestEntity2>("Name", Operator.Eq, "Value", false, 
                    DatabaseFunction.NullValue, "Empty", DatabaseFunction.NullValue, "Empty");
                predicate.Setup(p => p.GetOperatorString()).Returns("**").Verifiable();
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(0, parameters.Count);
                Assert.AreEqual("(IsNull(fooCol, Empty) ** IsNull(fooCol2, Empty))", sql);
            }

            protected Mock<PropertyPredicate<T, T2>> Setup<T, T2>(string propertyName, Operator op, string propertyName2, bool not, 
                DatabaseFunction leftDatabaseFunction = DatabaseFunction.None, string leftFunctionParameters = null,
                DatabaseFunction rightDatabaseFunction = DatabaseFunction.None, string rightFunctionParameters = null)
                where T : class
                where T2 : class
            {
                Mock<PropertyPredicate<T, T2>> predicate = new Mock<PropertyPredicate<T, T2>>();
                predicate.Object.PropertyName = propertyName;
                predicate.Object.PropertyName2 = propertyName2;
                predicate.Object.Operator = op;
                predicate.Object.Not = not;
                predicate.CallBase = true;
                predicate.Object.LeftDatabaseFunction = leftDatabaseFunction;
                predicate.Object.LeftDatabaseFunctionParameters = leftFunctionParameters;
                predicate.Object.RigthDatabaseFunction = rightDatabaseFunction;
                predicate.Object.RigthDatabaseFunctionParameters = rightFunctionParameters;
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

                Assert.AreEqual(2, GetParameterValue(parameters.Count)); // It will force test to pass thru the ELSE in GetParameterValue
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

                Assert.AreEqual(2, GetParameterValue(parameters.Count)); // It will force test to pass thru the ELSE in GetParameterValue
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

                var result = new Mock<ExistsPredicate<T>>();
                result.Object.Predicate = predicate;
                result.Object.Not = not;
                result.Protected().Setup<IClassMapper>("GetClassMapper", typeof(T), Configuration.Object).Returns(classMap).Verifiable();
                result.CallBase = true;
                return result;
            }
        }

        [TestFixture]
        public class InPredicateTests : PredicatesFixtureBase
        {

            [Test]
            public void GetSql_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Id", false, 1, 2, 3);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(3, parameters.Count);
                Assert.AreEqual(1, GetParameterValue(parameters["@Id_0"]));
                Assert.AreEqual(2, GetParameterValue(parameters["@Id_1"]));
                Assert.AreEqual(3, GetParameterValue(parameters["@Id_2"]));
                Assert.IsFalse(predicate.Object.Not);
                Assert.IsTrue(sql.Equals("(fooCol in (@Id_0, @Id_1, @Id_2))", StringComparison.InvariantCultureIgnoreCase));
            }

            [Test]
            public void GetSql_Not_ReturnsProperSql()
            {
                var predicate = Setup<PredicateTestEntity>("Id", true, 12, 20);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.Object.GetSql(Generator.Object, parameters, false);

                predicate.Verify();

                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual(12, GetParameterValue(parameters["@Id_0"]));
                Assert.AreEqual(20, GetParameterValue(parameters["@Id_1"]));
                Assert.IsTrue(predicate.Object.Not);
                Assert.IsTrue(sql.Equals("(fooCol NOT IN (@Id_0, @Id_1))", StringComparison.InvariantCultureIgnoreCase));
            }

            protected Mock<InPredicate<T>> Setup<T>(string propertyName, bool not, params long[] values)
                where T : class
            {
                Configuration.Setup(m => m.GetMap(It.IsAny<Type>())).Returns(new AutoClassMapper<T>());

                var predicate = new Mock<InPredicate<T>>(values, propertyName, not)
                {
                    CallBase = true
                };
                predicate.Protected()
                    .Setup<string>("GetColumnName", typeof(T), Generator.Object, propertyName, ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
                    .Returns("fooCol")
                    .Verifiable();
                return predicate;
            }
        }

        [TestFixture]
        public class VirtualPredicateTests : PredicatesFixtureBase
        {
            [Test]
            public void GetSql_ParameterizedConstructor_ReturnsProperSql()
            {
                var predicate = Predicates.VirtualPredicate("Rownum", Operator.Le, 100, false);
                var parameters = new Dictionary<string, object>();

                var sql = predicate.GetSql(Generator.Object, parameters, false);

                Assert.AreEqual(1, parameters.Count);
                Assert.AreEqual("(Rownum <= @Rownum_0)", sql);
            }

            [Test]
            public void GetSql_DefaultConstructor_ReturnsProperSql()
            {
                var predicate = new VirtualPredicate
                {
                    Comparable = "Rownum",
                    Operator = Operator.Le,
                    Not = false,
                    Value = 100
                };
                var parameters = new Dictionary<string, object>();

                var sql = predicate.GetSql(Generator.Object, parameters, false);

                Assert.AreEqual(1, parameters.Count);
                Assert.AreEqual("(Rownum <= @Rownum_0)", sql);
            }


            [Test]
            public void GetSql_FuncParameter_ReturnsProperSql()
            {
                Func<object> func = () => 100;
                var predicate = new VirtualPredicate
                {
                    Comparable = "Rownum",
                    Operator = Operator.Le,
                    Not = false,
                    Value = func
                };
                var parameters = new Dictionary<string, object>();

                var sql = predicate.GetSql(Generator.Object, parameters, false);

                Assert.AreEqual(1, parameters.Count);
                Assert.AreEqual("(Rownum <= @Rownum_0)", sql);
            }
        }
    }
}
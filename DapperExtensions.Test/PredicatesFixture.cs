using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class PredicatesFixture
    {
        public class PredicatesTests
        {
            [Test]
            public void Field_ReturnsSetupPredicate()
            {
                var pred = Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Eq, "Lead");
                Dictionary<string, object> parameters = new Dictionary<string, object>
                                                        {
                                                            { "Field", "123" }
                                                        };
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Name] = @Name_1)", result);
                Assert.AreEqual("Lead", parameters["@Name_1"]);
            }

            [Test]
            public void Group_ReturnsSetupPredicate()
            {
                var pred = Predicates.Group(GroupOperator.And,
                    Predicates.Field<PredicateTestEntity>(f => f.Id, Operator.Gt, 5),
                    Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Eq, "foo"));

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("(([PredicateTestEntity].[Id] > @Id_0) AND ([PredicateTestEntity].[Name] = @Name_1))", result);
                Assert.AreEqual(5, parameters["@Id_0"]);
                Assert.AreEqual("foo", parameters["@Name_1"]);
            }

            [Test]
            public void Property_ReturnsSetupPredicate()
            {
                var pred = Predicates.Property<PredicateTestEntity, PredicateTestEntity2>(f => f.Name, Operator.Eq, f => f.Value);
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Name] = [PredicateTestEntity2].[Value])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void Exists_ReturnsSetupPredicate()
            {
                var subPred = Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Eq, "Lead");
                var pred = Predicates.Exists<PredicateTestEntity2>(subPred, true);
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("(NOT EXISTS (SELECT 1 FROM [PredicateTestEntity2] WHERE ([PredicateTestEntity].[Name] = @Name_0)))", result);
                Assert.AreEqual(1, parameters.Count);
                Assert.AreEqual("Lead", parameters["@Name_0"]);
            }

            [Test]
            public void Between_ReturnsSetupPredicate()
            {
                BetweenValues values = new BetweenValues { Value1 = 12, Value2 = 24 };
                var pred = Predicates.Between<PredicateTestEntity>(f => f.Id, values, true);
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] NOT BETWEEN @Id_0 AND @Id_1)", result);
                Assert.AreEqual(12, parameters["@Id_0"]);
                Assert.AreEqual(24, parameters["@Id_1"]);
            }

            [Test]
            public void Sort_ReturnsSetupPredicate()
            {
                var sort = Predicates.Sort<PredicateTestEntity>(f => f.Name, false);
                Assert.AreEqual("Name", sort.PropertyName);
                Assert.IsFalse(sort.Ascending);
            }
        }

        public class FieldPredicateTests
        {
            [Test]
            public void Eq_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = false,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] = @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void Eq_ReturnsProperSqlWhenString()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = "Foo",
                                   Not = false,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] = @Id_0)", result);
                Assert.AreEqual("Foo", parameters["@Id_0"]);
            }

            [Test]
            public void Eq_ReturnsProperSqlWhenEnumerableOf_tring()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = new[] {"Alpha", "Beta", "Gamma", "Delta"},
                                   Not = false,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] IN (@Id_0, @Id_1, @Id_2, @Id_3))", result);
                Assert.AreEqual("Alpha", parameters["@Id_0"]);
                Assert.AreEqual("Beta", parameters["@Id_1"]);
                Assert.AreEqual("Gamma", parameters["@Id_2"]);
                Assert.AreEqual("Delta", parameters["@Id_3"]);
            }

            [Test]
            public void EqWithNot_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = true,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] <> @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void Eq_EnumerableReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = new[] {3, 4, 5},
                                   Not = false,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] IN (@Id_0, @Id_1, @Id_2))", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
                Assert.AreEqual(4, parameters["@Id_1"]);
                Assert.AreEqual(5, parameters["@Id_2"]);
            }

            [Test]
            public void EqWithNot_EnumerableReturns_Proper_Sql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = new[] {3, 4, 5},
                                   Not = true,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] NOT IN (@Id_0, @Id_1, @Id_2))", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
                Assert.AreEqual(4, parameters["@Id_1"]);
                Assert.AreEqual(5, parameters["@Id_2"]);
            }

            [Test]
            public void EnumerableThrowsException_If_Operator_Not_Eq()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = new[] {3, 4, 5},
                                   Not = false,
                                   Operator = Operator.Ge
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var ex = Assert.Throws<ArgumentException>(() => pred.GetSql(parameters));
                Assert.AreEqual("Operator must be set to Eq for Enumerable types", ex.Message);
            }

            [Test]
            public void EqWithNull_ReturnsPropertySql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                {
                    PropertyName = "Name",
                    Value = null,
                    Not = false,
                    Operator = Operator.Eq
                };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Name] IS NULL)", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void EqWithNullAndNot_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                {
                    PropertyName = "Name",
                    Value = null,
                    Not = true,
                    Operator = Operator.Eq
                };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Name] IS NOT NULL)", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void Gt_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = false,
                                   Operator = Operator.Gt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] > @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void GtWithNot_ReturnsProper_Sql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = true,
                                   Operator = Operator.Gt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] <= @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void Ge_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = false,
                                   Operator = Operator.Ge
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] >= @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void GeWithNot_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = true,
                                   Operator = Operator.Ge
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] < @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void Lt_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = false,
                                   Operator = Operator.Lt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] < @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void LtWithNot_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = true,
                                   Operator = Operator.Lt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] >= @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void Le_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = false,
                                   Operator = Operator.Le
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] <= @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void LeWithNot_ReturnsProperSql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = 3,
                                   Not = true,
                                   Operator = Operator.Le
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] > @Id_0)", result);
                Assert.AreEqual(3, parameters["@Id_0"]);
            }

            [Test]
            public void Like_ReturnsPropertySql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Name",
                                   Value = "%foo",
                                   Not = false,
                                   Operator = Operator.Like
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Name] LIKE @Name_0)", result);
                Assert.AreEqual("%foo", parameters["@Name_0"]);
            }

            [Test]
            public void LikeWithNot_ReturnsPropertySql()
            {
                var pred = new FieldPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Name",
                                   Value = "%foo",
                                   Not = true,
                                   Operator = Operator.Like
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Name] NOT LIKE @Name_0)", result);
                Assert.AreEqual("%foo", parameters["@Name_0"]);
            }
        }

        public class GroupPredicateTests
        {
            [Test]
            public void GroupPredicate_IncludesAllPredicates()
            {
                var pred = new PredicateGroup
                               {
                                   Operator = GroupOperator.And,
                                   Predicates = new List<IPredicate>
                                                    {
                                                        new TestPredicate("one"),
                                                        new TestPredicate("two")
                                                    }
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var result = pred.GetSql(parameters);
                Assert.AreEqual("(one AND two)", result);
            }

            [Test]
            public void GroupPredicate_IncludesAllSubPredicateGroup()
            {
                var pred = new PredicateGroup
                               {
                                   Operator = GroupOperator.And,
                                   Predicates = new List<IPredicate>
                                                    {
                                                        new TestPredicate("one"),
                                                        new TestPredicate("two"),
                                                        new PredicateGroup
                                                            {
                                                                Operator = GroupOperator.Or,
                                                                Predicates = new List<IPredicate>
                                                                                 {
                                                                                     new TestPredicate("three"),
                                                                                     new TestPredicate("four"),
                                                                                 }
                                                            }
                                                    }
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var result = pred.GetSql(parameters);
                Assert.AreEqual("(one AND two AND (three OR four))", result);
            }
        }

        public class PropertyPredicateTests
        {
            [Test]
            public void Eq_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = false,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] = [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void EqWithNot_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = true,
                                   Operator = Operator.Eq
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] <> [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void Gt_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = false,
                                   Operator = Operator.Gt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] > [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void GtWithNot_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = true,
                                   Operator = Operator.Gt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] <= [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void Ge_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = false,
                                   Operator = Operator.Ge
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] >= [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void GeWithNot_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = true,
                                   Operator = Operator.Ge
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] < [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void Lt_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = false,
                                   Operator = Operator.Lt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] < [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void LtWithNot_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = true,
                                   Operator = Operator.Lt
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] >= [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void Le_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = false,
                                   Operator = Operator.Le
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] <= [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void LeWithNot_ReturnsProperSql()
            {
                var pred = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                               {
                                   PropertyName = "Id",
                                   PropertyName2 = "Key",
                                   Not = true,
                                   Operator = Operator.Le
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] > [PredicateTestEntity2].[Key])", result);
                Assert.AreEqual(0, parameters.Count);
            }
        }

        public class ExistsPredicateTests
        {
            [Test]
            public void ExistsPredicate_ReturnsProperSql()
            {
                var pred = new ExistsPredicate<PredicateTestEntity2>
                               {
                                   Predicate = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                                                   {
                                                       PropertyName = "Id",
                                                       PropertyName2 = "Key",
                                                       Not = false,
                                                       Operator = Operator.Eq
                                                   }
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual(
                    "(EXISTS (SELECT 1 FROM [PredicateTestEntity2] WHERE ([PredicateTestEntity].[Id] = [PredicateTestEntity2].[Key])))",
                    result);
                Assert.AreEqual(0, parameters.Count);
            }

            [Test]
            public void ExistsPredicateWithNot_ReturnsProperSql()
            {
                var pred = new ExistsPredicate<PredicateTestEntity2>
                               {
                                   Not = true,
                                   Predicate = new PropertyPredicate<PredicateTestEntity, PredicateTestEntity2>
                                                   {
                                                       PropertyName = "Id",
                                                       PropertyName2 = "Key",
                                                       Not = false,
                                                       Operator = Operator.Eq
                                                   }
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual(
                    "(NOT EXISTS (SELECT 1 FROM [PredicateTestEntity2] WHERE ([PredicateTestEntity].[Id] = [PredicateTestEntity2].[Key])))",
                    result);
                Assert.AreEqual(0, parameters.Count);
            }
        }

        public class BetweenPredicateTests
        {
            [Test]
            public void BetweenPredicate_ReturnsProperSql()
            {
                var pred = new BetweenPredicate<PredicateTestEntity>
                               {
                                   PropertyName = "Id",
                                   Value = new BetweenValues {Value1 = 1, Value2 = 10}
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] BETWEEN @Id_0 AND @Id_1)", result);
                Assert.AreEqual(2, parameters.Count);
            }

            [Test]
            public void BetweenPredicateWithNot_ReturnsProperSql()
            {
                var pred = new BetweenPredicate<PredicateTestEntity>
                               {
                                   Not = true,
                                   PropertyName = "Id",
                                   Value = new BetweenValues {Value1 = 1, Value2 = 10}
                               };

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string result = pred.GetSql(parameters);
                Assert.AreEqual("([PredicateTestEntity].[Id] NOT BETWEEN @Id_0 AND @Id_1)", result);
                Assert.AreEqual(2, parameters.Count);
            }
        }
        
        private class PredicateTestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class PredicateTestEntity2
        {
            public int Key { get; set; }
            public string Value { get; set; }
        }

        private class TestPredicate : IPredicate
        {
            private string _value;

            public TestPredicate(string value)
            {
                _value = value;
            }

            public string GetSql(IDictionary<string, object> parameters)
            {
                return _value;
            }
        }
    }
}
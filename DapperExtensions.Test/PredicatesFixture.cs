using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class PredicatesFixture : BaseFixture
    {
        [Test]
        public void Predicates_Field_Returns_Setup_Predicate()
        {
            var pred = Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Eq, "Lead");
            Dictionary<string, object> parameters = new Dictionary<string, object>
                                                        {
                                                            { "Field", "123" }
                                                        };
            string result = pred.GetSql(parameters);
            Assert.AreEqual("([PredicateTestEntity].[Name] = @Namep1)", result);
            Assert.AreEqual("Lead", parameters["@Namep1"]);
        }

        [Test]
        public void FieldPredicate_Eq_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] = @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Eq_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] <> @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Gt_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] > @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Gt_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] <= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Ge_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] >= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Ge_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] < @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Lt_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] < @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Lt_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] >= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Le_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] <= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Le_Returns_Proper_Sql()
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
            Assert.AreEqual("([PredicateTestEntity].[Id] > @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_With_Null_Returns_Proper_Sql()
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
        public void FieldPredicate_With_Null_ANd_Not_Returns_Proper_Sql()
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
        public void Predicates_Group_Returns_Setup_Predicate()
        {
            var pred = Predicates.Group(GroupOperator.And,
                Predicates.Field<PredicateTestEntity>(f => f.Id, Operator.Gt, 5),
                Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Eq, "foo"));

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string result = pred.GetSql(parameters);
            Assert.AreEqual("(([PredicateTestEntity].[Id] > @Idp0) AND ([PredicateTestEntity].[Name] = @Namep1))", result);
            Assert.AreEqual(5, parameters["@Idp0"]);
            Assert.AreEqual("foo", parameters["@Namep1"]);
        }

        [Test]
        public void GroupPredicate_Includes_All_Predicates()
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
        public void GroupPredicate_Includes_Sub_PredicateGroup()
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

        [Test]
        public void Predicates_Sort_Retuns_Setup_Sort()
        {
            var sort = Predicates.Sort<PredicateTestEntity>(f => f.Name, false);
            Assert.AreEqual("Name", sort.PropertyName);
            Assert.IsFalse(sort.Ascending);
        }

        [Test]
        public void Predicates_Property_Returns_Setup_Predicate()
        {
            var pred = Predicates.Property<PredicateTestEntity, PredicateTestEntity2>(f => f.Name, Operator.Eq, f => f.Value);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string result = pred.GetSql(parameters);
            Assert.AreEqual("([PredicateTestEntity].[Name] = [PredicateTestEntity2].[Value])", result);
            Assert.AreEqual(0, parameters.Count);
        }

        [Test]
        public void PropertyPredicate_Eq_Returns_Proper_Sql()
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
        public void PropertyPredicate_Not_Eq_Returns_Proper_Sql()
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
        public void PropertyPredicate_Gt_Returns_Proper_Sql()
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
        public void PropertyPredicate_Not_Gt_Returns_Proper_Sql()
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
        public void PropertyPredicate_Ge_Returns_Proper_Sql()
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
        public void PropertyPredicate_Not_Ge_Returns_Proper_Sql()
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
        public void PropertyPredicate_Lt_Returns_Proper_Sql()
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
        public void PropertyPredicate_Not_Lt_Returns_Proper_Sql()
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
        public void PropertyPredicate_Le_Returns_Proper_Sql()
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
        public void PropertyPredicate_Not_Le_Returns_Proper_Sql()
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

        [Test]
        public void ExistsPredicate_Returns_Proper_Sql()
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
            Assert.AreEqual("(EXISTS (SELECT 1 FROM [PredicateTestEntity2] WHERE ([PredicateTestEntity].[Id] = [PredicateTestEntity2].[Key])))", result);
            Assert.AreEqual(0, parameters.Count);
        }

        [Test]
        public void ExistsPredicate_Not_Returns_Proper_Sql()
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
            Assert.AreEqual("(NOT EXISTS (SELECT 1 FROM [PredicateTestEntity2] WHERE ([PredicateTestEntity].[Id] = [PredicateTestEntity2].[Key])))", result);
            Assert.AreEqual(0, parameters.Count);
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
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class PredicatesFixture : BaseFixture
    {
        public override void Setup()
        {
            base.Setup();
            DapperExtensions.Formatter = new TestFormatter();
        }

        [Test]
        public void Predicates_Field_Returns_Setup_Predicate()
        {
            var pred = Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Eq, "Lead");
            Dictionary<string, object> parameters = new Dictionary<string, object>
                                                        {
                                                            { "Field", "123" }
                                                        };
            string result = pred.GetSql(parameters);
            Assert.AreEqual("(PredicateTestEntity.Name = @Namep1)", result);
            Assert.AreEqual("Lead", parameters["@Namep1"]);
        }

        [Test]
        public void FieldPredicate_Eq_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id = @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Eq_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id <> @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Gt_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id > @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Gt_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id <= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Ge_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id >= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Ge_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id < @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Lt_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id < @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Lt_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id >= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Le_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id <= @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_Not_Le_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Id > @Idp0)", result);
            Assert.AreEqual(3, parameters["@Idp0"]);
        }

        [Test]
        public void FieldPredicate_With_Null_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Name IS NULL)", result);
            Assert.AreEqual(0, parameters.Count);
        }

        [Test]
        public void FieldPredicate_With_Null_ANd_Not_Returns_Property_Sql()
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
            Assert.AreEqual("(PredicateTestEntity.Name IS NOT NULL)", result);
            Assert.AreEqual(0, parameters.Count);
        }
        
        private class TestFormatter : IDapperFormatter
        {
            public string GetTableName(IClassMapper map)
            {
                return map.TableName;
            }

            public string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
            {
                return string.Format("{0}.{1}", map.TableName, property.Name);
            }

            public Guid GetNextGuid()
            {
                throw new NotImplementedException();
            }
        }

        private class PredicateTestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
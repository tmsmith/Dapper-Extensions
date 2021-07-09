using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using DapperExtensions.Test.Extensions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class SqlGeneratorFixture
    {
        public abstract class SqlGeneratorFixtureBase
        {
            protected Mock<IDapperExtensionsConfiguration> Configuration;

            protected Mock<SqlGeneratorImpl> Generator;
            protected Mock<ISqlDialect> Dialect;
            protected Mock<IClassMapper> ClassMap;
            protected Mock<IList<IProjection>> Projections;
            [SetUp]
            public void Setup()
            {
                Configuration = new Mock<IDapperExtensionsConfiguration>();
                Dialect = new Mock<ISqlDialect>();
                ClassMap = new Mock<IClassMapper>();
                Projections = new Mock<IList<IProjection>>();

                Dialect.SetupGet(c => c.ParameterPrefix).Returns('@');
                Configuration.SetupGet(c => c.Dialect).Returns(Dialect.Object).Verifiable();

                Generator = new Mock<SqlGeneratorImpl>(Configuration.Object)
                {
                    CallBase = true
                };
            }
        }

        [TestFixture]
        public class SelectMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void WithNullParameters_ThrowsException()
            {
                Sort sort = new Sort();
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.Select(ClassMap.Object, null, null, null, null));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
            }

            [Test]
            public void WithoutPredicateAndSort_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, null, null, parameters, null);
                StringAssert.AreEqualIgnoringCase("SELECT Columns FROM TableName", result);
                ClassMap.Verify();
                Generator.Verify();
            }

            [Test]
            public void WithoutPredicateAndSortWithProjection_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();

                Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();

                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, Projections.Object, null))
                    .Returns("Columns 1, Columns 2")
                    .Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, null, null, parameters, Projections.Object);
                Assert.AreEqual("SELECT Columns 1, Columns 2 FROM TableName", result.RemoveLineEndings());
                ClassMap.Verify();
                Dialect.Verify();
            }

            [Test]
            public void WithPredicate_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, predicate.Object, null, parameters, null);
                StringAssert.AreEqualIgnoringCase("SELECT Columns FROM TableName WHERE PredicateWhere", result);
                ClassMap.Verify();
                predicate.Verify();
                Generator.Verify();
                predicate.Verify();
            }

            [Test]
            public void WithSort_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                var sortField = new Mock<ISort>();
                sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
                sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
                var sort = new List<ISort>
                                       {
                                           sortField.Object
                                       };

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, null, sort, parameters, null);
                StringAssert.AreEqualIgnoringCase("SELECT Columns FROM TableName ORDER BY SortColumn ASC", result);
                ClassMap.Verify();
                sortField.Verify();
                Generator.Verify();
            }

            [Test]
            public void WithPredicateAndSort_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<ISort> sortField = new Mock<ISort>();
                sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
                sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
                List<ISort> sort = new List<ISort>
                                       {
                                           sortField.Object
                                       };

                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, predicate.Object, sort, parameters, null);
                StringAssert.AreEqualIgnoringCase("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", result);
                ClassMap.Verify();
                sortField.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        [TestFixture]
        public class SelectPagedMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void WithNoSort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectPaged(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>(), null));
                StringAssert.Contains("null or empty", ex.Message);
            }

            [Test]
            public void WithEmptySort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>(), null));
                StringAssert.Contains("null or empty", ex.Message);
                StringAssert.AreEqualIgnoringCase("Sort", ex.ParamName);
            }

            [Test]
            public void WithNullParameters_ThrowsException()
            {
                Sort sort = new Sort();
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null, null));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
            }

            [Test]
            public void WithSort_GeneratesSql()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
                property1.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
                var properties = new List<IMemberMap>
                                     {
                                         property1.Object,
                                         property2.Object
                                     };
                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<ISort> sortField = new Mock<ISort>();
                sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
                sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
                List<ISort> sort = new List<ISort>
                                       {
                                           sortField.Object
                                       };

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(It.IsAny<IClassMapper>(), It.IsAny<IList<IProjection>>(), It.IsAny<IList<IReferenceMap>>())).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName ORDER BY SortColumn ASC", 2, 10, parameters, null)).Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectPaged(ClassMap.Object, null, sort, 2, 10, parameters, null);
                StringAssert.AreEqualIgnoringCase("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                Generator.Verify();
                Dialect.Verify();
            }

            [Test]
            public void WithPredicateAndSort_GeneratesSql()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
                property1.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
                var properties = new List<IMemberMap>
                                     {
                                         property1.Object,
                                         property2.Object
                                     };
                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<ISort> sortField = new Mock<ISort>();
                sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
                sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
                List<ISort> sort = new List<ISort>
                                       {
                                           sortField.Object
                                       };

                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(It.IsAny<IClassMapper>(), It.IsAny<IList<IProjection>>(), It.IsAny<IList<IReferenceMap>>())).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false, true)).Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", 2, 10, parameters, null))
                    .Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectPaged(ClassMap.Object, predicate.Object, sort, 2, 10, parameters, null);
                StringAssert.AreEqualIgnoringCase("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        [TestFixture]
        public class SelectSetMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void WithNoSort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectSet(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>(), null));
                StringAssert.Contains("null or empty", ex.Message);
            }

            [Test]
            public void WithEmptySort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectSet(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>(), null));
                StringAssert.Contains("null or empty", ex.Message);
                StringAssert.AreEqualIgnoringCase("Sort", ex.ParamName);
            }

            [Test]
            public void WithNullParameters_ThrowsException()
            {
                Sort sort = new Sort();
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectSet(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null, null));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
            }

            [Test]
            public void WithSort_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<ISort> sortField = new Mock<ISort>();
                sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
                sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
                List<ISort> sort = new List<ISort>
                                       {
                                           sortField.Object
                                       };

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(It.IsAny<IClassMapper>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetSetSql("SELECT Columns FROM TableName ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectSet(ClassMap.Object, null, sort, 2, 10, parameters, null);
                StringAssert.AreEqualIgnoringCase("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                Generator.Verify();
                Dialect.Verify();
            }

            [Test]
            public void WithPredicateAndSort_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<ISort> sortField = new Mock<ISort>();
                sortField.SetupGet(s => s.PropertyName).Returns("SortProperty").Verifiable();
                sortField.SetupGet(s => s.Ascending).Returns(true).Verifiable();
                List<ISort> sort = new List<ISort>
                                       {
                                           sortField.Object
                                       };

                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object, null, null)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(It.IsAny<IClassMapper>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetSetSql("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectSet(ClassMap.Object, predicate.Object, sort, 2, 10, parameters, null);
                StringAssert.AreEqualIgnoringCase("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        [TestFixture]
        public class CountMethod : SqlGeneratorFixtureBase
        {
            private void Arrange()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                var properties = new List<IMemberMap>
                                     {
                                         property1.Object,
                                         property2.Object
                                     };

                var table = new Table
                {
                    Alias = "y_1",
                    EntityType = ClassMap.Object.EntityType,
                    Name = ClassMap.Object.TableName,
                    ReferenceName = "",
                    Identity = ClassMap.Object.Identity,
                    ParentIdentity = ClassMap.Object.Identity,
                    IsVirtual = false,
                    PropertyInfo = null,
                    ClassMapper = ClassMap.Object,
                    LastIdentity = Guid.Empty,
                    ParentEntityType = null
                };

                var column1 = new Column("Column1", property1.Object, null, table);
                var column2 = new Column("Column2", property2.Object, null, table);
                //Generator.Setup(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()))
                //    .Returns<IColumn, bool, bool>((column, alias, prefix) => column.Alias).Verifiable();
                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                Dialect.SetupGet(d => d.OpenQuote).Returns('!').Verifiable();
                Dialect.SetupGet(d => d.CloseQuote).Returns('^').Verifiable();
                Dialect.Setup(d => d.GetCountSql(It.IsAny<string>()))
                    .Returns<string>(sql => $"SELECT COUNT(*) AS {Dialect.Object.OpenQuote}Total{Dialect.Object.CloseQuote} FROM {sql}").Verifiable();

                Generator.Setup(g => g.GetTables(It.IsAny<IClassMapper>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IList<IReferenceMap>>()))
                    .Returns("TableName").Verifiable();
            }


            [Test]
            public void WithNullParameters_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Count(ClassMap.Object, null, null));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
            }

            [Test]
            public void WithoutPredicate_ThrowsException()
            {
                Arrange();

                var result = Generator.Object.Count(ClassMap.Object, null, new Dictionary<string, object>());
                StringAssert.AreEqualIgnoringCase("SELECT COUNT(*) AS !Total^ FROM TableName", result);
                Generator.Verify();
                Dialect.Verify();
            }

            [Test]
            public void WithPredicate_ThrowsException()
            {
                Arrange();

                var parameters = new Dictionary<string, object>();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters, false)).Returns("PredicateWhere").Verifiable();

                var result = Generator.Object.Count(ClassMap.Object, predicate.Object, parameters);
                StringAssert.AreEqualIgnoringCase("SELECT COUNT(*) AS !Total^ FROM TableName WHERE PredicateWhere", result);
                Generator.Verify();
                predicate.Verify();
                Dialect.Verify();
            }
        }

        [TestFixture]
        public class InsertMethod : SqlGeneratorFixtureBase
        {
            private void SetupGenerator()
            {
                Generator.Setup(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
            }

            [Test]
            public void WithNoMappedColumns_Throws_Exception()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                var ex = Assert.Throws<ArgumentException>(() => Generator.Object.Insert(ClassMap.Object));

                StringAssert.Contains("columns were mapped", ex.Message);
                ClassMap.Verify();
                property1.Verify();
                property2.Verify();
            }

            [Test]
            public void DoesNotGenerateIdentityColumns()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                SetupGenerator();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var result = Generator.Object.Insert(ClassMap.Object);
                StringAssert.AreEqualIgnoringCase("INSERT INTO TableName (Column) VALUES (@i_1)", result);

                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Once());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            }

            [Test]
            public void DoesNotGenerateIgnoredColumns()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.Ignored).Returns(true).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                SetupGenerator();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var result = Generator.Object.Insert(ClassMap.Object);
                StringAssert.AreEqualIgnoringCase("INSERT INTO TableName (Column) VALUES (@i_1)", result);

                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Once());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            }

            [Test]
            public void DoesNotGenerateReadonlyColumns()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                SetupGenerator();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var result = Generator.Object.Insert(ClassMap.Object);
                StringAssert.AreEqualIgnoringCase("INSERT INTO TableName (Column) VALUES (@i_1)", result);

                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Once());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            }

            [Test]
            public void DoesNotPrefixColumnListWithTableName()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.SetupGet(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.SetupGet(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.SetupGet(p => p.Name).Returns("Name").Verifiable();
                property2.SetupGet(p => p.ColumnName).Returns("Name").Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();
                ClassMap.SetupGet(c => c.TableName).Returns("TableName");

                Dialect.Setup(c => c.GetTableName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((a, b, c) => b);
                Dialect.Setup(d => d.GetColumnName(It.IsAny<string>(), It.IsAny<string>(), null)).Returns<string, string, string>((a, b, c) => a + (a == null ? String.Empty : ".") + b);

                SqlGeneratorImpl generator = new SqlGeneratorImpl(Configuration.Object);
                var sql = generator.Insert(ClassMap.Object);

                Assert.AreEqual("INSERT INTO TableName (Name) VALUES (@i_1)", sql);
            }
        }

        [TestFixture]
        public class UpdateMethod : SqlGeneratorFixtureBase
        {
            private void SetupGenerator()
            {
                Generator.Setup(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
            }

            [Test]
            public void WithNullPredicate_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Update(ClassMap.Object, null, new Dictionary<string, object>(), false, null));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Predicate", ex.ParamName);
            }

            [Test]
            public void WithNullParameters_ThrowsException()
            {
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Update(ClassMap.Object, predicate.Object, null, false, null));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
            }

            [Test]
            public void WithNoMappedColumns_Throws_Exception()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                var ex = Assert.Throws<ArgumentException>(() => Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null));

                StringAssert.Contains("columns were mapped", ex.Message);
                ClassMap.Verify();
                property1.Verify();
                property2.Verify();
            }

            [Test]
            public void DoesNotGenerateIdentityColumns()
            {
                var property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                var property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                var properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                SetupGenerator();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var predicate = new Mock<IPredicate>();
                var parameters = new Dictionary<string, object>();
                predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()))
                    .Returns("Predicate").Verifiable();

                var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null);

                StringAssert.AreEqualIgnoringCase("UPDATE TableName SET Column = @u_1 WHERE Predicate", result);

                predicate.Verify();
                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Once());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            }

            [Test]
            public void DoesNotGenerateIgnoredColumns()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.Ignored).Returns(true).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                SetupGenerator();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                Mock<IPredicate> predicate = new Mock<IPredicate>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()))
                    .Returns("Predicate").Verifiable();

                var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null);

                StringAssert.AreEqualIgnoringCase("UPDATE TableName SET Column = @u_1 WHERE Predicate", result);

                predicate.Verify();
                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Once());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            }

            [Test]
            public void DoesNotGenerateReadonlyColumns()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IMemberMap> properties = new List<IMemberMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                SetupGenerator();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var parameters = new Dictionary<string, object>();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()))
                    .Returns("Predicate").Verifiable();

                var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters, false, null);

                StringAssert.AreEqualIgnoringCase("UPDATE TableName SET Column = @u_1 WHERE Predicate", result);

                predicate.Verify();
                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Once());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            }
        }

        [TestFixture]
        public class DeleteWithPredicateMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void WithNullPredicate_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Delete(ClassMap.Object, null, new Dictionary<string, object>()));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Predicate", ex.ParamName);
            }

            [Test]
            public void WithNullParameters_ThrowsException()
            {
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Delete(ClassMap.Object, predicate.Object, null));
                StringAssert.Contains("cannot be null", ex.Message);
                StringAssert.AreEqualIgnoringCase("Parameters", ex.ParamName);
            }

            [Test]
            public void GeneratesSql()
            {
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(It.IsAny<ISqlGenerator>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>())).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();

                var result = Generator.Object.Delete(ClassMap.Object, predicate.Object, new Dictionary<string, object>());
                StringAssert.AreEqualIgnoringCase("DELETE FROM TableName WHERE PredicateWhere", result);
                ClassMap.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        [TestFixture]
        public class IdentitySqlMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void CallsDialect()
            {
                Dialect.Setup(d => d.GetIdentitySql("TableName")).Returns("IdentitySql").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
                var result = Generator.Object.IdentitySql(ClassMap.Object);
                StringAssert.AreEqualIgnoringCase("IdentitySql", result);
                Generator.Verify();
                Dialect.Verify();
            }
        }

        [TestFixture]
        public class GetTableNameMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void CallsDialect()
            {
                ClassMap.SetupGet(c => c.SchemaName).Returns("SchemaName").Verifiable();
                ClassMap.SetupGet(c => c.TableName).Returns("TableName").Verifiable();
                Dialect.Setup(d => d.GetTableName("SchemaName", "TableName", null)).Returns("FullTableName").Verifiable();
                var result = Generator.Object.GetTableName(ClassMap.Object);
                StringAssert.AreEqualIgnoringCase("FullTableName", result);
                Dialect.Verify();
                ClassMap.Verify();
            }
        }

        [TestFixture]
        public class GetColumnNameMethod : SqlGeneratorFixtureBase
        {
            private void Arrange(out Mock<IMemberMap> property)
            {
                property = new Mock<IMemberMap>();
                property.SetupGet(p => p.ColumnName).Returns("Column").Verifiable();
                property.SetupGet(p => p.Name).Returns("Name").Verifiable();

                Generator.Setup(g => g.GetTableName(ClassMap.Object, It.IsAny<bool>())).Returns("TableName").Verifiable();
                Dialect.Setup(d => d.GetColumnName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("FullColumnName").Verifiable();
            }

            [Test]
            public void DoesNotIncludeAliasWhenParameterIsFalse()
            {
                Arrange(out var property);

                var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, false);
                StringAssert.AreEqualIgnoringCase("FullColumnName", result);
                property.Verify();
                Generator.Verify();
            }

            [Test]
            public void DoesNotIncludeAliasWhenColumnAndNameAreSame()
            {
                Arrange(out var property);

                var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, true, false);
                StringAssert.AreEqualIgnoringCase("FullColumnName", result);
                property.Verify();
                Generator.Verify();
            }

            [Test]
            public void IncludesAliasWhenColumnAndNameAreDifferent()
            {
                Arrange(out var property);

                var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, true, false);
                StringAssert.AreEqualIgnoringCase("FullColumnName", result);
                property.Verify();
                Generator.Verify();
            }
        }

        [TestFixture]
        public class GetColumnNameUsingStirngMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void ThrowsExceptionWhenDoesNotFindProperty()
            {
                ClassMap.SetupGet(c => c.Properties).Returns(new List<IMemberMap>()).Verifiable();
                var ex = Assert.Throws<ArgumentException>(() => Generator.Object.GetColumnName(ClassMap.Object, "property", true));
                StringAssert.Contains("Could not find 'property'", ex.Message);
                ClassMap.Verify();
            }

            [Test]
            public void CallsGetColumnNameWithProperty()
            {
                Mock<IMemberMap> property = new Mock<IMemberMap>();
                property.Setup(p => p.Name).Returns("property").Verifiable();
                ClassMap.SetupGet(c => c.Properties).Returns(new List<IMemberMap> { property.Object }).Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property.Object, true, false, true)).Returns("ColumnName").Verifiable();
                var result = Generator.Object.GetColumnName(ClassMap.Object, "property", true, true);
                StringAssert.AreEqualIgnoringCase("ColumnName", result);
                ClassMap.Verify();
                property.Verify();
                Generator.Verify();
            }
        }

        [TestFixture]
        public class SupportsMultipleStatementsMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void CallsDialect()
            {
                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(true).Verifiable();
                var result = Generator.Object.SupportsMultipleStatements();
                Assert.IsTrue(result);
                Dialect.Verify();
            }
        }

        [TestFixture]
        public class BuildSelectColumnsMethod : SqlGeneratorFixtureBase
        {
            [Test]
            public void GeneratesSql()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                var properties = new List<IMemberMap>
                                     {
                                         property1.Object,
                                         property2.Object
                                     };

                var table = new Table
                {
                    Alias = "y_1",
                    EntityType = ClassMap.Object.EntityType,
                    Name = ClassMap.Object.TableName,
                    ReferenceName = "",
                    Identity = ClassMap.Object.Identity,
                    ParentIdentity = ClassMap.Object.Identity,
                    IsVirtual = false,
                    PropertyInfo = null,
                    ClassMapper = ClassMap.Object
                };

                var column1 = new Column("Column1", property1.Object, null, table);
                var column2 = new Column("Column2", property2.Object, null, table);
                Generator.Setup(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns<IColumn, bool, bool>((column, alias_, prefix) => column.Alias).Verifiable();
                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                var result = Generator.Object.BuildSelectColumns(ClassMap.Object, null);
                StringAssert.AreEqualIgnoringCase($"{column1.Alias}, {column2.Alias}", result);
                ClassMap.Verify();
                Generator.Verify();
            }

            [Test]
            public void DoesNotIncludeIgnoredColumns()
            {
                Mock<IMemberMap> property1 = new Mock<IMemberMap>();
                property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
                property1.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
                Mock<IMemberMap> property2 = new Mock<IMemberMap>();
                property2.SetupGet(p => p.ClassMapper).Returns(ClassMap.Object).Verifiable();
                var properties = new List<IMemberMap>
                                     {
                                         property1.Object,
                                         property2.Object
                                     };

                const string columnName = "Column2";
                Generator.Setup(g => g.GetColumnName(It.IsAny<IColumn>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(columnName).Verifiable();
                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                var result = Generator.Object.BuildSelectColumns(ClassMap.Object, null);
                StringAssert.AreEqualIgnoringCase(columnName, result);
                ClassMap.Verify();
                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, true, false, true), Times.Never());
                property1.Verify();
            }
        }
    }
}

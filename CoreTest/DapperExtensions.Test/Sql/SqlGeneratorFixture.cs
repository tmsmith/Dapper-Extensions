using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Moq;
using Xunit;

namespace DapperExtensions.Test.Sql
{
    
    public class SqlGeneratorFixture
    {
        public abstract class SqlGeneratorFixtureBase
        {
            protected Mock<IDapperExtensionsConfiguration> Configuration;

            protected Mock<SqlGeneratorImpl> Generator;
            protected Mock<ISqlDialect> Dialect;
            protected Mock<IClassMapper> ClassMap;

            public SqlGeneratorFixtureBase()
            {
                Configuration = new Mock<IDapperExtensionsConfiguration>();
                Dialect = new Mock<ISqlDialect>();
                ClassMap = new Mock<IClassMapper>();

                Dialect.SetupGet(c => c.ParameterPrefix).Returns('@');
                Configuration.SetupGet(c => c.Dialect).Returns(Dialect.Object).Verifiable();

                Generator = new Mock<SqlGeneratorImpl>(Configuration.Object);
                Generator.CallBase = true;
            }
        }

        
        public class SelectMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void WithNullParameters_ThrowsException()
            {
                Sort sort = new Sort();
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.Select(ClassMap.Object, null, null, null));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Parameters", ex.ParamName);
            }

            [Fact]
            public void WithoutPredicateAndSort_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, null, null, parameters);
                Assert.Equal("SELECT Columns FROM TableName", result);
                ClassMap.Verify();
                Generator.Verify();
            }

            [Fact]
            public void WithPredicate_GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, predicate.Object, null, parameters);
                Assert.Equal("SELECT Columns FROM TableName WHERE PredicateWhere", result);
                ClassMap.Verify();
                predicate.Verify();
                Generator.Verify();
                predicate.Verify();
            }

            [Fact]
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

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false)).Returns("SortColumn").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, null, sort, parameters);
                Assert.Equal("SELECT Columns FROM TableName ORDER BY SortColumn ASC", result);
                ClassMap.Verify();
                sortField.Verify();
                Generator.Verify();
            }

            [Fact]
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
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false)).Returns("SortColumn").Verifiable();

                var result = Generator.Object.Select(ClassMap.Object, predicate.Object, sort, parameters);
                Assert.Equal("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", result);
                ClassMap.Verify();
                sortField.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        
        public class SelectPagedMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void WithNoSort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectPaged(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>()));
                Assert.True(ex.Message.Contains("null or empty"));
            }

            [Fact]
            public void WithEmptySort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>()));
                Assert.True(ex.Message.Contains("null or empty"));
                Assert.Equal("Sort", ex.ParamName);
            }

            [Fact]
            public void WithNullParameters_ThrowsException()
            {
                Sort sort = new Sort();
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectPaged(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Parameters", ex.ParamName);
            }

            [Fact]
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

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false)).Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectPaged(ClassMap.Object, null, sort, 2, 10, parameters);
                Assert.Equal("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                Generator.Verify();
                Dialect.Verify();
            }

            [Fact]
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
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false)).Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetPagingSql("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectPaged(ClassMap.Object, predicate.Object, sort, 2, 10, parameters);
                Assert.Equal("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        
        public class SelectSetMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void WithNoSort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectSet(ClassMap.Object, null, null, 0, 1, new Dictionary<string, object>()));
                Assert.True(ex.Message.Contains("null or empty"));
            }

            [Fact]
            public void WithEmptySort_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectSet(ClassMap.Object, null, new List<ISort>(), 0, 1, new Dictionary<string, object>()));
                Assert.True(ex.Message.Contains("null or empty"));
                Assert.Equal("Sort", ex.ParamName);
            }

            [Fact]
            public void WithNullParameters_ThrowsException()
            {
                Sort sort = new Sort();
                var ex = Assert.Throws<ArgumentNullException>(
                    () => Generator.Object.SelectSet(ClassMap.Object, null, new List<ISort> { sort }, 0, 1, null));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Parameters", ex.ParamName);
            }

            [Fact]
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

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false)).Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetSetSql("SELECT Columns FROM TableName ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectSet(ClassMap.Object, null, sort, 2, 10, parameters);
                Assert.Equal("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                Generator.Verify();
                Dialect.Verify();
            }

            [Fact]
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
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Generator.Setup(g => g.BuildSelectColumns(ClassMap.Object)).Returns("Columns").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, "SortProperty", false)).Returns("SortColumn").Verifiable();

                Dialect.Setup(d => d.GetSetSql("SELECT Columns FROM TableName WHERE PredicateWhere ORDER BY SortColumn ASC", 2, 10, parameters)).Returns("PagedSQL").Verifiable();

                var result = Generator.Object.SelectSet(ClassMap.Object, predicate.Object, sort, 2, 10, parameters);
                Assert.Equal("PagedSQL", result);
                ClassMap.Verify();
                sortField.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        
        public class CountMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void WithNullParameters_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Count(ClassMap.Object, null, null));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Parameters", ex.ParamName);
            }

            [Fact]
            public void WithoutPredicate_ThrowsException()
            {
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Dialect.SetupGet(d => d.OpenQuote).Returns('!').Verifiable();
                Dialect.SetupGet(d => d.CloseQuote).Returns('^').Verifiable();

                var result = Generator.Object.Count(ClassMap.Object, null, new Dictionary<string, object>());
                Assert.Equal("SELECT COUNT(*) AS !Total^ FROM TableName", result);
                Generator.Verify();
                Dialect.Verify();
            }

            [Fact]
            public void WithPredicate_ThrowsException()
            {
                var parameters = new Dictionary<string, object>();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("PredicateWhere").Verifiable();
                Dialect.SetupGet(d => d.OpenQuote).Returns('!').Verifiable();
                Dialect.SetupGet(d => d.CloseQuote).Returns('^').Verifiable();

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                var result = Generator.Object.Count(ClassMap.Object, predicate.Object, parameters);
                Assert.Equal("SELECT COUNT(*) AS !Total^ FROM TableName WHERE PredicateWhere", result);
                Generator.Verify();
                predicate.Verify();
                Dialect.Verify();
            }
        }

        
        public class InsertMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void WithNoMappedColumns_Throws_Exception()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };


                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                var ex = Assert.Throws<ArgumentException>(() => Generator.Object.Insert(ClassMap.Object));

                Assert.True(ex.Message.Contains("columns were mapped"));
                ClassMap.Verify();
                property1.Verify();
                property2.Verify();
            }

            [Fact]
            public void DoesNotGenerateIdentityColumns()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, false)).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var result = Generator.Object.Insert(ClassMap.Object);
                Assert.Equal("INSERT INTO TableName (Column) VALUES (@Name)", result);

                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Never());
                property2.Verify();
                
                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, false), Times.Never());
            }

            [Fact]
            public void DoesNotGenerateIgnoredColumns()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.Ignored).Returns(true).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, false)).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var result = Generator.Object.Insert(ClassMap.Object);
                Assert.Equal("INSERT INTO TableName (Column) VALUES (@Name)", result);

                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Never());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, false), Times.Never());
            }

            [Fact]
            public void DoesNotGenerateReadonlyColumns()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, false)).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                var result = Generator.Object.Insert(ClassMap.Object);
                Assert.Equal("INSERT INTO TableName (Column) VALUES (@Name)", result);

                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Never());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, false), Times.Never());
            }
        }

        
        public class UpdateMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void WithNullPredicate_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Update(ClassMap.Object, null, new Dictionary<string, object>()));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Predicate", ex.ParamName);
            }

            [Fact]
            public void WithNullParameters_ThrowsException()
            {
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Update(ClassMap.Object, predicate.Object, null));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Parameters", ex.ParamName);
            }

            [Fact]
            public void WithNoMappedColumns_Throws_Exception()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };


                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                var ex = Assert.Throws<ArgumentException>(() => Generator.Object.Update(ClassMap.Object, predicate.Object, parameters));

                Assert.True(ex.Message.Contains("columns were mapped"));
                ClassMap.Verify();
                property1.Verify();
                property2.Verify();
            }
            
            [Fact]
            public void DoesNotGenerateIdentityColumns()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.KeyType).Returns(KeyType.Identity).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, false)).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                Mock<IPredicate> predicate = new Mock<IPredicate>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("Predicate").Verifiable();
                
                var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters);

                Assert.Equal("UPDATE TableName SET Column = @Name WHERE Predicate", result);

                predicate.Verify();
                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Never());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, false), Times.Never());
            }

            [Fact]
            public void DoesNotGenerateIgnoredColumns()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.Ignored).Returns(true).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, false)).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                Mock<IPredicate> predicate = new Mock<IPredicate>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("Predicate").Verifiable();

                var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters);

                Assert.Equal("UPDATE TableName SET Column = @Name WHERE Predicate", result);

                predicate.Verify();
                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Never());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, false), Times.Never());
            }

            [Fact]
            public void DoesNotGenerateReadonlyColumns()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.Setup(p => p.IsReadOnly).Returns(true).Verifiable();

                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                property2.Setup(p => p.KeyType).Returns(KeyType.NotAKey).Verifiable();
                property2.Setup(p => p.Name).Returns("Name").Verifiable();

                List<IPropertyMap> properties = new List<IPropertyMap>
                                                    {
                                                        property1.Object,
                                                        property2.Object
                                                    };

                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, false)).Returns("Column").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(false).Verifiable();

                Mock<IPredicate> predicate = new Mock<IPredicate>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("Predicate").Verifiable();

                var result = Generator.Object.Update(ClassMap.Object, predicate.Object, parameters);

                Assert.Equal("UPDATE TableName SET Column = @Name WHERE Predicate", result);

                predicate.Verify();
                ClassMap.Verify();
                property1.Verify();
                property1.VerifyGet(p => p.Name, Times.Never());
                property2.Verify();

                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, false), Times.Never());
            }
        }

        
        public class DeleteWithPredicateMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void WithNullPredicate_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Delete(ClassMap.Object, null, new Dictionary<string, object>()));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Predicate", ex.ParamName);
            }

            [Fact]
            public void WithNullParameters_ThrowsException()
            {
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                var ex = Assert.Throws<ArgumentNullException>(() => Generator.Object.Delete(ClassMap.Object, predicate.Object, null));
                Assert.True(ex.Message.Contains("cannot be null"));
                Assert.Equal("Parameters", ex.ParamName);
            }

            [Fact]
            public void GeneratesSql()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                Mock<IPredicate> predicate = new Mock<IPredicate>();
                predicate.Setup(p => p.GetSql(Generator.Object, parameters)).Returns("PredicateWhere");

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();

                var result = Generator.Object.Delete(ClassMap.Object, predicate.Object, parameters);
                Assert.Equal("DELETE FROM TableName WHERE PredicateWhere", result);
                ClassMap.Verify();
                predicate.Verify();
                Generator.Verify();
            }
        }

        
        public class IdentitySqlMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void CallsDialect()
            {
                Dialect.Setup(d => d.GetIdentitySql("TableName")).Returns("IdentitySql").Verifiable();
                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                var result = Generator.Object.IdentitySql(ClassMap.Object);
                Assert.Equal("IdentitySql", result);
                Generator.Verify();
                Dialect.Verify();
            }
        }

        
        public class GetTableNameMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void CallsDialect()
            {
                ClassMap.SetupGet(c => c.SchemaName).Returns("SchemaName").Verifiable();
                ClassMap.SetupGet(c => c.TableName).Returns("TableName").Verifiable();
                Dialect.Setup(d => d.GetTableName("SchemaName", "TableName", null)).Returns("FullTableName").Verifiable();
                var result = Generator.Object.GetTableName(ClassMap.Object);
                Assert.Equal("FullTableName", result);
                Dialect.Verify();
                ClassMap.Verify();
            }
        }

        
        public class GetColumnNameMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void DoesNotIncludeAliasWhenParameterIsFalse()
            {
                Mock<IPropertyMap> property = new Mock<IPropertyMap>();
                property.SetupGet(p => p.ColumnName).Returns("Column").Verifiable();
                property.SetupGet(p => p.Name).Returns("Name").Verifiable();

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Dialect.Setup(d => d.GetColumnName("TableName", "Column", null)).Returns("FullColumnName").Verifiable();
                var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, false);
                Assert.Equal("FullColumnName", result);
                property.Verify();
                Generator.Verify();
            }

            [Fact]
            public void DoesNotIncludeAliasWhenColumnAndNameAreSame()
            {
                Mock<IPropertyMap> property = new Mock<IPropertyMap>();
                property.SetupGet(p => p.ColumnName).Returns("Column").Verifiable();
                property.SetupGet(p => p.Name).Returns("Column").Verifiable();

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Dialect.Setup(d => d.GetColumnName("TableName", "Column", null)).Returns("FullColumnName").Verifiable();
                var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, true);
                Assert.Equal("FullColumnName", result);
                property.Verify();
                Generator.Verify();
            }

            [Fact]
            public void IncludesAliasWhenColumnAndNameAreDifferent()
            {
                Mock<IPropertyMap> property = new Mock<IPropertyMap>();
                property.SetupGet(p => p.ColumnName).Returns("Column").Verifiable();
                property.SetupGet(p => p.Name).Returns("Name").Verifiable();

                Generator.Setup(g => g.GetTableName(ClassMap.Object)).Returns("TableName").Verifiable();
                Dialect.Setup(d => d.GetColumnName("TableName", "Column", "Name")).Returns("FullColumnName").Verifiable();
                var result = Generator.Object.GetColumnName(ClassMap.Object, property.Object, true);
                Assert.Equal("FullColumnName", result);
                property.Verify();
                Generator.Verify();
            }
        }

        
        public class GetColumnNameUsingStirngMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void ThrowsExceptionWhenDoesNotFindProperty()
            {
                ClassMap.SetupGet(c => c.Properties).Returns(new List<IPropertyMap>()).Verifiable();
                var ex = Assert.Throws<ArgumentException>(() => Generator.Object.GetColumnName(ClassMap.Object, "property", true));
                Assert.True(ex.Message.Contains("Could not find 'property'"));
                ClassMap.Verify();
            }

            [Fact]
            public void CallsGetColumnNameWithProperty()
            {
                Mock<IPropertyMap> property = new Mock<IPropertyMap>();
                property.Setup(p => p.Name).Returns("property").Verifiable();
                ClassMap.SetupGet(c => c.Properties).Returns(new List<IPropertyMap> { property.Object }).Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property.Object, true)).Returns("ColumnName").Verifiable();
                var result = Generator.Object.GetColumnName(ClassMap.Object, "property", true);
                Assert.Equal("ColumnName", result);
                ClassMap.Verify();
                property.Verify();
                Generator.Verify();
            }
        }

        
        public class SupportsMultipleStatementsMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void CallsDialect()
            {
                Dialect.SetupGet(d => d.SupportsMultipleStatements).Returns(true).Verifiable();
                var result = Generator.Object.SupportsMultipleStatements();
                Assert.True(result);
                Dialect.Verify();
            }
        }

        
        public class BuildSelectColumnsMethod : SqlGeneratorFixtureBase
        {
            [Fact]
            public void GeneratesSql()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                var properties = new List<IPropertyMap>
                                     {
                                         property1.Object,
                                         property2.Object
                                     };

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property1.Object, true)).Returns("Column1").Verifiable();
                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, true)).Returns("Column2").Verifiable();
                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                var result = Generator.Object.BuildSelectColumns(ClassMap.Object);
                Assert.Equal("Column1, Column2", result);
                ClassMap.Verify();
                Generator.Verify();
            }

            [Fact]
            public void DoesNotIncludeIgnoredColumns()
            {
                Mock<IPropertyMap> property1 = new Mock<IPropertyMap>();
                property1.SetupGet(p => p.Ignored).Returns(true).Verifiable();
                Mock<IPropertyMap> property2 = new Mock<IPropertyMap>();
                var properties = new List<IPropertyMap>
                                     {
                                         property1.Object,
                                         property2.Object
                                     };

                Generator.Setup(g => g.GetColumnName(ClassMap.Object, property2.Object, true)).Returns("Column2").Verifiable();
                ClassMap.SetupGet(c => c.Properties).Returns(properties).Verifiable();

                var result = Generator.Object.BuildSelectColumns(ClassMap.Object);
                Assert.Equal("Column2", result);
                ClassMap.Verify();
                Generator.Verify();
                Generator.Verify(g => g.GetColumnName(ClassMap.Object, property1.Object, true), Times.Never());
                property1.Verify();
            }
        }
    }
}

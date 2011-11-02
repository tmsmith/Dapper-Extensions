# Introduction

Dapper Extensions is a small library that complements [Dapper](https://github.com/SamSaffron/dapper-dot-net) by adding basic CRUD operations (Get, Insert, Update, Delete) for your POCOs. For more advanced querying scenarios, Dapper Extensions provides a predicate system. The goal of this library is to keep your POCOs pure by not requiring any attributes or base class inheritance.

Customized mappings are achieved through ClassMapper. 

**Important**: This library is a separate effort from Dapper.Contrib (a sub-system of the [Dapper](https://github.com/SamSaffron/dapper-dot-net) project).

Features
--------
* Zero configuration out of the box.
* Automatic mapping of POCOs for Get, Insert, Update, and Delete operations.
* Automatic support for Guid and Integer primary keys.
* Pure POCOs through use of ClassMapper.
* Customized mapping through the use of ClassMapper.
* Composite Primary Key support (coming soon).
* Singular and Pluralized table name support.
* Easy-to-use Predicate System for more advanced scenarios.
* GetList, Count methods for more advanced scenarios.
* Properly escapes table/column names in generated SQL (Ex: SELECT [FirstName] FROM [Users] WHERE [Users].[UserId] = @UserId)

Naming Conventions
------------------
* POCO names should match the table name in the database. Pluralized table names are supported through the PlurizedAutoClassMapper.
* POCO property names should match each column name in the table.
* By convention, the primary key should be named Id. Using another name is supported through custom mappings.

# Installation

**Using Nuget**

```
PM> Install-Package Dapper
PM> Install-Package DapperExtensions (Coming Soon)
```

**Manual Installation**

Include SqlMapper.cs in your project (from Dapper project)
Include DapperExtensions.cs in your project

# Examples
The following examples will use a Person POCO defined as:

```
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```


## Get Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = cn.Get<Person>(1);	
    cn.Close();
}
```

## Simple Insert Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = new Person { FirstName = "Foo", LastName = "Bar" };
    cn.Insert(person);
    // person.Id is populated after the insertion.
    cn.Close();
}
```

## Simple Update Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = _connection.Get<Person>(1);
    person.LastName = "Baz";
    cn.Update(person);
    cn.Close();
}
```


## Simple Delete Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = _connection.Get<Person>(1);
    cn.Delete(person);
    cn.Close();
}
```

## GetList Operation (with Predicates)

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
    IEnumerable<Person> list = cn.GetList<Person>(predicate);
    cn.Close();
}
```

## Count Operation (with Predicates)
```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
    int count = cn.Count<Person>(predicate);
    cn.Close();
}            
```

# License

Copyright 2011 Thad Smith, Page Brooks and contributors

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
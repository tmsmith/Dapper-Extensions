# Introduction

Dapper, Dapper Extensions and Linq. Dapper.Extensions.Linq builds on this providing advanced DB access through Linq queries. The fluid configuration makes setup simplistic and quick.

Release Notes
-------------

### 1.1.4
* Added new AutoClassMapper attributes TableName and PrefixForColumns.
* Isolated AutoMap to AutoClassMapper for clarity.
* Removed an unused reference to EntityFramework on Dapper.Linq.Extensions.SQLite


Features
--------
* Simplistic fluid configuration
* Automatic mapping of Entities 
* Sql generation with Linq
* Customisable entity mapping with [ClassMapper](https://github.com/tmsmith/Dapper-Extensions/wiki/AutoClassMapper).
* Entites can be manipulated with attributes
* Custom IoC containers
* Multiple connection providers
* Support for Sql Server, Postgre Sql, MySql, SqlCe and SQLite

Attributes 
---------
* DataContext - Which connection provider to use, defaults to **__Default**, you can additionally override this with fluid configuration.

Property Attributes
-------------------------
* Ignore - This property will not be mapped
* MapTo - Assign a property to a different database column

Naming Conventions
------------------
* Entity names should match the table name in the database. Pluralized table names are supported through the PlurizedAutoClassMapper.
* Entity properties should match the column name in the table.
* By convention, the primary key should be named Id. Using another name is supported through custom mappings.

# Installation

http://nuget.org/List/Packages/Dapper.Extensions.Linq

```
PM> Install-Package Dapper.Extensions.Linq
```

# Configuration
Note that dapper configuration requires a container in order to build. You can use the nuget package Dapper.Extensions.Linq.CastleWindsor or implement your own. The caslte provides you with dependency injection for the repositories.

Basic configuration and setup:

```c#
DapperConfiguration
    .Use()
    .UseClassMapper(typeof(AutoClassMapper<>))
    .UseContainer<Dapper.Extensions.Linq.CastleWindsor.WindsorContainer>(c => c.UseExisting(_container))
	.UseSqlDialect(new SqlServerDialect())
    .FromAssembly("Dapper.Entities")
    .Build();
```

# Examples
The following examples will use a Person Entity

```c#
public class Person : IEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool Active { get; set; }
    public DateTime DateCreated { get; set; }
}
```

## Get

```c#
_dapperRepository.Get(Id) //Using a get
_dapperRepository.Query(e => e.Id == Id) //Using linq by id
_dapperRepository.Query(e => e.FirstName == "Gary").SingleOrDefault() //Using linq by first name

```

## Insert

```c#
var person = new Person { FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.Now };
_dapperRepository.Insert(person);
```

## Update

```c#
Person person = _dapperRepository.Get(PersonId);
person.LastName = "Rwar";
_dapperRepository.Update(person);
```

## Delete

```c#
Person person = _dapperRepository.Get(PersonId);
_dapperRepository.Delete(person);
```

## Count

```c#
int numberOfPeople = _dapperRepository.Count();

or

int numberOfPeople = _dapperRepository.Count(e => e.FirstName == "Foo");
```

## Lists with Linq

```c#
List<Person> people = _dapperRepository
                       .Query(e => e.Active && e.DateCreated > DateTime.AddDays(-5))
                       .ToList();
```

# License

Copyright 2011 Thad Smith, Page Brooks, Ryan Watson and contributors

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
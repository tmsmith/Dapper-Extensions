Dapper Extensions
==========================
Dapper Extensions add the basic CRUD operations (Get, Insert, Update, Delete) for your POCOs. This library keeps your POCOs pure by not requiring attributes.

Custom mappings are achieved through ClassMapper<T>. 

Naming Conventions
-----------
* POCO names should match the table name in the database. Pluralized table names are supported through the PlurizedAutoClassMapper.
* POCO property names should match each column name in the table.
* By convention, the primary key should be named Id. Using another name is supported through custom mappings.

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
    Person person = _connection.Get<Person>(1);
	cn.Close();
}
```

## Simple Insert Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
	cn.Open();
    Person person = new Person { FirstName = "Foo", LastName = "Bar" };
    _connection.Insert(person);
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
    _connection.Update(person);
	cn.Close();
}
```


## Simple Delete Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
	cn.Open();
	Person person = _connection.Get<Person>(1);
    _connection.Delete(person);
	cn.Close();
}
```
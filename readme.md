
CRUD Extensions for Dapper
==========================

```
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```


# Get Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
	cn.Open();
    Person person = _connection.Get<Person>(1);
	cn.Close();
}
```

# Simple Insert Operation

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

# Simple Update Operation

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


# Simple Delete Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
	cn.Open();
	Person person = _connection.Get<Person>(1);
    _connection.Delete(person);
	cn.Close();
}
```
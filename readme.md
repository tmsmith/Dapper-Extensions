
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
    Person person = _connection.Object.Get<Person>(1);
}
```

# Insert Operation

```
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    Person p = new Person { FirstName = "Foo", LastName = "Bar" };
    _connection.Object.Insert(p);
    // Person.Id is populated after the insertion.
}
```

CRUD Extensions for Dapper
==========================

# Get Operation
'''
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
'''
...
'''
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    Person person = _connection.Object.Get<Person>(1);
}
'''
# JsonToSql

A simple class library, that provide Insert, Update and Delete functionalities to MS Sql simple JSON object that holds the table columns values.

## Example:
```cs
var connectioString = "Data Source=.;Initial Catalog=DB1;Integrated Security=True";

            var jsonToSql = new ObjectModel.JsonToSql(connectioString);

            jsonToSql.Save("Test", JObject.FromObject(new
            {
                id1 = 3,
                id2 = 2,
                email = "user0@hopmail.com",
                b = true,
                dt = DateTime.Now,
                d = 450.59,
                i = 160,
                m = 350.75
            }), CommandType.Insert)
            .Save("Test", JObject.FromObject(new
            {
                id1 = 3,
                id2 = 3,
                email = "user1@hopmail.com",
                b = false,
                dt = DateTime.Now.AddHours(1),
                d = 460.1,
                i = 260,
                m = 400.87
            }), CommandType.Insert)
            .Save("T1", JObject.FromObject(new
            {
                username = "user0",
                password = "password0"
            }), CommandType.Insert)
            .Save("T1", JObject.FromObject(new
            {
                id = 1,
                username = "user1",
                password = "password"
            }), CommandType.Update);

            jsonToSql.SaveChanges();
```

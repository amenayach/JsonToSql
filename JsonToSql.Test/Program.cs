using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonToSql.ObjectModel;
using Newtonsoft.Json.Linq;

namespace JsonToSql.Test
{
    class Program
    {
        static void Main(string[] args)
        {

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
            }), CommandType.Update)
            .Save("T1", JObject.FromObject(new
            {
                id = 3
            }), CommandType.Delete);

            jsonToSql.SaveChanges();

            //var command = new Command()
            //{
            //    Tablename = "Test",
            //    Type = CommandType.Insert,
            //    Json = JObject.FromObject(new
            //    {
            //        id1 = 2,
            //        id2 = 2,
            //        email = "abc@yopmail.com",
            //        b = false,
            //        dt = DateTime.Now,
            //        d = 3455.55,
            //        i = 6,
            //        m = 2344.3
            //    })
            //};

            //using (var connection = new SqlConnection(connectioString))
            //{

            //    command.Execute(connection);

            //}

        }
    }
}

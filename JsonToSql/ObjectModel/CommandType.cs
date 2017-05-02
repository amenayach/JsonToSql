using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonToSql.ObjectModel
{
    /// <summary>
    /// The types of SQL command insert, update and delete
    /// </summary>
    public enum CommandType
    {
        None,
        Insert,
        Update,
        Delete
    }
}

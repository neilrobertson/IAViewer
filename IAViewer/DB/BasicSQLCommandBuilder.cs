using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAViewer.DB
{
    public class BasicSQLCommandBuilder
    {

        public BasicSQLCommandBuilder() { }

        public static String BasicSQLInsert(String table, Dictionary<String,String> param)
        {
            StringBuilder insertBuilder = new StringBuilder();
            insertBuilder.Append("INSERT INTO ");
            insertBuilder.Append(table);
            insertBuilder.Append(" (");
            foreach (var x in param.Keys)
            {

            }


            return insertBuilder.ToString();
        }
    }
}

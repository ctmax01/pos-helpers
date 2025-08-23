using System;
using System.Collections.Generic;
using System.Data;

namespace Pos.Helpers
{
    public static class DataHelper
    {

        public static List<Dictionary<string, object>> ToDictionaryList(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    object value = dr[col];
                    if (value != DBNull.Value && value is DateTime)
                        row[col.ColumnName] = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        row[col.ColumnName] = value;
                }

                rows.Add(row);
            }

            return rows;
        }
    }
}

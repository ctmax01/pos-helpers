using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Text;

namespace Pos.Helpers
{
    public static class Helpers
    {
        public static string GetBranchName(HttpRequest request)
        {


            if (request == null)
                throw new ArgumentNullException("request", "Request IS NULL");

            string path = request.Path;
            var segments = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
                throw new Exception("Не удалось определить branchName");

            return segments[0];
        }



        // Convert DataTable to List
        public static List<Dictionary<string, object>> ToDictionaryList(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    row[col.ColumnName] = dr[col];
                }

                rows.Add(row);
            }

            return rows;
        }


        public static string FixEncoding(string input)
        {

            if (string.IsNullOrEmpty(input))
                return input;

            bool broken = false;

            foreach (char c in input)
            {

                if ((c >= 0x20 && c <= 0x7E) || (c >= 0x0400 && c <= 0x04FF))
                    continue;
                broken = true;
                break;
            }

            if (!broken)
                return input;
            try
            {
                byte[] bytes = Encoding.GetEncoding(1251).GetBytes(input);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return input;
            }


        }











    }






}
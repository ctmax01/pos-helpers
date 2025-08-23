using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Pos.Helpers
{
    public static class DbHelper
    {
        private static readonly Lazy<string> _connectionString = new Lazy<string>(() =>
        {
            string configPath = HttpContext.Current.Server.MapPath("~/pos/config.json");
            string json = File.ReadAllText(configPath);
            var serializer = new JavaScriptSerializer();
            var dict = serializer.Deserialize<Dictionary<string, string>>(json);
            return dict["ConnectionString"];
        });

        public static string GetBranchConnectionString(string branchName)
        {
            string sql = "SELECT conString FROM PosConnections WHERE branchName = @branch";
            return ExecuteScalar(sql, new Dictionary<string, object>
            {
                { "@branch", branchName }
            })?.ToString();
        }

        private static string GetConnectionString()
        {
            return _connectionString.Value;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        public static int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection cn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                AddParameters(cmd, parameters);
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection cn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                
                AddParameters(cmd, parameters);
                cn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection cn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                AddParameters(cmd, parameters);
                cn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        private static void AddParameters(SqlCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters == null) return;

            foreach (var p in parameters)
            {
                var value = p.Value ?? DBNull.Value;
                var param = cmd.Parameters.Add(p.Key, GetDbType(value));
                param.Value = value;
            }
        }

        private static SqlDbType GetDbType(object value)
        {
            if (value == null || value is DBNull) return SqlDbType.NVarChar;

            Type type = value.GetType();

            if (type == typeof(int)) return SqlDbType.Int;
            if (type == typeof(long)) return SqlDbType.BigInt;
            if (type == typeof(DateTime)) return SqlDbType.DateTime;
            if (type == typeof(bool)) return SqlDbType.Bit;
            if (type == typeof(decimal)) return SqlDbType.Decimal;
            if (type == typeof(double)) return SqlDbType.Float;
            if (type == typeof(Guid)) return SqlDbType.UniqueIdentifier;
            if (type == typeof(byte[])) return SqlDbType.VarBinary;

            return SqlDbType.NVarChar;
        }
    }
}

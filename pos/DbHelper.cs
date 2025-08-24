using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Pos.Helpers
{
    public static class DB
    {
        public static string GetDefaultConnectionString()
        {
            string configPath = HttpContext.Current.Server.MapPath("~/pos/config.json");
            string json = File.ReadAllText(configPath);
            var serializer = new JavaScriptSerializer();
            var dict = serializer.Deserialize<Dictionary<string, string>>(json);

            if (!dict.ContainsKey("ConnectionString") || string.IsNullOrEmpty(dict["ConnectionString"]))
                throw new Exception("Default connection string не найдена в config.json");

            return dict["ConnectionString"];
        }



        public static string GetConString(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            string path = request.Path;
            var segments = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                throw new Exception("Не удалось определить branchName");
            string branchName = segments[0];


            string sql = "SELECT conString FROM PosConnections WHERE branchName = @branch";
            string defaultConnection = GetDefaultConnectionString();
            object result = ExecuteScalar(defaultConnection, sql, new Dictionary<string, object>
            {
                { "@branch", branchName }
            });


            if (segments.Length == 0)
                throw new ClientException("Не удалось определить branchName", 400);
            if (result == null || string.IsNullOrEmpty(result.ToString()))
                throw new ClientException("Строка подключения для branch " + branchName + " не найдена", 400);

            string connectionString = result.ToString();

            if (!TestConnection(connectionString))
            {
                throw new ClientException(
                    "Не удалось подключиться к базе для branch " + branchName, errors: connectionString, code: 401
                );
            }

            return connectionString;
        }


        private static bool TestConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            try
            {
                using (var cn = new SqlConnection(connectionString))
                {
                    cn.Open();
                    return cn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }




        public static int ExecuteNonQuery(string connectionString, string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    AddParameters(cmd, parameters);
                    cn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                LogError(ex, sql, parameters);
                throw;
            }
        }

        public static object ExecuteScalar(string connectionString, string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    AddParameters(cmd, parameters);
                    cn.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                LogError(ex, sql, parameters);
                throw;
            }
        }

        public static DataTable ExecuteQuery(string connectionString, string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
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
            catch (SqlException ex)
            {
                LogError(ex, sql, parameters);
                throw;
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

        private static void LogError(Exception ex, string sql, Dictionary<string, object> parameters)
        {
            string logPath = HttpContext.Current.Server.MapPath("~/pos/error.log");
            using (StreamWriter sw = new StreamWriter(logPath, true))
            {
                sw.WriteLine("[" + DateTime.Now.ToString() + "] Ошибка SQL:");
                sw.WriteLine(sql);

                if (parameters != null)
                {
                    foreach (var p in parameters)
                        sw.WriteLine(p.Key + " = " + (p.Value ?? "null"));
                }

                sw.WriteLine(ex.ToString());
                sw.WriteLine("---------------------------------------------------");
            }
        }
    }

    public class Params : Dictionary<string, object>
    {
        public Params() : base(System.StringComparer.OrdinalIgnoreCase) { }

        public static Params Create(params object[] args)
        {
            if (args.Length % 2 != 0)
                throw new ArgumentException("Нечетное количество аргументов. Должно быть парами: ключ, значение.");

            var dict = new Params();
            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i] == null)
                    throw new ArgumentNullException(
                        "Параметр с индексом {" + i + "} (ключ) равен null"
                    );

                string key = args[i].ToString();
                dict[key] = args[i + 1];
            }
            return dict;
        }
    }


}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace LoginRegisterV2
{
    /// <summary>Small database helpers shared by every page.</summary>
    public static class Db
    {
        public static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["LoginRegisterDB"].ConnectionString; }
        }

        /// <summary>Always passes the value as an object so 0 is never mistaken for an enum.</summary>
        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(table);
                }
            }
            return table;
        }

        public static int Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>Writes the technical details to App_Data/error.log so pages can show a friendly message instead.</summary>
        public static void LogError(Exception ex)
        {
            try
            {
                string path = HostingEnvironment.MapPath("~/App_Data/error.log");
                if (path == null) return;
                File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + ex.ToString() + Environment.NewLine + Environment.NewLine);
            }
            catch
            {
                // never let logging break the page
            }
        }
    }

    /// <summary>
    /// Brings the existing database up to date (adds only what is missing) by running
    /// App_Data/UpgradeSchema.sql. Runs once when the web app starts.
    /// </summary>
    public static class SchemaMigrator
    {
        private static readonly object Sync = new object();
        private static bool _ready;

        public static string LastError { get; private set; }

        public static bool EnsureSchema()
        {
            if (_ready)
            {
                return true;
            }

            lock (Sync)
            {
                if (_ready)
                {
                    return true;
                }

                try
                {
                    string path = HostingEnvironment.MapPath("~/App_Data/UpgradeSchema.sql");
                    if (path == null || !File.Exists(path))
                    {
                        throw new FileNotFoundException("App_Data/UpgradeSchema.sql was not found.");
                    }

                    string script = File.ReadAllText(path, Encoding.UTF8);

                    using (SqlConnection conn = new SqlConnection(Db.ConnStr))
                    {
                        conn.Open();

                        foreach (string batch in SplitBatches(script))
                        {
                            bool optional = batch.TrimStart().StartsWith("-- OPTIONAL", StringComparison.OrdinalIgnoreCase);

                            try
                            {
                                using (SqlCommand cmd = new SqlCommand(batch, conn))
                                {
                                    cmd.CommandTimeout = 120;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (SqlException)
                            {
                                if (!optional)
                                {
                                    throw;
                                }
                            }
                        }
                    }

                    _ready = true;
                    LastError = null;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    Db.LogError(ex);
                    _ready = false;
                }

                return _ready;
            }
        }

        private static List<string> SplitBatches(string script)
        {
            List<string> batches = new List<string>();
            string[] parts = Regex.Split(script, @"^[ \t]*GO[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (string part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    batches.Add(part);
                }
            }

            return batches;
        }
    }
}

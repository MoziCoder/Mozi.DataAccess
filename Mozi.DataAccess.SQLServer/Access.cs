using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Mozi.DataAccess.SQLServer
{
    /// <summary>
    /// SQLServer数据访问层
    /// </summary>
    public sealed class Access:AbsDataAccess
    {
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="connString">数据库链接字符串</param>
        public Access(string connString) : base(connString)
        {

        }

        public Access(Config.ServerConfig config):base(config)
        {

        }

        public override string GenerateConnectionString()
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder
            {
                DataSource = _config.Host + (string.IsNullOrEmpty(_config.Instance) ? "" : "\\" + _config.Instance),
                InitialCatalog = _config.Database,
                UserID = _config.User,
                Password = _config.Password
            };
            return sb.ConnectionString;
        }

        /// <summary>
        /// 以连接字符串形式配置连接
        /// </summary>
        /// <returns></returns>
        protected override DbConnection BuildConnection()
        {
            SqlConnection sc = new SqlConnection(_connect);
            return sc;
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <param name="wherecause"></param>
        /// <returns></returns>
        public override DataTable ExecuteQuery(SqlStatement statement, object param, string wherecause)
        {
            string sql = InflateParams(statement, param, wherecause);
            using (SqlConnection sc = (SqlConnection)BuildConnection())
            {
                SqlCommand sqlcmd = sc.CreateCommand();
                sqlcmd.CommandText = sql.Trim();
                sqlcmd.CommandTimeout = TimeoutCommandDefault;
                sc.Open();
                SqlDataAdapter sa= new SqlDataAdapter(sqlcmd);
                DataTable dt = new DataTable();
                sa.Fill(dt);
                sc.Close();
                return dt;
            }
        }
        /// <summary>
        /// 执行无返回的SQL
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <param name="wherecause"></param>
        /// <returns></returns>
        public override bool ExecuteCommand(SqlStatement statement, object param, string wherecause)
        {
            string sql = InflateParams(statement, param, wherecause);
            using (SqlConnection sc = (SqlConnection)BuildConnection())
            {
                SqlCommand sqlcmd = sc.CreateCommand();
                sqlcmd.CommandText = sql;
                sqlcmd.CommandTimeout = TimeoutCommandDefault;
                sc.Open();
                int i = sqlcmd.ExecuteNonQuery();
                sc.Close();
                return i > 0;
            }
        }

        /// <summary>
        /// 批量执行SQL语句 实现单会话 主要是为了在一次会话中使用临时表 事务型
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override bool ExecuteCommandBatch(List<SqlStatement> statements, List<object> parameters)
        {
            List<string> sqls = new List<string>();
            for (int i = 0; i < statements.Count; i++)
            {
                sqls.Add(InflateParams(statements[i], parameters[i]));
            }

            using (SqlConnection sc = (SqlConnection)BuildConnection())
            {
                sc.Open();
                SqlTransaction st = sc.BeginTransaction();
                SqlCommand sqlcmd = sc.CreateCommand();
                sqlcmd.CommandTimeout = TimeoutCommandDefault;
                sqlcmd.Transaction = st;
                try
                {
                    foreach (string s in sqls)
                    {
                        sqlcmd.CommandText = s;
                        int i = sqlcmd.ExecuteNonQuery();
                    }
                    st.Commit();
                    sc.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    st.Rollback();
                    throw ex;
                }
            }
        }
        /// <summary>
        /// 批量执行SQL语句 实现单会话 主要是为了在一次会话中使用临时表 非事务型
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override bool ExecuteCommandBatchWithoutTran(List<SqlStatement> statements, List<object> parameters)
        {
            List<string> sqls = new List<string>();
            for (int i = 0; i < statements.Count; i++)
            {
                sqls.Add(InflateParams(statements[i], parameters[i]));
            }
            using (SqlConnection sc = (SqlConnection)BuildConnection())
            {
                sc.Open();
                SqlCommand sqlcmd = sc.CreateCommand();
                foreach (string s in sqls)
                {
                    sqlcmd.CommandText = s;
                    int i = sqlcmd.ExecuteNonQuery();
                }
                sc.Close();
                return true;
            }
        }
    }
}

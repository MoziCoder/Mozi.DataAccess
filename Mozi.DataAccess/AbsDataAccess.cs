using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Mozi.DataAccess
{
    /// <summary>
    /// 数据访问抽象类
    /// </summary>
    public abstract class AbsDataAccess
    {        
        /// <summary>
        /// SQL命令超时时间 计时单位是秒
        /// </summary>
        protected const int TimeoutCommandDefault = 60;  
      
        protected string _connect = "";

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get { return _connect; } }
        /// <summary>
        /// 调试开关
        /// </summary>
        public bool Debug = false;
        /// <summary>
        /// 全局参数
        /// </summary>
        protected readonly List<SqlParameter> GlobalParams = new List<SqlParameter>();

        protected AbsDataAccess(string connString)
        {
            _connect = connString;
        }
        /// <summary>
        /// 取服务器版本信息字符串
        /// </summary>
        /// <returns></returns>
        public string GetServerVersion()
        {
            string serverVersion = "";
            using (DbConnection db = BuildConnection())
            {
                db.Open();
                serverVersion=db.ServerVersion;
                db.Close();
            }
            return serverVersion;
        }
        /// <summary>
        /// 增加全局参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        public void SetParam(string paramName, object paramValue)
        {
            if (GlobalParams.Exists(x=>x.name==paramName))
            {
                GlobalParams.Find(x => x.name == paramName).value = paramValue;
            }
            else
            {
              GlobalParams.Add(new SqlParameter(){name=paramName, value=paramValue});  
            }
        }
        /// <summary>
        /// 增加全局参数
        /// </summary>
        /// <param name="pms"></param>
        public void AddParams(Dictionary<string, object> pms)
        {
            foreach (var p in pms)
            {
                SetParam(p.Key, p.Value);
            }
        }
        /// <summary>
        /// 注入参数
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        protected string InflateParams(SqlStatement statement, object param)
        {
            return InflateParams(statement, param, "");
        }

        protected abstract DbConnection BuildConnection();
        
        /// <summary>
        /// 实时注入参数
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <param name="wherecause"></param>
        /// <returns></returns>
        protected string InflateParams(SqlStatement statement, object param, string wherecause)
        {
            string sql = statement.statement;
            //写入sql头部注释
            if (Debug)
            {
                sql = "/****" + statement.comment + "****/\n" + sql;
            }

            //参数检查
            Regex regGlobalParams = new Regex("\\$.*\\$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex regTrueParams = new Regex("#(parameter|pm|p|param|params)\\.\\w+#", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            //1,全局参数
            Regex regParamLocalDate/*本地格式化日期*/= new Regex("\\$date\\.?\\([yMdhHmsf]+(,[\\-\\+]?[\\d]+[yMdhHms])?\\)\\$", RegexOptions.Multiline);
            Regex regParamWhereCause = new Regex("\\$(wherecause|where)\\$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            //匹配全局参数
            foreach (var gp in GlobalParams)
            {
                Regex regGP = new Regex("\\$" + gp.name + "\\$");
                Regex regGP2 = new Regex("\\$\\." + gp.name + "\\$");
                if (gp.name.Equals("schema"))
                {
                    sql = regGP.Replace(sql, "[" + gp.value + "]");
                }
                else if (gp.value is string)
                {
                    sql = regGP.Replace(sql, "'" + gp.value + "'");
                    sql = regGP2.Replace(sql, gp.value.ToString());
                }
                else
                {
                    sql = regGP.Replace(sql, gp.value.ToString());
                }
            }

            //1,注入查询条件参数
            if (regParamWhereCause.IsMatch(sql) && !string.IsNullOrWhiteSpace(wherecause))
            {
                sql = regParamWhereCause.Replace(sql, wherecause);
            }

            //2,注入本地格式化时间
            sql = regParamLocalDate.Replace(sql, (x) =>
            {
                string sResult = "";
                string sDateFormat = x.ToString();
                string[] arrParams = sDateFormat.Split('(', ')');
                string sActString = arrParams[1];
                string[] arrActFunctions = sActString.Split(',');
                //1,$date(yyyyMMddHHmmssfff)
                //2,$date(yyyy,-1y)
                if (arrActFunctions.Length == 1)
                {
                    sResult = DateTime.Now.ToString(sActString);
                }
                else
                {
                    string sFormatParam = arrActFunctions[0];
                    string sActs = arrActFunctions[1];
                    //分割操作符号 操作数 操作量
                    string sActAmount = sActs.Length > 2 ? sActs.Substring(0, sActs.Length - 1) : "0";
                    int iActAmount = int.Parse(sActAmount);
                    string sActUnit = sActs.Substring(sActs.Length - 1, 1);
                    //yMdhHms;
                    switch (sActUnit)
                    {
                        case "y":
                        case "Y":
                            sResult = DateTime.Now.AddYears(iActAmount).ToString(sFormatParam);
                            break;
                        case "M":
                            sResult = DateTime.Now.AddMonths(iActAmount).ToString(sFormatParam);
                            break;
                        case "d":
                        case "D":
                            sResult = DateTime.Now.AddDays(iActAmount).ToString(sFormatParam);
                            break;
                        case "h":
                        case "H":
                            sResult = DateTime.Now.AddDays(iActAmount).ToString(sFormatParam);
                            break;
                        case "m":
                            sResult = DateTime.Now.AddMinutes(iActAmount).ToString(sFormatParam);
                            break;
                        case "s":
                        case "S":
                            sResult = DateTime.Now.AddSeconds(iActAmount).ToString(sFormatParam);
                            break;
                        default:
                            sResult = DateTime.Now.ToString(sFormatParam);
                            break;
                    }
                }
                //值的注入格式
                if (x.Value.Contains("date."))
                {
                    return sResult;
                }
                else
                {
                    return "'" + sResult + "'";
                }
            });
            //3,注入SQL参数
            foreach (string f in statement.parameter)
            {
                BindingFlags flag = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
                PropertyInfo p = param.GetType().GetProperty(f.Trim(), flag);
                if (p != null)
                {
                    //顺序注入
                    string fieldName = p.Name;
                    Type fieldType = p.PropertyType;
                    object fieldValue = p.GetValue(param, null);

                    Regex reg = new Regex("#(parameter|pm|p|param|params)\\.{1,2}" + fieldName + "#", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    sql = reg.Replace(sql,
                        (x) =>
                        {
                            if (fieldValue != null)
                            {
                                if (fieldType == typeof(string))
                                {
                                    if (x.Value.Contains(".."))
                                    {
                                        return fieldValue.ToString();
                                    }
                                    else
                                    {
                                        return "'" + fieldValue.ToString() + "'";
                                    }
                                }
                                else if (fieldType == typeof(DateTime))
                                {
                                    return "'" + fieldValue.ToString() + "'";
                                }
                                else
                                {
                                    return fieldValue.ToString();
                                }
                            }
                            else
                            {
                                return "''";
                            }
                        }, 1000);
                }
                else
                {
                    //抛出传入参数数量不足够
                }
            }

            #region 检查参数注入完整性

            //检查全局参数
            if (regGlobalParams.IsMatch(sql))
            {
                throw new Exception("全局参数不完整,接口:" + statement.name + "[" + regGlobalParams.Match(sql, 0) + "]");
            }
            //检查形式参数
            if (regTrueParams.IsMatch(sql))
            {
                throw new Exception("实际参数不完整,接口:" + statement.name + "[" + regTrueParams.Match(sql, 0).Value + "]");
            }

            #endregion

            return sql;
        }
        public abstract DataTable ExecuteQuery(SqlStatement statement, object param, string wherecause);
        public  virtual DataTable ExecuteQuery(SqlStatement statement, object param)
        {
            return ExecuteQuery(statement, param, "");
        }
        /// <summary>
        /// 无参 自拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public virtual T ExecuteQueryForTop<T>(string sql, List<string> results)
        {
            SqlStatement statement = new SqlStatement
            {
                statement = sql,
                name = "temp",
                comment = "自由SQL",
                results = results,
            };
            return ExecuteQueryForTop<T>(statement, null);
        }
        /// <summary>
        /// 只取一行值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual T ExecuteQueryForTop<T>(SqlStatement statement, object param)
        {
           return ExecuteQueryForTop<T>(statement, param,"");
        }
        /// <summary>
        /// 执行取一行查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <param name="wherecause"></param>
        /// <returns></returns>
        public virtual T ExecuteQueryForTop<T>(SqlStatement statement, object param, string wherecause)
        {
            T data = Activator.CreateInstance<T>();
            DataTable dt = ExecuteQuery(statement, param, wherecause);
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                //PropertyInfo[] props = data.GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                foreach (string item in statement.results)
                {
                    PropertyInfo prop = typeof(T).GetProperty(item, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        prop.SetValue(data, dr[item.Trim()], null);
                    }
                }
            }
            return data;
        }
        /// <summary>
        /// 无参 自拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public virtual T ExecuteQueryForOne<T>(string sql, List<string> results)
        {
            SqlStatement statement = new SqlStatement
            {
                statement = sql,
                name = "temp",
                comment = "自由SQL",
                results = results,
            };
            return ExecuteQueryForOne<T>(statement, null);
        }
        /// <summary>
        /// 只取一个值
        /// </summary>
        public virtual T ExecuteQueryForOne<T>(SqlStatement statement, object param)
        {
            return ExecuteQueryForOne<T>(statement, param, "");
        }
        /// <summary>
        /// 只取一个值 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <param name="wherecause"></param>
        /// <returns></returns>
        public virtual T ExecuteQueryForOne<T>(SqlStatement statement, object param, string wherecause)
        {
            T data;
            if (typeof(T) == Type.GetType("System.String"))
            {
                data = default(T);
            }
            else
            {
                data = Activator.CreateInstance<T>();
            }

            DataTable dt = ExecuteQuery(statement, param, wherecause);

            if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                if (!dr[0].Equals(DBNull.Value))
                {
                    data = (T)dr[0];
                }
                else
                {
                    return default(T);
                }
            }
            return data;
        }
        /// <summary>
        /// 无参 自拼接 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public virtual List<T> ExecuteQueryForList<T>(string sql, List<string> results)
        {
            SqlStatement statement = new SqlStatement
            {
                statement = sql,
                name = "temp",
                comment = "自由SQL",
                results = results,
            };
            return ExecuteQueryForList<T>(statement, null);
        }
        /// <summary>
        /// 执行取集查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <param name="wherecause"></param>
        /// <returns></returns>
        public virtual List<T> ExecuteQueryForList<T>(SqlStatement statement, object param, string wherecause)
        {
            List<T> list = new List<T>();
            DataTable dt = ExecuteQuery(statement, param, wherecause);

            //PropertyInfo[] props = data.GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            dt.AsEnumerable().ToList().ForEach(dr =>
            {
                T f = Activator.CreateInstance<T>();
                foreach (string item in statement.results)
                {
                    PropertyInfo prop = typeof(T).GetProperty(item, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        var v = dr[item.Trim()];
                        if (!v.Equals(DBNull.Value))
                        {
                            prop.SetValue(f, v, null);
                        }
                        else
                        {
                            prop.SetValue(f, default(T), null);
                        }
                    }
                }
                list.Add(f);
            });
            return list;
        }
        /// <summary>
        /// 取集合值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual List<T> ExecuteQueryForList<T>(SqlStatement statement, object param)
        {
            return ExecuteQueryForList<T>(statement, param, "");
        }
        /// <summary>
        /// 无参 自拼接
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual bool ExecuteCommand(string sql)
        {
            SqlStatement statement = new SqlStatement
            {
                statement = sql,
                name = "temp",
                comment = "自由SQL",
                results = null
            };
            return ExecuteCommand(statement, null);
        }
        /// <summary>
        /// 执行无返回的SQL
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual bool ExecuteCommand(SqlStatement statement, object param)
        {
            return ExecuteCommand(statement, param, "");
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="param"></param>
        /// <param name="wherecause"></param>
        /// <returns></returns>
        public abstract bool ExecuteCommand(SqlStatement statement, object param, string wherecause);
        /// <summary>
        /// 执行事务命令
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract bool ExecuteCommandBatch(List<SqlStatement> statements, List<object> parameters);
        /// <summary>
        /// 批量执行命令 非事务
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract bool ExecuteCommandBatchWithoutTran(List<SqlStatement> statements, List<object> parameters);
    }  
}

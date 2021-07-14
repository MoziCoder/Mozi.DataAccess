using System;
using System.Collections.Generic;

namespace Mozi.DataAccess
{
    /// <summary>
    /// SQL表达式容器
    /// </summary>
    public class SqlMapContainer
    {

        private readonly List<SqlStatement> _statements = new List<SqlStatement>();

        private static SqlMapContainer _container;

        public int Count { get { return _statements.Count; } }

        public List<SqlStatement> Statements { get { return _statements; } }

        public static SqlMapContainer Instance
        {
            get { return _container ?? (_container = new SqlMapContainer()); }
        }

        private SqlMapContainer()
        {

        }
        /// <summary>
        /// 获取表达式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SqlStatement Find(string name)
        {
            return Instance.Get(name);
        }
        /// <summary>
        /// 获取表达式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SqlStatement Get(string name)
        {
            if (_statements.Exists(x => x.name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return _statements.Find(x => x.name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                throw new Exception("本地不存在[" + name + "]的接口");
            }
        }
        /// <summary>
        /// 注入表达式
        /// </summary>
        /// <param name="statement"></param>
        public void AddStatement(SqlStatement statement)
        {
            _statements.RemoveAll(x => x.name.Equals(statement.name, StringComparison.OrdinalIgnoreCase));
            _statements.Add(statement);
        }
        /// <summary>
        /// 批量注入表达式
        /// </summary>
        /// <param name="statements"></param>
        public void AddStatements(List<SqlStatement> statements)
        {
            foreach (SqlStatement s in statements)
            {
                AddStatement(s);
            }
        }

        /// <summary>
        /// 清空所有
        /// </summary>
        public void Clear()
        {
            _statements.Clear();
        }

        public SqlStatement this[string index]
        {
            get { return Get(index); }
        }
    }
}

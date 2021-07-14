using System;
using System.Collections.Generic;

namespace Mozi.DataAccess
{
    /// <summary>
    /// 表达式
    /// </summary>
    public class SqlStatement
    {
        public string name            { get; set; }
        public string command         { get; set; }
        public string comment         { get; set; }
        public string statement       { get; set; }
        public List<string> results   { get; set; }
        public List<string> parameter { get; set; }

        public SqlStatement()
        {
            parameter = new List<string>();
            results = new List<string>();
        }
    }

    /// <summary>
    /// SQL表达式编译结果
    /// </summary>
    public class SqlStatementApplied : SqlStatement
    {
        public string      datasource       { get; set; }
        public object      globalparams     { get; set; }
        public object      parameteractual  { get; set; }
        public int         executed         { get; set; }
        public DateTime    executetime      { get; set; }
        public DateTime    resulttime       { get; set; }
        public object      resultset        { get; set; }
        public int         trycount         { get; set; }
    }
    /// <summary>
    /// 结果
    /// </summary>
    public class SqlResults
    {
        public string name  { get; set; }
        public Type   type  { get; set; }
        public string value { get; set; }
    }
    /// <summary>
    /// SQL语句实参
    /// </summary>
    public class SqlParameter : object
    {
        public string name         { get; set; }
        public object value        { get; set; }
        public string matchquery   { get; set; }
        public Type   valuetype    { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Mozi.DataAccess
{
    /// <summary>
    /// SQL语句表达式
    /// </summary>
    [Serializable]
    public class SqlStatement
    {
        /// <summary>
        /// SQL语句全局唯一名称
        /// </summary>
        public string name            { get; set; }
        /// <summary>
        /// SQL语句命令类型 query|update|delete|inert 
        /// </summary>
        public string command         { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string comment         { get; set; }
        /// <summary>
        /// SQL语句模版
        /// </summary>
        public string statement       { get; set; }
        /// <summary>
        /// 结果字段集合
        /// </summary>
        public List<string> results   { get; set; }
        /// <summary>
        /// 参数字段集合
        /// </summary>
        public List<string> parameter { get; set; }
        /// <summary>
        /// 语句代码路径
        /// </summary>
        public CodeBase codebase { get; set; }
        public SqlStatement()
        {
            parameter = new List<string>();
            results = new List<string>();
        }
    }
    [Serializable]
    public class CodeBase
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }
    /// <summary>
    /// SQL表达式编译结果
    /// </summary>
    [Serializable]
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
    [Serializable]
    public class SqlResults
    {
        public string name  { get; set; }
        public Type   type  { get; set; }
        public string value { get; set; }
    }
    /// <summary>
    /// SQL语句实参
    /// </summary>
    [Serializable]
    public class SqlParameter : object
    {
        public string name         { get; set; }
        public object value        { get; set; }
        public string matchquery   { get; set; }
        public Type   valuetype    { get; set; }
    }
}

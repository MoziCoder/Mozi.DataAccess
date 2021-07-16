using System;
using System.Collections.Generic;
using Mozi.DataAccess.Config;

namespace Mozi.DataAccess.TaskQuence
{
    /// <summary>
    /// sql任务
    /// </summary>
    public class SqlTask
    {
        public SqlStatement statement { get; set; }
        public ServerConfig session { get; set; }
        public object actparams { get; set; }
        public DateTime buildtime { get; set; }
        public DateTime begintime { get; set; }
        public DateTime endtime { get; set; }
        public string message { get; set; }
        public int trycount { get; set; }
        public int errorcount { get; set; }
        public object result { get; set; }
        public bool success { get; set; }
        public Dictionary<string, object> globalparams = new Dictionary<string, object>();
    }
}

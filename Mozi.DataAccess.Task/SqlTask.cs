using System;
using System.Collections.Generic;
using Mozi.DataAccess.Config;

namespace Mozi.DataAccess.TaskQuence
{
    /// <summary>
    /// sql任务
    /// </summary>
    [Serializable]
    public class SqlTask
    {
        public string TaskId { get; set; }
        public SqlTaskState TaskState { get; set; }
        public SqlStatement Statement { get; set; }
        public ServerConfig Session { get; set; }
        public object ActParams { get; set; }
        public DateTime BuildTime { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Message { get; set; }
        public int TryCount { get; set; }
        public int ErrorCount { get; set; }
        public object Result { get; set; }
        public bool Success { get; set; }

        public Dictionary<string, object> globalparams = new Dictionary<string, object>();
        public SqlTask()
        {
            TaskId = Guid.NewGuid().ToString("N");
            TaskState = SqlTaskState.Idle;
        }
    }
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum SqlTaskState
    {
        Idle=1,
        Wait=2,
        Executing=3,
        Complete=4
    }
}

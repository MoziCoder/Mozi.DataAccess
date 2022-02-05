using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mozi.DataAccess.Config;

namespace Mozi.DataAccess.TaskQuence
{
    /// <summary>
    /// sql任务管理 自动线程均衡
    /// </summary>
    //TODO 实现一个定时器
    public class SqlTaskManager
    {
        private static SqlTaskManager _sm;

        private bool _autoStart = false;

        private const uint ThreadCount = 4;

        private uint _maxTaskTryCount = 4;
        private uint _maxThreadCount = 4;

        private readonly ConcurrentQueue<SqlTask> _recycleDocker = new ConcurrentQueue<SqlTask>();

        private readonly List<ConcurrentQueue<SqlTask>> _docker = new List<ConcurrentQueue<SqlTask>>();

        private readonly List<Task> _tasks = new List<Task>();
        private readonly List<CancellationTokenSource> _tokens = new List<CancellationTokenSource>();

        //队列调度器
        private Timer _dispatcher;

        //队列调度器审视间隔
        private uint _dispatchperiod = 100;

        //任务统计
        private object _statistics = new object();

        private ManualResetEvent _restEvent = new ManualResetEvent(true);
        /// <summary>
        /// 任务追加事件
        /// </summary>
        public event TaskAdded OnTaskAdded;
        /// <summary>
        /// 任务状态变更事件
        /// </summary>
        public event TaskStateChange OnTaskStateChange;

        /// <summary>
        /// 任务最大尝试次数
        /// </summary>
        public uint MaxTaskTryCount
        {
            get { return _maxTaskTryCount; }
            set { _maxTaskTryCount = value; }
        }

        /// <summary>
        /// 最大线程数
        /// </summary>
        public uint MaxThreadCount
        {
            get { return _maxThreadCount; }
            set { _maxThreadCount = value; }
        }

        /// <summary>
        /// 进队任务是否自动执行
        /// </summary>
        public bool AutoStart
        {
            get { return _autoStart; }
            set { _autoStart = value; }
        }

        public static SqlTaskManager Instance
        {
            get { return _sm ?? (_sm = new SqlTaskManager()); }
        }

        private SqlTaskManager()
        {
            _dispatcher = new Timer(DispatchHandler, null, 0, _dispatchperiod);
            for (int i = 0; i < MaxThreadCount; i++)
            {
                _docker.Add(new ConcurrentQueue<SqlTask>());
            }

            for (int i = 0; i < MaxThreadCount; i++)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                Task t = new Task((x) => { }, tokenSource.Token);
                _tasks.Add(t);
                _tokens.Add(tokenSource);
                t.Start();
            }
        }

        /// <summary>
        /// 暂时不使用加权均衡策略
        /// </summary>
        /// <param name="state"></param>
        private void DispatchHandler(object state)
        {
            //先检查回收队列中是否有任务
            if (_recycleDocker.Count > 0)
            {
                foreach (var rqs in _recycleDocker)
                {
                    Enqueue(rqs);
                }
            }
            //再平衡 
            int avgcount = _docker.Sum(x => x.Count) / _docker.Count;
            //TODO 明明是队列头 重排队却进入到了队尾 是否合理
            foreach (var h in _docker.OrderByDescending(x => x.Count))
            {
                if (h.Count < avgcount && avgcount > 0)
                {
                    SqlTask t;
                    if (h.TryDequeue(out t))
                    {
                        Enqueue(t);
                    }
                }
            }
            //唤醒线程
            Start();
        }
        /// <summary>
        /// 入队 优先空闲线程
        /// </summary>
        /// <param name="st"></param>
        private void Enqueue(SqlTask st)
        {
            //分配队列
            ConcurrentQueue<SqlTask> target = _docker.OrderBy(x => x.Count).First();
            target.Enqueue(st);
            SetTaskState(st, SqlTaskState.Wait);
            Console.WriteLine("{1}进入队列{0}", _docker.IndexOf(target), DateTime.Now);
            Console.WriteLine("{0},{1},{2},{3}", _docker[0].Count, _docker[1].Count, _docker[2].Count, _docker[3].Count);

            if (OnTaskAdded != null)
            {
                OnTaskAdded(this, st);
            }

        }
        //TODO 要考虑任务的保存和恢复的问题，执行未结束的任务应该可恢复执行

        /// <summary>
        /// 装填任务
        /// </summary>
        /// <param name="_ts"></param>
        public void Load(ICollection<SqlTask> _ts)
        {
            foreach (var sqlTask in _ts)
            {
                Enqueue(sqlTask);
            }
        }

        private void SetTaskState(SqlTask st,SqlTaskState state)
        {
            var oldState = st.TaskState;
            st.TaskState = state;
            if (oldState != state && OnTaskStateChange != null)
            {
                OnTaskStateChange(this, st,oldState, st.TaskState);
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="st"></param>
        private void Execute(SqlTask st)
        {
            Console.WriteLine(" {0} 进入执行状态", st.Statement.name + st.Session.ConnectionName);
            SetTaskState(st, SqlTaskState.Executing);
            if (st.TryCount < 4)
            {
                ServerConfig server = st.Session;
                SqlStatement ss = st.Statement;
                SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder
                {
                    DataSource = server.Host + (string.IsNullOrEmpty(server.Instance) ? "" : "\\" + server.Instance),
                    InitialCatalog = server.Database,
                    UserID = server.User,
                    Password = server.Password
                };
                SQLServer.Access rss = new SQLServer.Access(sb.ConnectionString);
                rss.AddParams(st.globalparams);
                try
                {
                    rss.ExecuteCommand(ss, st.ActParams);
                    st.Success = true;
                }
                catch (Exception ex)
                {
                    st.Message = ex.Message;
                    st.ErrorCount++;
                }
                finally
                {
                    st.TryCount++;
                }
            }
            else
            {

            }
        }
        /// <summary>
        /// 开始执行任务
        /// </summary>
        public void Start()
        {
            for (int i = 0; i < MaxThreadCount; i++)
            {
                Task t = _tasks[i];
                var i1 = i;
                if (t.IsCompleted)
                {
                    t.ContinueWith((x) => ThreadInvoker(i1));
                }
            }
        }
        /// <summary>
        /// 终止所有线程
        /// </summary>
        public void Stop()
        {
            for (int i = 0; i < MaxThreadCount; i++)
            {
                var t = _tokens[i];
                t.Cancel();
            }
        }

        private void ThreadInvoker(object args)
        {
            int i = (int)args;
            ConcurrentQueue<SqlTask> queue = _docker[i];
            Console.WriteLine(queue.IsEmpty ? string.Format("队列{0},空闲",i) : string.Format("队列{0},调用开始", i));
            //信号指示
            while (!queue.IsEmpty)
            {

                SqlTask task;
                if (queue.TryDequeue(out task))
                {
                    try
                    {
                        Execute(task);
                        if (!task.Success)
                        {
                            _recycleDocker.Enqueue(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        task.Message += ex.Message;
                    }
                    finally
                    {
                        SetTaskState(task, SqlTaskState.Complete);
                    }
                }
            }
            //lock (_restEvent)
            //{
            //    //无限期等待
            //    _restEvent.WaitOne();
            //    ThreadInvoker(args);
            //}
        }
    }
}
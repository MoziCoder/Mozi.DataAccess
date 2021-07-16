using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mozi.DataAccess.Task
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

        private readonly ConcurrentQueue<SqlTask> _recycleposition = new ConcurrentQueue<SqlTask>();

        private readonly List<ConcurrentQueue<SqlTask>> _docker = new List<ConcurrentQueue<SqlTask>>();

        private readonly List<System.Threading.Tasks.Task> _tasks = new List<System.Threading.Tasks.Task>();
        //队列调度器
        private Timer _dispatcher;

        //队列调度器审视间隔
        private uint _dispatchperiod = 100;

        //任务统计
        private object _statistics = new object();

        private ManualResetEvent _restEvent = new ManualResetEvent(true);

        //单任务最大尝试次数
        public uint MaxTaskTryCount
        {
            get { return _maxTaskTryCount; }
            set { _maxTaskTryCount = value; }
        }

        //最大线程数
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

        private SqlTaskManager()
        {
            _dispatcher = new Timer(DispatchHandler, null, 0, _dispatchperiod);
            for (int i = 0; i < MaxThreadCount; i++)
            {
                _docker.Add(new ConcurrentQueue<SqlTask>());
            }

            for (int i = 0; i < MaxThreadCount; i++)
            {
                Task t = new Task((x) => { }, TaskScheduler.Default);
                _tasks.Add(t);
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
            if (_recycleposition.Count > 0)
            {
                foreach (var rqs in _recycleposition)
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
            Begin();
        }

        public static SqlTaskManager Instance
        {
            get { return _sm ?? (_sm = new SqlTaskManager()); }
        }

        /// <summary>
        /// 入队 优先空闲线程
        /// </summary>
        /// <param name="t"></param>
        private void Enqueue(SqlTask t)
        {
            //分配队列
            ConcurrentQueue<SqlTask> target = _docker.OrderBy(x => x.Count).First();
            target.Enqueue(t);
            Console.WriteLine("{1}进入队列{0}", _docker.IndexOf(target), DateTime.Now);
            Console.WriteLine("{0},{1},{2},{3}", _docker[0].Count, _docker[1].Count, _docker[2].Count, _docker[3].Count);

        }
        /// <summary>
        /// 载入任务
        /// </summary>
        /// <param name="_ts"></param>
        public void Load(ICollection<SqlTask> _ts)
        {
            foreach (var sqlTask in _ts)
            {
                Enqueue(sqlTask);
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="st"></param>
        private void Execute(SqlTask st)
        {
            Console.WriteLine(" {0} 进入执行状态", st.statement.name + st.session.ConnectionName);
            if (st.trycount < 4)
            {
                ServerConfig server = st.session;
                SqlStatement ss = st.statement;
                SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder
                {
                    DataSource = server.Host + (string.IsNullOrEmpty(server.Instance) ? "" : "\\" + server.Instance),
                    InitialCatalog = server.Database,
                    UserID = server.User,
                    Password = server.Password
                };
                DataAccess.SQLServer.Access rss = new DataAccess.SQLServer.Access(sb.ConnectionString);
                rss.AddParams(st.globalparams);
                try
                {
                    rss.ExecuteCommand(ss, st.actparams);
                    st.success = true;
                }
                catch (Exception ex)
                {
                    st.message = ex.Message;
                    st.errorcount++;
                }
                finally
                {
                    st.trycount++;
                }
            }
            else
            {

            }
        }

        public void Begin()
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
        //
        private void ThreadInvoker(object args)
        {
            int i = (int)args;
            ConcurrentQueue<SqlTask> queue = _docker[i];
            Console.WriteLine(queue.IsEmpty ? "空队列" : "调用开始");
            //信号指示
            while (!queue.IsEmpty)
            {

                SqlTask task;
                if (queue.TryDequeue(out task))
                {
                    try
                    {
                        Execute(task);
                        if (!task.success)
                        {
                            _recycleposition.Enqueue(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        task.message += ex.Message;
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
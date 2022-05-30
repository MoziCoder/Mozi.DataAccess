using System.Collections.Generic;

namespace Mozi.DataAccess
{
    /// <summary>
    /// 多数据库管理类
    /// </summary>
    public class DataManager
    {
        private static List<AbsDataAccess> _dbs;

        private static DataManager _dm;

        public static DataManager Instance
        {
            get { return _dm ?? (new DataManager()); }
        }

        private DataManager()
        {
            _dbs = new List<AbsDataAccess>();
        }
        /// <summary>
        /// 初始化数据访问层
        /// </summary>
        /// <param name="db"></param>
        public void AddProvider(AbsDataAccess db)
        {

            _dbs.Add(db);
        }
    }
}

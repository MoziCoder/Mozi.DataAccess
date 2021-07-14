namespace Mozi.DataAccess
{
    /// <summary>
    /// 多数据库管理类
    /// </summary>
    public class DataManager
    {
        private static AbsDataAccess _db;

        private static DataManager _dm;

        public static DataManager Instance
        {
            get { return _dm ?? (new DataManager()); }
        }

        private DataManager()
        {

        }
        /// <summary>
        /// 初始化数据访问层
        /// </summary>
        /// <param name="db"></param>
        public void Init(AbsDataAccess db)
        {
            _db = db;
        }
    }
}

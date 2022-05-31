using Mozi.DataAccess.Config;
using Mozi.DataAccess.Test.Model;

namespace Mozi.DataAccess.Test.DA
{
    public class DaUser
    {
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        SQLServer.Access _server = new SQLServer.Access(new ServerConfig()
        {
            Host="127.0.0.1",
            Instance="",
            User="sa",
            Password="123456",
            ConnectionName="测试库",
            Database="example"
        });
        /// <summary>
        /// 查询指定的用户
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public User GetUsers(string userid)
        {
            SqlStatement sql = SqlMapContainer.Find("mz.getuserinfo");
            return _server.ExecuteQueryForTop<User>(sql, new { UserId = userid });
        }
    }
}
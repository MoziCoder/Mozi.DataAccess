using Mozi.DataAccess.Config;
using Mozi.DataAccess.Test.Model;

namespace Mozi.DataAccess.Test.DA
{
    public class DaUser
    {
        /// <summary>
        /// ���ݿ���ʶ���
        /// </summary>
        SQLServer.Access _server = new SQLServer.Access(new ServerConfig()
        {
            Host="127.0.0.1",
            Instance="",
            User="sa",
            Password="123456",
            ConnectionName="���Կ�",
            Database="example"
        });

        /// <summary>
        /// ��ʼ��ȫ�ֲ���
        /// </summary>
        public void Init()
        {
            //schema����
            _server.SetParam("schema", "0000");
        }

        /// <summary>
        /// ��ѯָ�����û�
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
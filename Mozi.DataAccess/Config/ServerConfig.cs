namespace Mozi.DataAccess.Config
{
    public class ServerConfig
    {
        public string ConnectionName { get; set; }
        public string Remark { get; set; }
        public string Domain { get; set; }
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Instance { get; set; }

        public ServerConfig()
        {
            Host = "";
            Database = "";
            User = "";
            Password = "";
            Instance = "";
            Domain = "";
            ConnectionName = "";
        }
    }
}
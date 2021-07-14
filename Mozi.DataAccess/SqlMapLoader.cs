using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace Mozi.DataAccess
{
    /// <summary>
    /// 映射对象载入器
    /// </summary>
    public class SqlMapLoader
    {
        private static SqlMapLoader _loader;

        private readonly SqlMapContainer _mapContainer;

        private string _mapFileDir = AppDomain.CurrentDomain.BaseDirectory + @"Maps\";

        private string _mapFileExt = ".json";

        private SqlMapLoader()
        {
            _mapContainer = SqlMapContainer.Instance;
        }
        /// <summary>
        /// 单实例
        /// </summary>
        public static SqlMapLoader Instance
        {
            get { return _loader ?? (_loader = new SqlMapLoader()); }
        }

        public void FindScripts(string path)
        {

        }
        /// <summary>
        /// 配置映射文件扫描目录
        /// </summary>
        /// <param name="dir"></param>
        public void SetMapRoot(string dir)
        {
            _mapFileDir = dir;
        }
        /// <summary>
        /// 设置映射文件扩展名
        /// </summary>
        /// <param name="extName"></param>
        public void SetMapFileExt(string extName)
        {
            _mapFileExt = extName;
        }
        /// <summary>
        /// 装载全局参数
        /// </summary>
        public void LoadGlobal()
        {
            string globalPath = _mapFileDir+"Global"+_mapFileExt;
            //载入全局变量
        }
        /// <summary>
        /// 目录扫描
        /// </summary>
        public void LoadScripts()
        {
            LoadScripts(_mapFileDir);
        }
        /// <summary>
        /// 目录扫描
        /// </summary>
        /// <param name="dirname"></param>
        public void LoadScripts(string dirname)
        {
            DirectoryInfo dir = new DirectoryInfo(dirname);
            _mapContainer.Clear();
            foreach (FileInfo d in dir.GetFiles())
            {
                //全局参数由全局载入器载入
                if (d.Name.StartsWith("global", true, CultureInfo.CurrentCulture))
                {
                    continue;
                }
                if (d.Extension.Equals(_mapFileExt))
                {
                    //提取脚本内容
                    StreamReader sr = d.OpenText();
                    try
                    {
                        string text = sr.ReadToEnd();
                        List<SqlStatement> listSqls = AnalysisScripts(text);
                        if (listSqls != null)
                        {
                            _mapContainer.AddStatements(listSqls);
                        }
                    }
                    finally
                    {
                        sr.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 分析脚本指令
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<SqlStatement> AnalysisScripts(string text)
        {
            var list = JsonConvert.DeserializeObject<List<SqlStatement>>(text);
            return list;
        }
    }
}

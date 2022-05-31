# Mozi.DataAccess ORM框架

## 简介

Mozi.DataAccess是一个基于.Net开发的SQL ORM套件。框架的设计理念是：轻量，简洁，务实。

## 目的和愿景

开发这个项目就是为了最大限度的降低学习和使用成本，减少项目重构成本，提高开发效率。ORM这个圈子本身是有很多成熟的优秀的框架，今年流行这个明年流行那个，常常使我们疲于学习。
无论框架怎么变，一个ORM框架的核心无非是：1，持久化；2，对象映射；3，数据库访问。在以上三点的基础上需要同时保证：1，易用且实用；2，充分解耦合重构成本低；3，性能损耗低。

## 特点

1. 轻量化  
	项目编译结果小，没有复杂的配置文件

2. 可用性  
	框架经过了长周期的项目考验

3. 低耦合  
	实现了业务逻辑和SQL的彻底分离，框架只专注于数据库的访问

4. 可控性  
	框架的使用最大限度的保留了SQL的原貌

## 模块

1. 数据库访问

2. 对象映射

3. SQL执行队列


## 数据库适配

- SQLServer
- Sqlite
- MySql


## SQL表达式定义
~~~json

[{
	"name": "mz.createtableuser",
	"command": "query",
	"parameter": [ ],
	"statement": "
		IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE id=object_id(\'$schema$.tbUsers\') AND TYPE =\'U\'))
		CREATE TABLE tbUsers
		(
				UserId   varchar(10) default \'\' not null ,
				NickName varchar(100) default \'\' not null,
				UserPwd  varchar(32) default \'\' not null,
				RegDate  date not null,
				Mobile   varchar(20) default \'\' not null,
				IsForbidden int default 0 not null
				CONSTRAINT PK_TBUSERS PRIMARY KEY (UserId)
		)
	",
	"results": [ ],
	"comment": "创建用户信息表"
},{
	"name": "mz.getuserinfo",
	"command": "query",
	"parameter": [ "UserId" ],
	"statement": "select * from $schema$.tbUsers where UserId=#param.UserId# ",
	"results": [ "UserId", "Nickname" ],
	"comment": "获取用户信息"
}]   

~~~
## 模型类定义
~~~
using System;

namespace Mozi.DataAccess.Test.Model
{
	public class User
    {
        public string UserId    { get; set; }
        public string NickName  { get; set; }
        public string Password  { get; set; }
        public string Mobile    { get; set; }
        public DateTime RegDate { get; set; }
        public int IsForbidden  { get; set; }
    }
}

~~~

## 数据访问接口定义

~~~csharp

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

~~~

## 全局参数注入

## 版权说明

本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

### By [Jason][1] on Oct. 12,2017 

[1]:mailto:brotherqian@163.com


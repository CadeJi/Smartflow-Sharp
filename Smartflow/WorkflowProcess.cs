﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Smartflow.Enums;
using System.Data;

namespace Smartflow
{
    public class WorkflowProcess : IPersistent, IRelationShip
    {
        protected IDbConnection Connection
        {
            get { return DapperFactory.CreateWorkflowConnection(); }
        }

        /// <summary>
        /// 外键
        /// </summary>
        public long RNID
        {
            get;
            set;
        }

        /// <summary>
        /// 唯一标识
        /// </summary>
        public long NID
        {
            get;
            set;
        }

        /// <summary>
        /// 当前节点
        /// </summary>
        public string FROM
        {
            get;
            set;
        }

        /// <summary>
        /// 跳转到的节点
        /// </summary>
        public string TO
        {
            get;
            set;
        }

        /// <summary>
        /// 路线ID
        /// </summary>
        public long TID
        {
            get;
            set;
        }

        /// <summary>
        /// 实例ID
        /// </summary>
        public string INSTANCEID
        {
            get;
            set;
        }

        /// <summary>
        /// 节点类型
        /// </summary>
        public WorkflowNodeCategeory NODETYPE
        {
            get;
            set;
        }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CREATEDATETIME
        {
            get;
            set;
        }


        /// <summary>
        /// 将数据持久到数据库
        /// </summary>
        public void Persistent()
        {
            string sql = "INSERT INTO T_PROCESS([FROM],[TO],TID,INSTANCEID,NODETYPE,RNID) VALUES(@FROM,@TO,@TID,@INSTANCEID,@NODETYPE,@RNID)";
            Connection.Execute(sql, new
            {
                FROM = FROM,
                TO = TO,
                TID = TID,
                INSTANCEID = INSTANCEID,
                NODETYPE = NODETYPE.ToString(),
                RNID = RNID
            });
        }

        public static WorkflowProcess GetWorkflowProcessInstance(string instanceID, long NID)
        {
            WorkflowProcess instance = new WorkflowProcess();
            //兼容其它数据库
            string query = " SELECT * FROM T_PROCESS WHERE INSTANCEID=@INSTANCEID AND RNID=@NID ORDER BY CREATEDATETIME DESC ";
            instance = instance.Connection.Query<WorkflowProcess>(query, new
            {
                INSTANCEID = instanceID,
                NID = NID

            }).OrderByDescending(order => order.CREATEDATETIME).FirstOrDefault();

            return instance;
        }
    }
}

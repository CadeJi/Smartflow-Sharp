﻿/********************************************************************
 License: https://github.com/chengderen/Smartflow/blob/master/LICENSE 
 Home page: https://www.smartflow-sharp.com
 ********************************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Smartflow.Elements;
using Smartflow.Enums;

namespace Smartflow
{
    /**
     * 工作流引擎，由工作流引擎统一提供对外服务接口，以此来驱动流程跳转
     */
    public  class WorkflowEngine
    {
        private IWorkflow workflowService = WorkflowServiceProvider.OfType<IWorkflow>();

        public static event DelegatingProcessHandle OnProcess;

        public static event DelegatingCompletedHandle OnCompleted;

        /// <summary>
        /// 触发流程跳转事件
        /// </summary>
        /// <param name="executeContext">执行上下文</param>
        protected void OnExecuteProcess(ExecutingContext executeContext)
        {
            Processing(executeContext);
            OnProcess(executeContext);
            if (OnCompleted != null && executeContext.To.NodeType == WorkflowNodeCategory.End)
            {
                OnCompleted(executeContext);
            }
        }

        /// <summary>
        /// 根据传递的流程XML字符串,启动工作流
        /// </summary>
        /// <param name="resourceXml"></param>
        /// <returns></returns>
        public string Start(string resourceXml)
        {
            return workflowService.Start(resourceXml);
        }

        /// <summary>
        /// 终止流程
        /// </summary>
        /// <param name="instance">工作流实例</param>
        public void Kill(WorkflowInstance instance)
        {
            workflowService.Kill(instance);
        }

        /// <summary>
        /// 中断流程
        /// </summary>
        /// <param name="instance">工作流实例</param>
        public void Terminate(WorkflowInstance instance)
        {
            workflowService.Terminate(instance);
        }

        /// <summary>
        /// 恢复流程
        /// </summary>
        /// <param name="instance">工作流实例</param>
        public void Revert(WorkflowInstance instance)
        {
            workflowService.Revert(instance);
        }

        /// <summary>
        /// 进行流程跳转
        /// </summary>
        /// <param name="context"></param>
        public void Jump(WorkflowContext context)
        {
            WorkflowInstance instance = context.Instance;
            if (instance.State == WorkflowInstanceState.Running)
            {
                WorkflowNode current = instance.Current;

                context.SetOperation(WorkflowAction.Jump);


                string transitionTo = current.Transitions
                                  .FirstOrDefault(e => e.NID == context.TransitionID).Destination;

                current.SetActor(context.ActorID, context.ActorName, WorkflowAction.Jump);
                instance.Jump(transitionTo);

                ASTNode to = current.GetNode(transitionTo);
                OnExecuteProcess(new ExecutingContext()
                {
                    From = current,
                    To = to,
                    TransitionID = context.TransitionID,
                    Instance = instance,
                    Data = context.Data,
                    Operation = context.Operation,
                    ActorID = context.ActorID,
                    ActorName = context.ActorName
                });

                if (to.NodeType == WorkflowNodeCategory.End)
                {
                    instance.State = WorkflowInstanceState.End;
                    instance.Transfer();
                }
                else if (to.NodeType == WorkflowNodeCategory.Decision)
                {
                    WorkflowDecision wfDecision = WorkflowDecision.ConvertToReallyType(to);
                    Transition transition = wfDecision.GetTransition();
                    if (transition == null) return;
                    Jump(new WorkflowContext()
                    {
                        Instance = WorkflowInstance.GetInstance(instance.InstanceID),
                        TransitionID = transition.NID,
                        ActorID = context.ActorID,
                        Data = context.Data
                    });
                }
            }
        }

        /// <summary>
        /// 跳转过程处理入库
        /// </summary>
        /// <param name="executeContext">执行上下文</param>
        protected void Processing(ExecutingContext executeContext)
        {
            workflowService.Processing(new WorkflowProcess()
            {
                RelationshipID = executeContext.To.NID,
                Origin = executeContext.From.ID,
                Destination = executeContext.To.ID,
                TransitionID = executeContext.TransitionID,
                InstanceID = executeContext.Instance.InstanceID,
                NodeType = executeContext.From.NodeType,
                Operation = executeContext.Operation
            });
        }
    }
}

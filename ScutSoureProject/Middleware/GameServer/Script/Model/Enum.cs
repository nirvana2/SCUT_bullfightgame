using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Script.Model
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum ResActionType
    {
        /// <summary>
        /// 加
        /// </summary>
        Add = 1,
        /// <summary>
        /// 减
        /// </summary>
        Minus = -1
    }

    public enum TaskTypeEnum
    {
        /// <summary>
        /// 雇佣员工总数量
        /// </summary>
        Employee = 1,
        /// <summary>
        /// 建设楼层总数量
        /// </summary>
        Floor = 2,
        /// <summary>
        /// 触发后邀请玩家数量
        /// </summary>
        User = 3,
        /// <summary>
        /// 触发后收取金币次数
        /// </summary>
        Coin = 4,
        /// <summary>
        /// 触发后补货次数
        /// </summary>
        Goods = 5,
        /// <summary>
        /// 触发后装修次数
        /// </summary>
        Fitment = 6,
        /// <summary>
        /// 触发后开电梯次数
        /// </summary>
        Lift = 7
    }

    public enum TaskStatusEnum
    {
        /// <summary>
        /// 任务进行中
        /// </summary>
        Ing = 1,
        /// <summary>
        /// 待领奖
        /// </summary>
        Wait = 2,
        /// <summary>
        /// 已领取奖励结束任务
        /// </summary>
        Over = 3
    }

    public enum MailTypeEnum
    {
        /// <summary>
        /// 交易
        /// </summary>
        Trading = 1
    }
}

using System;
using ProtoBuf;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Model;
using ZyGames.Framework.Event;

namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Dictionary, DbConfig.ConnData)]
    public class tb_Task : BaseEntity
    {
        public tb_Task()
            : base(AccessLevel.ReadWrite)
        {
            Items = new CacheList<UserTask>();
        }

        /// <summary>
        /// 玩家ID
        /// </summary>
        [ProtoMember(1)]
        [EntityField(true)]
        public int UserId { get; set; }

        /// <summary>
        /// 任务集合
        /// </summary>
        [ProtoMember(2)]
        [EntityField(true, ColumnDbType.LongText)]
        public CacheList<UserTask> Items { get; set; }

        protected override int GetIdentityId() { return UserId; }
    }

    [Serializable, ProtoContract]
    public class UserTask : EntityChangeEvent
    {
        public UserTask()
            : base(false)
        {
        }

        /// <summary>
        /// 任务ID
        /// </summary>
        [ProtoMember(1)]
        public int Id { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        [ProtoMember(2)]
        public TaskStatusEnum TaskStatus { get; set; }

        /// <summary>
        /// 任务进度
        /// </summary>
        [ProtoMember(3)]
        public int TaskNum { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [ProtoMember(4)]
        public TaskTypeEnum TaskType { get; set; }
    }
}

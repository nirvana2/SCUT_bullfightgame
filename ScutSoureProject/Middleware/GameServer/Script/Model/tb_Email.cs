using System;
using ProtoBuf;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Model;
using ZyGames.Framework.Event;

namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Entity, DbConfig.ConnData, IsExpired = false)]
    public class tb_Email : ShareEntity
    {
        public tb_Email()
            : base(AccessLevel.ReadWrite)
        { }

        /// <summary>
        /// 交易号
        /// </summary>
        [ProtoMember(1)]
        [EntityField(true)]
        public string TradeNo { get; set; }

        /// <summary>
        /// 发送者
        /// </summary>
        [ProtoMember(2)]
        [EntityField]
        public int FromUserId { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        [ProtoMember(3)]
        [EntityField]
        public int ToUserId { get; set; }

        /// <summary>
        /// 邮件类型，1交易
        /// </summary>
        [ProtoMember(4)]
        [EntityField]
        public MailTypeEnum MailType { get; set; }

        /// <summary>
        /// 交易内容值
        /// </summary>
        [ProtoMember(5)]
        [EntityField(true, ColumnDbType.LongText)]
        public CacheList<string> Content { get; set; }

        /// <summary>
        /// 状态，0没有附件，1有附件，2附件已领取
        /// </summary>
        [ProtoMember(6)]
        [EntityField]
        public int State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [ProtoMember(7)]
        [EntityField]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        [ProtoMember(8)]
        [EntityField(true, ColumnDbType.LongText)]
        public Attach Attach { get; set; }

        /// <summary>
        /// 交易内容状态，0失败，1成功
        /// </summary>
        [ProtoMember(9)]
        [EntityField]
        public int CState { get; set; }
    }

    [Serializable, ProtoContract]
    public class Attach : EntityChangeEvent
    {
        [ProtoMember(1)]
        public long Num { get; set; }
    }
}

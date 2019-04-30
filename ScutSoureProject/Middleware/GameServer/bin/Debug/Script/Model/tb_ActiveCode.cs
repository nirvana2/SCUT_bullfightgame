using System;
using ProtoBuf;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Model;

namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Entity, DbConfig.ConnData, IsExpired = false)]
    public class tb_ActiveCode : ShareEntity
    {
        public tb_ActiveCode()
            : base(AccessLevel.ReadWrite)
        { }

        /// <summary>
        /// 激活码
        /// </summary>
        [ProtoMember(1)]
        [EntityField(true, ColumnLength = 16)]
        public string Activecode { get; set; }

        /// <summary>
        /// 生成者
        /// </summary>
        [ProtoMember(2)]
        [EntityField]
        public int GenerateUserId { get; set; }

        /// <summary>
        /// 使用者
        /// </summary>
        [ProtoMember(3)]
        [EntityField]
        public int UseUserId { get; set; }

        [ProtoMember(4)]
        [EntityField]
        public DateTime CreateDate { get; set; }
    }
}

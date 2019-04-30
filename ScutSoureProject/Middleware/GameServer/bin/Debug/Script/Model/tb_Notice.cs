using System;
using System.Collections.Generic; 
using ZyGames.Framework.Model;
using ProtoBuf;
using ZyGames.Framework.Game.Context;

namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(AccessLevel.ReadOnly, strFixed.strConnectstring, "tb_notice")]
    //[EntityTable(AccessLevel.ReadOnly, CacheType.Entity, true, strFixed.strConnectstring, "tb_notice", 30, "id")]
    public  class tb_Notice : ShareEntity  // BaseEntity
    {
        public tb_Notice()
        {
        }

        [ProtoMember(1)]
        [EntityField(true)]
        public int id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [ProtoMember(2)]
        [EntityField]
        public string title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [ProtoMember(3)]
        [EntityField] 
        public string content { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [ProtoMember(4)]
        [EntityField]
        public DateTime noticetime { get; set; }

        /// <summary>
        /// 作者
        /// </summary>     
        [ProtoMember(5)]
        [EntityField]
        public string _author { get; set; }
        /// <summary>
        /// 类型 1公告，2系统消息。
        /// </summary>     
        [ProtoMember(6)]
        [EntityField]
        public string _type { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [ProtoMember(7)]
        [EntityField]
        public int isStart { get; set; }

    }


}

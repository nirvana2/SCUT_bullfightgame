using System;
using ZyGames.Framework.Model;
using ProtoBuf;
using System.Collections.Generic;

namespace GameServer.Script.Model
{

    [Serializable, ProtoContract]
    [EntityTable(CacheType.Entity, strFixed.strConnectstring, "tb_tablemoneylog")]
    public class tb_TableMoneyLog : ShareEntity
    {
        [ProtoMember(1)]
        [EntityField("id", IsKey = true)]
        public string id { get; set; }

        /// <summary>
        /// 写入暂时没用找到最大ID的方法，   用的GUID
        /// </summary>
        [ProtoMember(2)]
        [EntityField]
        public string TableRecordID { get; set; }

        /// <summary>
        /// 6位数字编号
        /// </summary>
        [ProtoMember(3)]
        [EntityField]
        public int MatchCode { get; set; }

        [ProtoMember(4)]
        [EntityField]
        public int UserID { get; set; }

        [ProtoMember(5)]
        [EntityField]
        public int _pos { get; set; }

        [ProtoMember(6)]
        [EntityField]
        public decimal AddorReduceMoney { get; set; }

        [ProtoMember(7)]
        [EntityField]
        public string _ipport { get; set; }

        /// <summary>
        /// 牌的信息
        /// </summary>
        [ProtoMember(8)]
        [EntityField(true, ColumnDbType.LongText)] //[EntityField]
        public List<int> _cardList { get; set; }

        [ProtoMember(9)]
        [EntityField]
        public int gameid { get; set; }
        /// <summary>
        /// 游戏id
        /// </summary>
        [ProtoMember(10)]
        [EntityField]
        public bool _isover { get; set; }
        /// <summary>
        /// 牌对应牛的类型对应的赔率
        /// </summary>
        [ProtoMember(11)]
        [EntityField]
        public int _bullrate { get; set; }
        /// <summary>
        /// 是否爆分，当观众了
        /// </summary>
        [ProtoMember(12)]
        [EntityField]
        public int _isWatch { get; set; }
                   
        /// <summary>
        /// 生成的一个房间消耗内的唯一编号
        /// </summary>
        [ProtoMember(13)]
        [EntityField]
        public string _guid { get; set; }

        /// <summary>
        /// 1 表示 这局赢了
        /// </summary>
        [ProtoMember(14)]
        [EntityField]
        public bool _win { get; set; }

        [ProtoMember(15)]
        [EntityField]
        public DateTime CreateDate { get; set; }

        public tb_TableMoneyLog()
        {
            id = Guid.NewGuid().ToString("N");
            CreateDate = DateTime.Now;
        }
    }

}

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Data;
using ZyGames.Framework.Model;

namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Dictionary, strFixed.strConnectstring, "tb_userrechargelog")]
    public  class tb_UserRechangeLog: BaseEntity
    {
        public tb_UserRechangeLog()
        {
            createtime = DateTime.Now;
        }
        [ProtoMember(1)]
        [EntityField(true)]
        /// <summary>
        /// 自增ID
        /// </summary>
        public int id { set; get; }
        [ProtoMember(2)]
        [EntityField]
        /// <summary>
        /// 充值会员账号id
        /// </summary>
        public int userid { set; get; }
        /// <summary>
        /// 充值数量
        /// </summary>
        [ProtoMember(3)]
        [EntityField]
        public decimal money { set; get; }
        /// <summary>
        ///  1、充值金币 2、砖石
        /// </summary>
        [EntityField]
        [ProtoMember(4)]
        public int cointype { set; get; }
        /// <summary>
        /// 充值类型  1、后台充值 2、账号转账
        /// </summary>
        [EntityField]
        [ProtoMember(5)]
        public int fromtype { set; get; }
        /// <summary>
        /// 代理会员ID
        /// </summary>
        [EntityField]
        [ProtoMember(6)]
        public int fromuserid { set; get; }
        /// <summary>
        /// 后台管理员ID
        /// </summary>
        [EntityField]
        [ProtoMember(7)]
        public int fromadminid { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        [EntityField]
        [ProtoMember(8)]
        public string remarks { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [EntityField]
        [ProtoMember(9)]
        public DateTime createtime { set; get; }
        /// <summary>
        /// 充值时玩家账户金额
        /// </summary>
        [EntityField]
        [ProtoMember(10)]
        public decimal oldmoney { set; get; }

        protected override int GetIdentityId()
        {
            return id;
        }
    }
}

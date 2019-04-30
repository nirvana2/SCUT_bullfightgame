using ProtoBuf;
using System;
using ZyGames.Framework.Model;

/// <summary>
/// Scut 缓存获取不支持，?类型
/// </summary>
namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Dictionary, strFixed.strConnectstring, "tb_user")]              //, StorageType = StorageType.ReadWriteDB
    public class tb_User :  BaseEntity
    {
        public tb_User()
        {
        }
        /// <summary>
        /// 是与SessionUser关联的ID，由SNS服务器产生的过来的 很重要，       必须放第一个位置，否则清理 REDIS后会报错
        /// </summary> 
        [ProtoMember(1)]
        [EntityField(true)]
        public int UserID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(2)]
        [EntityField]
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(3)]
        [EntityField]
        public string UserPassword { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(4)]
        [EntityField]
        public decimal UserMoney { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(5)]
        [EntityField]
        public decimal UserMaxMoney { get; set; }
       
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(6)]
        [EntityField]
        public string  LastLotinTime1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(7)]
        [EntityField]
        public string LastLotinTime2 { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(8)]
        [EntityField]
        public string RegTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(9)]
        [EntityField]
        public string wechatName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(10)]
        [EntityField]
        public string IP { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(11)]
        [EntityField]
        public string Desc { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(12)]
        [EntityField]
        public int isRobot { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        [ProtoMember(13)]
        [EntityField]
        public int Status { get; set; }
        /// <summary>
        /// 0或无表示不开启
        /// </summary> 
        [ProtoMember(14)]
        [EntityField]
        public int RobotLevel { get; set; }       

        /// <summary>
        /// 充值的钻石或房卡
        /// </summary> 
        [ProtoMember(15)]
        [EntityField]
        public float diamond { get; set; }
       
        /// <summary>
        /// 1表示是代理人员 有特殊操作 0
        /// </summary> 
        [ProtoMember(16)]
        [EntityField]
        public int isagent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(17)]
        [EntityField]
        public string wechatHeadIcon { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(18)]
        [EntityField]
        public int Sex { get; set; }
        /// <summary>
        /// 总充值金额
        /// </summary> 
        [ProtoMember(19)]
        [EntityField]
        public decimal TotalMoney { get; set; }
        /// <summary>
        /// 充值总钻石
        /// </summary>
        [ProtoMember(20)]
        [EntityField]
        public decimal totaldiamond { get; set; }
        /// <summary>
        /// 锁定时间
        /// </summary>
        [ProtoMember(21)]
        [EntityField]
        public string lockTime { get; set; }
        /// <summary>
        /// 胜率
        /// </summary>
        [ProtoMember(22)]
        [EntityField]
        public int winpercent { get; set; }
        /// 代理ID
        /// </summary>
        [ProtoMember(23)]
        [EntityField]
        public int AgentId { get; set; }

        protected override int GetIdentityId()
        {
            return UserID;
        }
    }

    public class strFixed
    {
        public const string strConnectstring= "ConnData";
    }
}


  

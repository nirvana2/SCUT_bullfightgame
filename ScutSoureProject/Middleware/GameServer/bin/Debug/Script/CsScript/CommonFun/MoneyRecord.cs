using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 一桌中一个人所有钱的记录  用于客户端结算显示 在杠与，胡的地方 添加
    /// </summary>
    public class MoneyRecord
    {
        public MoneyRecord()
        {
        }
        public MoneyRecord(int _Type, int _Position, float _Money, List<int> _TargetPosition, List<float> _GiveMoney)
        {
            Type = _Type;
            Position = _Position;
            GetMoney = _Money;
            TargetPosition = _TargetPosition;
            GiveMoney = _GiveMoney;
        }
        /// <summary>
        /// 给钱类型 1,胡钱。2，杠钱。3，服务钱。
        /// </summary>
        public int Type;
        /// <summary>
        /// 一桌中的位置 
        /// </summary>
        public int Position;
        /// <summary>
        /// 此次的钱 加了好多
        /// </summary>
        public float GetMoney;
        /// <summary>
        /// 此次出钱方
        /// </summary>
        public List<int> TargetPosition;
        /// <summary>
        /// TargetPosition 此次的钱 减了好多   总和等于GetMoney
        /// </summary>
        public List<float> GiveMoney;

    }
}

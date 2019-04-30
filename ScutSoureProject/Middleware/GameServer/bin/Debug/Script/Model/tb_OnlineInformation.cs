using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Script.Model
{
    /// <summary>
    /// 在线人数信息
    /// </summary>
    [Serializable]
    public class tb_OnlineInformation
    {
        public string ID { get; set; }
        /// <summary>
        /// 游戏类型
        /// </summary>
        public int GameType { get; set; }
        /// <summary>
        /// 在线人数
        /// </summary>
        public int OnlineCount { get; set; }
        /// <summary>
        /// 房间ID
        /// </summary>
        public int RoomId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 游戏方式（门卡,金币）
        /// </summary>
        public int GameModel { get; set; }
    }
}

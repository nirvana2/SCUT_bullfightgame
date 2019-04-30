using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Script.CsScript.Action
{
    public class t_shopbaseList
    {
        public static List<t_shopbase> mList;

        public static void LoadDataSync()
        {
            mList = ConfigLoader.LoadJsonListFile<t_shopbase>("t_shopbase.bytes");
        }
        public static t_shopbase GetDataByID(int id)
        {
            for (int i = 0; i < mList.Count; i++)
            {
                if (id == mList[i].id)
                {
                    return mList[i];
                }
            }
            ////Debug.LogError("Can not find t_floorbase by ID:" + id);
            return null;
        }
    }

    [Serializable]
    public class t_shopbase
    {
        public int id;
        public string name;
        public string desc;
        public int type;
        public int item_id;
        public int cost;
    }
}

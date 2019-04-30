using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace GameServer.Script.CsScript.Action
{

    /// <summary>
    /// 扑克牌的数据类
    /// </summary>
    public class ThreeCard
    {
        private static object obj = new object();

        /// <summary>
        /// 红  A->114
        /// </summary>
        private static int[] arrHeart = { 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114 };
        /// <summary>
        /// 黑  A->214
        /// </summary>
        private static int[] arrSpade = { 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 114 };
        /// <summary>
        /// 梅  A->314
        /// </summary>
        private static int[] arrClub = { 302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313, 114 };
        /// <summary>
        /// 方  A->414
        /// </summary>
        private static int[] arrDiamond = { 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, 114 };

        /// <summary>
        /// 一付Poker 52张
        /// </summary>
        private static Queue<int> ALLPoker;

        /// <summary>
        /// 不要大小王
        /// </summary>
        private static int mNumALLPoker = 52;
        /// <summary>
        /// 单个用户牌的个数
        /// </summary>
        private static int _NumPerUser = 3;

        /// <summary>
        /// 洗牌， 让所有纸牌，随机顺序
        /// </summary>
        private static void Shuffle()
        {
            ALLPoker = new Queue<int>();
            int[] arrPoker = new int[mNumALLPoker];
            int arrindex = 0;
            foreach (int temptong in arrHeart)
            {
                arrPoker[arrindex] = temptong;
                arrindex++;
            }
            foreach (int temptiao in arrSpade)
            {
                arrPoker[arrindex] = temptiao;
                arrindex++;
            }
            foreach (int tempwan in arrClub)
            {
                arrPoker[arrindex] = tempwan;
                arrindex++;
            }
            foreach (int tempDiamond in arrDiamond)
            {
                arrPoker[arrindex] = tempDiamond;
                arrindex++;
            }
            //随机生成排序这108张牌
            Array array = Array.CreateInstance(typeof(int), arrPoker.Length);
            for (int i = 0; i < arrPoker.Length; i++)
            {
                array.SetValue(arrPoker[i], i);
            }
            array = MixArray(array);
            for (int i = 0; i < arrPoker.Length; i++)
            {
                ALLPoker.Enqueue((int)array.GetValue(i));
            }
            if (ALLPoker.Count != mNumALLPoker)
            {
                ErrorRecord.Record("201610212116 ALLPoker.Count != mNumALLPoker 即扑克初始不正确");
            }
        }

        /// <summary>
        /// 随机打乱数组中数据顺序
        /// </summary>
        /// <param name="arr">要处理的数组</param>
        /// <param name="type">要处理的数组值类型</param>
        /// <returns></returns>
        private static Array MixArray(Array arr)
        {
            Array goal = Array.CreateInstance(typeof(int), arr.Length);
            Random rnd = new Random();//实例化一个伪随机数生成器
            for (int i = 0; i < arr.Length; i++)
            { //循环要处理的数组
                //随机生成一个数组索引号，注意每循环一次，将范围缩小一个
                int pos = rnd.Next(0, arr.Length - i - 1);
                //将随机的索引号pos所在的值 赋给输出数组中的当前循环索引（i）
                goal.SetValue(arr.GetValue(pos), i);
                //由于每次循环，范围都缩小了一个，而在该范围外的一个值，可能会丢掉了。
                //所以要将原数组pos位置的值更改为该范围外的那个值，当前位置的值已传给输出数组了
                arr.SetValue(arr.GetValue(arr.Length - 1 - i), pos);
            }
            return goal;
        }

        /// <summary>
        /// 初始化后 把纸牌分给2~6人
        /// </summary> 
        /// <returns></returns>
        public static Dictionary<int, List<int>> DistributePoker(out Queue<int> LeftCard, int userCount)
        {
            if (userCount > 6 || userCount < 2)
            {
                ErrorRecord.Record("201610212210  userCount > 6 || userCount < 2   " + userCount);
                LeftCard = null;
                return null;
            }
            Shuffle(); 
            if (ALLPoker.Count != mNumALLPoker)
            {
                ErrorRecord.Record("201208241544066 ALLPoker!=  " + mNumALLPoker);
                LeftCard = null;
                return null;
            }
          
            Dictionary<int, List<int>> retDic = new Dictionary<int, List<int>>();

            for (int j = 1; j <= userCount; j++)
            {
                retDic.Add(j, new List<int>());
            }
            for (int i = 0; i < _NumPerUser; i++)
            {
                for (int j = 1; j <= userCount; j++)
                {
                    retDic[j].Add(ALLPoker.Dequeue());
                }
            }

            LeftCard = ALLPoker;
            if (ALLPoker.Count != mNumALLPoker - _NumPerUser * userCount)
            {
                ErrorRecord.Record(" 20120824154501   分牌都分错了");
            }
            return retDic;
        }


        /// <summary>
        /// 排序 2，3，4，5，6，7，8，9 ~ 14
        /// </summary>
        /// <param name="paiarr"></param>
        /// <returns></returns>
        public static List<int> OrderPai(List<int> paiarr)
        {
            int[] temparr = paiarr.ToArray<int>();
            Array.Sort<int>(temparr);
            return temparr.ToList<int>();
        }
        /// <summary>
        /// 返回刻子的值
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static int GetThreeTypeValue(List<int> cardlist)
        {
            if (cardlist[0] == cardlist[1] && cardlist[0] == cardlist[2])
            {
                return cardlist[0];
            }
            return 0;
        }

        /// <summary>
        /// 是否为同花顺，，
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static bool IsTongHuaSun(List<int> cardlist)
        {
            return false;
        }

        /// <summary>
        ///  是否同花 金花  ===***需要带花色的牌***===
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static bool IsTongHua(List<int> cardlist)
        {
            int hua0 = cardlist[0] / 100;
            int hua1 = cardlist[1] / 100;
            int hua2 = cardlist[2] / 100;
            if (hua0 == hua1 && hua0 == hua2) return true;
            return false;
        }

        /// <summary> 
        /// 判断牌组是否为顺子 
        /// </summary> 
        /// <param name="PG">牌组</param> 
        /// <returns>是否为顺子</returns> 
        private static bool IsSunZhi(List<int> cardlist)
        {
            if (cardlist[0] - 1 == cardlist[1] && cardlist[1] - 1 == cardlist[2]) return true;
            return false;
        }
        /// <summary> 
        /// 判断牌组是否为顺子  123   14,3,2
        /// </summary> 
        /// <param name="PG">牌组</param> 
        /// <returns>是否为顺子</returns> 
        private static bool IsSunZhi123(List<int> cardlist)
        {
            if (cardlist[0] == 14 && cardlist[1] == 3 && cardlist[2] == 2) return true;
            return false;
        }
        private static int GetDoubleValue(List<int> cardlist)
        {
            if (cardlist[0] == cardlist[1])
            {
                return cardlist[0];
            }
            if (cardlist[1] == cardlist[2])
            {
                return cardlist[1];
            }
            return 0;
        }

        /// <summary>
        /// 去色，排序，从大到小，
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static List<int> QuSeOrder(List<int> cardlist)
        {
            List<int> _orderCardList = new List<int>(cardlist);
            for (int i = 0; i < _orderCardList.Count; i++)
            {
                _orderCardList[i] = _orderCardList[i] % 100;
            }
            _orderCardList = OrderPai(_orderCardList);//默认是从小到大，
            _orderCardList.Reverse();//反转成从大到小， }
            return _orderCardList;
        }
        public static PokerThreeGroupType GetThreeCardType(List<int> cardlist)
        {
            if (cardlist.Count != 3) return PokerThreeGroupType.Single;//

            List<int> _orderCardList = QuSeOrder(cardlist);

            if (GetThreeTypeValue(_orderCardList) > 0) return PokerThreeGroupType.Three;
            if (IsTongHua(cardlist) && IsSunZhi(_orderCardList)) return PokerThreeGroupType.TongHuaSun;
            if (IsTongHua(cardlist)) return PokerThreeGroupType.TongHua;
            if (IsSunZhi(_orderCardList)) return PokerThreeGroupType.SunZhi;
            if (IsSunZhi123(_orderCardList)) return PokerThreeGroupType.SunZhi123;
            if (GetDoubleValue(_orderCardList) > 0) return PokerThreeGroupType.Double;
            return PokerThreeGroupType.Single;
        }
        public static bool ComparePoker(List<int> applycard,   List<int> cardlist)
        { 
            PokerThreeGroupType _applytype = GetThreeCardType(applycard);
            PokerThreeGroupType _type = GetThreeCardType(cardlist);
            return ComparePoker(applycard, _applytype, cardlist, _type);
        }
        /// <summary>
         /// applycard 表示申请比牌的人
         /// </summary>
         /// <param name="applycard"></param>
         /// <param name="_applytype"></param>
         /// <param name="cardlist"></param>
         /// <param name="_type"></param>
         /// <returns></returns>
        private static bool ComparePoker(List<int> applycard, PokerThreeGroupType _applytype, List<int> cardlist, PokerThreeGroupType _type)
        {
            if (_applytype > _type) return true;
            else if (_applytype < _type) return false;
            else
            {//相同类型 需要根据规则再处理一次， 

                List<int> _tapplyList = QuSeOrder(applycard);

                List<int> _tempCardList = QuSeOrder(cardlist);
                switch (_applytype)
                {
                    case PokerThreeGroupType.Three://
                        if (GetThreeTypeValue(_tapplyList) > GetThreeTypeValue(_tempCardList)) return true;
                        return false;
                    case PokerThreeGroupType.TongHuaSun:
                    case PokerThreeGroupType.TongHua:
                    case PokerThreeGroupType.SunZhi:
                        return GetApplyWin(_tapplyList, _tempCardList);
                    case PokerThreeGroupType.SunZhi123:
                        return false;
                    case PokerThreeGroupType.Double:
                        if (GetDoubleValue(_tapplyList) > GetDoubleValue(_tempCardList)) return true;
                        else if (GetDoubleValue(_tapplyList) < GetDoubleValue(_tempCardList)) return false;
                        else
                        {
                            if (_tapplyList[0] == _tapplyList[1])
                            {
                                if (_tapplyList[2] > _tempCardList[2]) return true;
                                else return false;
                            }
                            else
                            {
                                if (_tapplyList[0] > _tempCardList[0]) return true;
                                else return false;
                            }
                        }
                    case PokerThreeGroupType.Single:
                        return GetApplyWin(_tapplyList, _tempCardList);
                }
            }
            return false;
        }
        /// <summary>
        /// 一样大 申请者输， 
        /// </summary>
        /// <param name="applycard">需要去色，排序，从大到小，</param>
        /// <param name="cardlist">需要去色，排序，从大到小，</param>
        /// <returns></returns>
        private static bool GetApplyWin(List<int> applycard, List<int> cardlist)
        {
            if (applycard[0] > cardlist[0]) return true;
            else if (applycard[0] < cardlist[0]) return false;
            else //if (applycard[0] == cardlist[0])
            {
                if (applycard[1] > cardlist[1]) return true;
                else if (applycard[1] < cardlist[1]) return false;
                else //if (applycard[1] == cardlist[1])
                {
                    if (applycard[2] > cardlist[2]) return true;
                    else if (applycard[2] < cardlist[2]) return false;
                    else // if (applycard[2] == cardlist[2])
                    {
                        return false;//一样大 申请者输，
                    }
                }
            }
        } 
 
       
        public enum PokerThreeGroupType
        {
            Single = 1,         //单牌
            Double = 2,         //对牌
            SunZhi123 = 3,      // 123顺子
            SunZhi = 4,         // 顺子
            TongHua = 5,        //同色 即金花
            TongHuaSun = 6,     //同花顺，顺金花
            Three = 7,          //刻子，豹子，三同一个意思 
        }
    }

}

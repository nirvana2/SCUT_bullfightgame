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
    public class BullFight
    {

        private static object obj = new object();
        /// <summary>
        /// 黑
        /// </summary>
        private static int[] arrSpade = { 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, };
        /// <summary>
        /// 红
        /// </summary>
        private static int[] arrHeart = { 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, };
        /// <summary>
        /// 梅
        /// </summary>
        private static int[] arrClub = { 301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313, };
        /// <summary>
        /// 方
        /// </summary>
        private static int[] arrDiamond = { 401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, };
        /////// <summary>
        /////// 大王15 ，小王14   有的地方区别不同，大小王算10，有的地方直接不要，
        /////// </summary>
        ////private static int[] arrKing = { 14, 15 };

        /// <summary>
        /// 一付Poker 54张
        /// </summary>
        private static Queue<int> ALLPoker;
        /// <summary>
        ///  要大小王
        /// </summary>
        private static int mNumALLPoker = 52;
        /// <summary>
        /// 单个用户牌的个数
        /// </summary>
        private static int _NumPerUser = 5;

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
            //随机生成排序这54张牌
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
                ErrorRecord.Record("201208241334102 ALLPoker.Count != 54 即扑克初始不正确");
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
        /// 初始化后 把纸牌分给三家 
        /// </summary> 
        /// <returns></returns>
        public static Dictionary<int, List<int>> DistributePoker(out Queue<int> LeftCard, int userCount)
        {
            if (userCount > 6 || userCount < 2)
            {
                ErrorRecord.Record("2016102122101  userCount > 6 || userCount < 2   " + userCount);
                LeftCard = null;
                return null;
            }
            Shuffle();
            if (ALLPoker.Count != mNumALLPoker)
            {
                ErrorRecord.Record("201208241544012 ALLPoker!= mNumALLPoker：  " + mNumALLPoker);
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
            if (ALLPoker.Count != mNumALLPoker - 5 * userCount)
            {
                ErrorRecord.Record(" 20120824154501115   分牌都分错了");
            }
            return retDic;
        }
        /// <summary>
        /// 初始化后 把纸牌分给三家 
        /// </summary> 
        /// <returns></returns>
        public static Dictionary<int, List<int>> DistributePokerCrazy(out Queue<int> LeftCard, int userCount)
        {
            if (userCount > 6 || userCount < 2)
            {
                ErrorRecord.Record("2016102122101  userCount > 6 || userCount < 2   " + userCount);
                LeftCard = null;
                return null;
            }
            Shuffle();
            //去掉2~8
            List<int> _tempno28 = new List<int>(ALLPoker);
            ALLPoker = new Queue<int>();
            foreach (int poker in _tempno28)
            {
                if ((poker % 100) >= 2 && (poker % 100) <= 8) continue;
                ALLPoker.Enqueue(poker);
            }
            if (userCount > 4)
            {
                ErrorRecord.Record("201703301717  userCount > 4   " + userCount);
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
            return retDic;
        }

        /// <summary>
        /// 根据牛牛类型传出牛牛的值
        /// </summary>
        /// <param name="_type">牛的类型</param>
        /// <returns>对应的一组牛的牌</returns>
        public static List<int> GetPokerbyBullFightType(PokerBullFightType _type)
        {
            int type = _type.GetHashCode();
            int[] allCards = new int[5];
            int[] bullcrads = new int[3];
            int[] spotcrads = new int[2];
            if (type >= 1 && type <= 10)
            {
                Random rd = new Random();
                bullcrads[0] = rd.Next(1, 11);
                bullcrads[1] = rd.Next(1, 11);
                int remainnum = (bullcrads[0] + bullcrads[1]) % 10;
                bullcrads[2] = 10 - remainnum;

                spotcrads[0] = rd.Next(1, 11);
                if (type > spotcrads[0])
                {
                    spotcrads[1] = type - spotcrads[0];
                }
                else
                {
                    spotcrads[1] = 10 + type - spotcrads[0];
                }
            }
            else if (type == 11)
            {
                bullcrads = new int[] { 10, 10, 10 };
                spotcrads = new int[] { 10, 10 };
            }
            else if (type == 0)
            {
                Random rd = new Random();
                bullcrads[0] = rd.Next(1, 11);
                bullcrads[1] = rd.Next(1, 11);
                List<int> havaNum = new List<int>();
                havaNum.Add(bullcrads[0]);
                havaNum.Add(bullcrads[1]);
                List<int> filterNum = new List<int>();
                //// 生成第三个数
                filterNum = GetFilterList(havaNum);
                bullcrads[2] = GetNullNum(filterNum);
                havaNum.Add(bullcrads[2]);
                //// 生成第四个数
                filterNum = GetFilterList(havaNum);
                spotcrads[0] = GetNullNum(filterNum);
                havaNum.Add(spotcrads[0]);
                //// 生成第五个数,注意无牛的时候可能出现五个一样的数字
                filterNum = GetFilterList(havaNum);
                if (havaNum.FindAll(p => p == havaNum[0]).Count == 4)
                {
                    filterNum.Add(havaNum[0]);
                }
                spotcrads[1] = GetNullNum(filterNum);
            }

            bullcrads.CopyTo(allCards, 0);
            spotcrads.CopyTo(allCards, bullcrads.Length);

            //// 替换10, 注意牛牛替换以后不能是五花牛，也不能出现5个10；五花牛替换以后不能出现10，也不能出现5个一样的数字
            List<int> flowerCards = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                //// 替换 10
                if (allCards[i] == 10)
                {
                    if (type < 10)
                    {  
                        allCards[i] = ToolsEx.GetRandomSys(10, 14);
                    }
                    else if (type == 10)
                    {
                        if (i == 4 && allCards.ToList().FindAll(p => p > 10).Count == 4)
                        {
                            allCards[i] = 10;
                        }
                        else
                        { 
                            allCards[i] = ToolsEx.GetRandomSys(10, 14);
                        }
                    }
                    else if (type == 11)
                    {
                        if (i == 4 && allCards.ToList().FindAll(p => p == allCards[0]).Count == 4)
                        {
                            int fivenum = allCards[0];
                            while (fivenum == allCards[0])
                            { 
                                allCards[i] = ToolsEx.GetRandomSys(11, 14);
                            }
                        }
                        else
                        { 
                            allCards[i] = ToolsEx.GetRandomSys(11, 14);
                        }
                    }
                }

                //// 替换花色 
                int num = ToolsEx.GetRandomSys(1, 5) * 100 + allCards[i];
                flowerCards[i] = num;
                while (flowerCards.Contains(num))
                {
                    num = ToolsEx.GetRandomSys(1, 5)  * 100 + allCards[i];
                }

                flowerCards[i] = num;
            }


            flowerCards = ListRandom(flowerCards);
            return flowerCards; ;
        }

        /// <summary>
        /// 随机排列数组元素
        /// </summary>
        /// <param name="myList"></param>
        /// <returns></returns>
        public static List<int> ListRandom(List<int> myList)
        { 
            int index = 0;
            int temp = 0;
            for (int i = 0; i < myList.Count; i++)
            { 
                index = ToolsEx.GetRandomSys(1, myList.Count - 1);
                if (index != i)
                {
                    temp = myList[i];
                    myList[i] = myList[index];
                    myList[index] = temp;
                }
            }
            return myList;
        }

        /// <summary>
        /// 获取一张无牛牌组的牌
        /// </summary>
        /// <param name="filterNum">不能要的数字</param>
        /// <returns>一张牌</returns>
        public static int GetNullNum(List<int> filterNum)
        {
            int result = filterNum[0];

            while (filterNum.Contains(result))
            {
                result = ToolsEx.GetRandomSys(1, 11);
            } 
            return result;
        }

        /// <summary>
        /// 获取无牛牌组需要过滤的数字
        /// </summary>
        /// <param name="havaNum">现有的数字</param>
        /// <returns>过滤的数字数组</returns>
        public static List<int> GetFilterList(List<int> havaNum)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < havaNum.Count; i++)
            {
                for (int j = i + 1; j < havaNum.Count; j++)
                {
                    int num = 10 - ((havaNum[i] + havaNum[j]) % 10);
                    if (!result.Contains(num))
                    {
                        result.Add(num);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///  对传牌按黑>红>梅>方  从小到大会反转的 1~13,   小王14, 大王15 排序 
        /// </summary>
        /// <param name="paiarr"></param>
        /// <returns></returns>         
        public static List<int> OrderPai(List<int> paiarr)
        {
            int[] temparr = paiarr.ToArray<int>();
            Array.Sort<int>(temparr);
            return temparr.ToList<int>();
        }

        #region 获取牛的类型
        /// <summary>
        /// 获取牛的类型
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        public static PokerBullFightType GetBullType(List<int> cardlist)
        {
            if (cardlist == null || cardlist.Count == 0)
            {
                ErrorRecord.Record("201704041144 cardlist.Count == 0");
                return PokerBullFightType.Bull_No;
            }
            List<int> _orderCardList = new List<int>(cardlist);
            for (int i = 0; i < _orderCardList.Count; i++)
            {
                _orderCardList[i] = _orderCardList[i] % 100;
            }

            ////if (IsSmallSmall(_orderCardList)) return PokerBullFightType.Bull_SmallSmall;
            ////if (IsBomb(_orderCardList) >1) return PokerBullFightType.Bull_Bomb;
            if (isFiveBull(_orderCardList)) return PokerBullFightType.Bull_Five;
            ////if (isFourBull(_orderCardList)) return PokerBullFightType.Bull_Four;       
            int _tt = HaveBull(_orderCardList);
            if (_tt == 0) return PokerBullFightType.Bull_Bull;
            if (_tt == -1) return PokerBullFightType.Bull_No;
            return (PokerBullFightType)_tt;
        }
        /// <summary>
        /// 牌类游戏牛牛中五张牌均小于5点，且牌点总数小余或等于10，是牛牛里最大的牌，一般为5倍赔率。
        /// 5张牌加起来，小于等于零10  最NB的牌   1，1，1，1，2，按最大的算，不算牛炸
        /// 1,1,2,2,3=9  可能会出两个，牛炸
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static bool IsSmallSmall(List<int> cardlist)
        {
            int maxvalue = 0;
            foreach (int card in cardlist)
            {
                if (card >= 5) return false;
                maxvalue += card;
            }
            if (maxvalue <= 10) return true;
            else return false;
        }
        /// <summary>
        /// 检查 是否为牛炸    且返回牛炸的值
        /// </summary>
        /// <param name="paiarr"></param>
        /// <returns></returns>
        private static int IsBomb(List<int> paiarr)
        {
            if (paiarr.Count != 5)
            {
                return 0;
            }
            int _first = paiarr[0];
            int _firstNum = 0;
            int _sec = paiarr[1];
            int _secnum = 0;
            foreach (int pai in paiarr)
            {
                if (pai == _first) _firstNum++;
                if (pai == _sec) _secnum++;
            }
            if (_firstNum == 4)
                return paiarr[0];
            else if (_secnum == 4)
                return paiarr[1];
            else
                return 0;
        }
        private static bool isFiveBull(List<int> cardlist)
        {
            int _big10 = 0;
            foreach (int card in cardlist)
            {
                if (card <= 10) return false;
                else { _big10++; }
            }
            return _big10 == 5;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static bool isFourBull(List<int> cardlist)
        {
            int _big10 = 0;
            foreach (int card in cardlist)
            {
                if (card > 10) return false;
                else { _big10++; }
            }
            return _big10 == 4;
        }
        /// <summary>
        /// 是否为牛牛    所有值 加起是10的倍数 算法错误
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static bool isBullBull(List<int> cardlist)
        {
            return false;
        }
        /// <summary>
        /// 查是否有牛 且返回牛的值
        /// http://wenku.baidu.com/link?url=VKnH9BrsQw0qHdKD8ZEgU2GoNLf5iEG26mzbg8MTunl2qjFAbGValtTyft5FOZEuF8Q0b7Az65cJv4Tt09AP7e4Ch5bN89Bxd5yEiYB5eaC
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static int HaveBull(List<int> _orderCardList)
        {
            List<int> _tempCardList = new List<int>(_orderCardList);
            for (int i = 0; i < _tempCardList.Count; i++)
            {
                if (_tempCardList[i] > 10) _tempCardList[i] = 10;
            }
            _tempCardList = OrderPai(_tempCardList);//默认是从小到大，
            _tempCardList.Reverse();//反转成从大到小，
            //------------------
            int cardall = 0;
            int big10 = 0;
            List<int> _equal10 = new List<int>();
            foreach (int card in _tempCardList)
            {
                cardall += card;
                if (card >= 10) big10++;
                if (card < 10) continue;
                _equal10.Add(card);
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    if (i == j) continue;
                    if ((cardall - _tempCardList[i] - _tempCardList[j]) % 10 == 0)
                    {
                        return (_tempCardList[i] + _tempCardList[j]) % 10;
                    }
                }
            }
            return -1;
        }

        #endregion

        #region 
        /// <summary>
        /// 获取牛的类型 3张基本牌 如果没有，随机3张
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        public static List<int> GetBullTypeHelp(List<int> cardlist)
        {
            return GetHaveBull3Card(cardlist);
        }
        /// <summary>
        /// 查是 有牛3基础牌 没牛就随机
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        private static List<int> GetHaveBull3Card(List<int> _orderCardList)
        {
            List<int> _basebull = new List<int>(_orderCardList);
            List<int> _Card100List = new List<int>(_orderCardList);
            for (int i = 0; i < _Card100List.Count; i++)
            {
                _Card100List[i] = _Card100List[i] % 100;
            }

            List<int> _tempCardList = new List<int>(_Card100List);
            for (int i = 0; i < _tempCardList.Count; i++)
            {
                if (_tempCardList[i] > 10) _tempCardList[i] = 10;
            }
            _tempCardList = OrderPai(_tempCardList);//默认是从小到大，
            _tempCardList.Reverse();//反转成从大到小，
                                    //------------------
            int cardall = 0;
            int big10 = 0;
            List<int> _equal10 = new List<int>();
            foreach (int card in _tempCardList)
            {
                cardall += card;
                if (card >= 10) big10++;
                if (card < 10) continue;
                _equal10.Add(card);
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    if (i == j) continue;
                    if ((cardall - _tempCardList[i] - _tempCardList[j]) % 10 == 0)
                    {
                        for (int tbull = 0; tbull < _basebull.Count; tbull++)
                        {
                            int _tb = _basebull[tbull] % 100;
                            if (_tb > 10) _tb = 10;
                            if (_tb == _tempCardList[i])
                            {
                                _basebull.RemoveAt(tbull); break;
                            }
                        }
                        for (int tbull = 0; tbull < _basebull.Count; tbull++)
                        {
                            int _tb = _basebull[tbull] % 100;
                            if (_tb > 10) _tb = 10;
                            if (_tb == _tempCardList[j])
                            {
                                _basebull.RemoveAt(tbull); break;
                            }
                        }
                        return _basebull;
                    }
                }
            }
            _basebull.RemoveAt(0);
            _basebull.RemoveAt(0);
            return _basebull;
        }

        #endregion
        /// <summary>
        ///   
        /// 然后再比较普通牌组的第一张牌就可以了
        /// bankercard > cardlist  返回True
        /// </summary>

        public static bool ComparePoker(List<int> bankercard, PokerBullFightType _bankeBulltype, List<int> cardlist, PokerBullFightType _bulltype)
        {

            if (_bankeBulltype > _bulltype) return true;
            else if (_bankeBulltype < _bulltype) return false;
            else
            {//相同类型 需要根据规则再处理一次，

                int _bankMax = GetMaxCard(bankercard);
                int _cmax = GetMaxCard(cardlist);
                switch (_bankeBulltype)
                {
                    case PokerBullFightType.Bull_No: //比首张大小，如是一样大，庄win，
                        if (_bankMax % 100 >= _cmax % 100) return true;
                        break;
                    case PokerBullFightType.Bull_1:
                    case PokerBullFightType.Bull_2:
                    case PokerBullFightType.Bull_3:
                    case PokerBullFightType.Bull_4:
                    case PokerBullFightType.Bull_5:
                    case PokerBullFightType.Bull_6:
                    case PokerBullFightType.Bull_7:
                    case PokerBullFightType.Bull_8:
                    case PokerBullFightType.Bull_9:
                    case PokerBullFightType.Bull_Bull:
                    //  case PokerBullFightType.Bull_Four:
                    case PokerBullFightType.Bull_Five://比首张大小，如是一样大，再比花色   
                        if (_bankMax % 100 > _cmax % 100) return true;
                        else if (_bankMax % 100 < _cmax % 100) return false;
                        else
                        {
                            if (_bankMax < _cmax) return true; //花色大小：从大到小为黑桃、红桃、梅花、方块。
                            else return false;
                        }
                        ////case PokerBullFightType.Bull_Bomb:
                        ////    if (IsBomb(bankercard) > IsBomb(cardlist)) return true;
                        ////    else return false;
                        ////case PokerBullFightType.Bull_SmallSmall:
                        ////    if (_bankMax % 100 > _cmax % 100) return true;
                        ////    else if (_bankMax % 100 < _cmax % 100) return false;
                        ////    else
                        ////    {
                        ////        if (_bankMax > _cmax) return true;
                        ////        else return false;
                        ////    }
                }
            }
            return false;
        }
        /// <summary>
        /// 最牌中最大一张，保留花色信息
        /// </summary>
        /// <param name="cardlist"></param>
        /// <returns></returns>
        public static int GetMaxCard(List<int> cardlist)
        {
            int max = 0;
            int maxCard = 0;
            for (int i = 0; i < cardlist.Count; i++)
            {
                int _tc = cardlist[i] % 100;
                if (_tc > max)
                {
                    max = _tc;
                    maxCard = cardlist[i];
                }
                else if (_tc == max)
                {
                    maxCard = cardlist[i] < maxCard ? cardlist[i] : maxCard;
                }

            }
            return maxCard;
        }
        //花色大小：从大到小为黑桃、红桃、梅花、方块。

        public static Dictionary<PokerBullFightType, int> _dicbullfightRate = new Dictionary<PokerBullFightType, int>();
        public static void InitRate()
        {
            if (_dicbullfightRate.Count >= 12) return;
            _dicbullfightRate.Add(PokerBullFightType.Bull_No, 1);
            _dicbullfightRate.Add(PokerBullFightType.Bull_1, 1);
            _dicbullfightRate.Add(PokerBullFightType.Bull_2, 1);
            _dicbullfightRate.Add(PokerBullFightType.Bull_3, 1);
            _dicbullfightRate.Add(PokerBullFightType.Bull_4, 1);
            _dicbullfightRate.Add(PokerBullFightType.Bull_5, 1);
            _dicbullfightRate.Add(PokerBullFightType.Bull_6, 1);
            _dicbullfightRate.Add(PokerBullFightType.Bull_7, 2);
            _dicbullfightRate.Add(PokerBullFightType.Bull_8, 2);
            _dicbullfightRate.Add(PokerBullFightType.Bull_9, 2);
            _dicbullfightRate.Add(PokerBullFightType.Bull_Bull, 3);
            //_dicbullfightRate.Add(PokerBullFightType.Bull_Four, 3);
            //_dicbullfightRate.Add(PokerBullFightType.Bull_Bomb, 3);
            _dicbullfightRate.Add(PokerBullFightType.Bull_Five, 3);
            //_dicbullfightRate.Add(PokerBullFightType.Bull_SmallSmall, 3);
        }
    }


    /// <summary>
    /// 牛的不同类型 需要约定比例 
    /// 牛七以下是1倍，牛八和牛九是2倍，牛牛是3倍
    /// 牛炸4倍，五花牛5倍，五小牛10倍
    /// </summary>
    public enum PokerBullFightType
    {
        /// <summary>
        /// 无牛
        /// </summary>
        Bull_No = 0,
        /// <summary>
        /// 牛1~9
        /// </summary>
        Bull_1 = 1,
        Bull_2 = 2,
        Bull_3 = 3,
        Bull_4 = 4,
        Bull_5 = 5,
        Bull_6 = 6,
        Bull_7 = 7,
        /// <summary>
        /// 牛八和牛九是2倍
        /// </summary>
        Bull_8 = 8,
        Bull_9 = 9,
        /// <summary>
        /// 牛牛是3倍
        /// </summary>
        Bull_Bull = 10,
        ////Bull_Four = 11,
        Bull_Five = 12,
        ////////Bull_Bomb = 13,
        /////// <summary>
        /////// 小小牛 五张牌均小于5点，且牌点总数小余或等于10，是牛牛里最大的牌，
        /////// </summary>
        ////Bull_SmallSmall = 14, 
    }


    public enum PokerNumBull
    {
        r1 = 101,
        r2 = 102,
        r3 = 103,
        r4 = 104,
        r5 = 105,
        r6 = 106,
        r7 = 107,
        r8 = 108,
        r9 = 109,
        r10 = 110,
        r11 = 111,
        r12 = 112,
        r13 = 113,

        b1 = 201,
        b2 = 202,
        b3 = 203,
        b4 = 204,
        b5 = 205,
        b6 = 206,
        b7 = 207,
        b8 = 208,
        b9 = 209,
        b10 = 210,
        b11 = 211,
        b12 = 212,
        b13 = 213,

        c1 = 301,
        c2 = 302,
        c3 = 303,
        c4 = 304,
        c5 = 305,
        c6 = 306,
        c7 = 307,
        c8 = 308,
        c9 = 309,
        c10 = 310,
        c11 = 311,
        c12 = 312,
        c13 = 313,

        d1 = 401,
        d2 = 402,
        d3 = 403,
        d4 = 404,
        d5 = 405,
        d6 = 406,
        d7 = 407,
        d8 = 408,
        d9 = 409,
        d10 = 410,
        d11 = 411,
        d12 = 412,
        d13 = 413,

        smallKing = 14,
        bigKing = 15
    }
}

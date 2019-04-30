using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.ComponentModel;
using ZyGames.Framework.Model;
using ZyGames.Framework.Cache.Generic;

namespace GameServer
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extension
    {
        public static string ToFormat(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return dateTime.ToString(format);
        }

        /// <summary>
        /// 求分页的总页数
        /// </summary>
        /// <param name="rowCount">总条数</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns></returns>
        public static int CountPages(this int rowCount, int pageSize)
        {
            return (rowCount + pageSize - 1) / pageSize;
        }

        #region 对List进行随机排序
        /// <summary>  
        /// 对List进行随机排序  
        /// </summary>  
        /// <param name="listT"></param>  
        /// <returns></returns>  
        public static List<T> RandomSortList<T>(this List<T> listT)
        {
            return RandomSortListImpl<T>(listT);
        }
        public static List<T> RandomSortList<T>(this List<T> listT, int nums)
        {
            for (int i = 1; i <= nums; i++)
            {
                listT = RandomSortListImpl<T>(listT);
            }
            return listT;
        }
        private static List<T> RandomSortListImpl<T>(List<T> listT)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            List<T> newList = new List<T>();
            foreach (T item in listT)
            {
                newList.Insert(random.Next(newList.Count), item);
            }
            return newList;
        }
        #endregion

        /// <summary>
        /// 使用Lambada删除List元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="match"></param>
        public static void Remove<T>(this List<T> data, Predicate<T> match)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                if (match(item) && data.Remove(item))
                {
                    break;
                }
            }
        }

        #region 随机数、概率
        /// <summary>
        /// 返回一个小于所指定最大值的非负随机整数
        /// </summary>
        /// <param name="maxValue">要生成的随机数的上限（随机数不能取该上限值）。maxValue 必须大于或等于零。</param>
        /// <returns>大于等于零且小于 maxValue 的 32 位带符号整数，即：返回值的范围通常包括零但不包括 maxValue。不过，如果 maxValue 等于零，则返回maxValue。</returns>
        public static int Next(this int maxValue)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            return rnd.Next(maxValue);
        }

        /// <summary>
        /// 返回在指定范围内的任意整数。
        /// </summary>
        /// <param name="minValue">返回的随机数的下界（随机数可取该下界值）。</param>
        /// <param name="maxValue">返回的随机数的上界（随机数可取该上界值）。maxValue 必须大于或等于 minValue。</param>
        /// <returns>一个大于等于 minValue 或小于等于 maxValue 的 32 位带符号整数</returns>
        public static int Next(this int minValue, int maxValue)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            return rnd.Next(minValue, maxValue + 1);
        }

        /// <summary>
        /// 是否命中
        /// </summary>
        /// <param name="rate">含百分比形式值</param>
        /// <returns></returns>
        public static bool IsHit(this double rate)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            return rnd.NextDouble() < rate;
        }

        /// <summary>
        /// 是否命中
        /// </summary>
        /// <param name="percent">百分比概率值</param>
        /// <returns></returns>
        public static bool IsHit(this int percent)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            return (percent * 0.01).IsHit();
        }

        #endregion
        public static string GetMD5(this string sDataIn)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }
        public static string GetMD5(this Stream sDataIn)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            md5.Initialize();
            byte[] bytes = md5.ComputeHash(sDataIn);
            md5.Clear();
            StringBuilder sb = new StringBuilder();

            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        #region ConcurrentDictionary扩展
        /// <summary>
        /// 尝试从 System.Collections.Concurrent.ConcurrentDictionary(TKey,TValue) 中移除并返回具有指定键的值(扩展)。
        /// </summary>
        /// <typeparam name="TKey">TKey</typeparam>
        /// <typeparam name="TValue">TValue</typeparam>
        /// <param name="dict">要处理的集合</param>
        /// <param name="key">要移除并返回的元素的键。</param>
        /// <param name="defaultValue">此方法返回时，value 包含从 System.Collections.Concurrent.ConcurrentDictionary(TKey,TValue)
        /// 中移除的对象；如果操作失败，则包含默认值。
        /// </param>
        /// <returns>如果成功移除了对象，则为 true；否则为 false。</returns>
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dict.TryRemove(key, out value);
        }

        /// <summary>
        /// 尝试将指定的键和值添加到 System.Collections.Concurrent.ConcurrentDictionary(TKey,TValue)中.
        /// </summary>
        /// <typeparam name="TKey">TKey</typeparam>
        /// <typeparam name="TValue">TValue</typeparam>
        /// <param name="dict">源</param>
        /// <param name="key">要添加的元素的键。</param>
        /// <param name="value">要添加的元素的值。该值对于引用类型可以是空引用（在 Visual Basic 中为 Nothing）。</param>
        /// <returns></returns>
        public static bool Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            return dict.TryAdd(key, value);
        }

        /// <summary>
        /// 尝试从 System.Collections.Concurrent.ConcurrentDictionary(TKey,TValue) 获取与指定的键关联的值(扩展)。
        /// </summary>
        /// <typeparam name="TKey">TKey</typeparam>
        /// <typeparam name="TValue">TValue</typeparam>
        /// <param name="dict">要处理的集合</param>
        /// <param name="key">要获取的值的键。</param>
        /// <param name="defaultValue">此方法返回时，value 包含 System.Collections.Concurrent.ConcurrentDictionary(TKey,TValue)
        /// 中具有指定键的对象；如果操作失败，则包含默认值。
        /// </param>
        /// <returns>如果在 System.Collections.Concurrent.ConcurrentDictionary(TKey,TValue) 中找到该键，则为TValue，否则为NULL</returns>
        public static TValue GetValue<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
        public static TValue GetValue<TKey, TValue>(this SortedDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
        #endregion

        #region string.Format扩展
        /// <summary>
        ///  将指定字符串中的格式项替换为指定数组中相应对象的字符串表示形式。(扩展)
        /// </summary>
        /// <param name="format">复合格式字符串。</param>
        /// <param name="args">一个对象数组，其中包含零个或多个要设置格式的对象。</param>
        /// <returns></returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
        /// <summary>
        ///  将指定字符串中的格式项替换为指定数组中相应对象的字符串表示形式。(扩展)
        /// </summary>
        /// <param name="format">复合格式字符串。</param>
        /// <param name="arg0">要设置格式的第一个对象。</param>
        /// <returns></returns>
        public static string FormatWith(this string format, object arg0)
        {
            return string.Format(format, arg0);
        }
        /// <summary>
        ///  将指定字符串中的格式项替换为指定数组中相应对象的字符串表示形式。(扩展)
        /// </summary>
        /// <param name="format">复合格式字符串。</param>
        /// <param name="arg0">要设置格式的第一个对象</param>
        /// <param name="arg1">要设置格式的第二个对象。</param>
        /// <returns></returns>
        public static string FormatWith(this string format, object arg0, object arg1)
        {
            return string.Format(format, arg0, arg1);
        }
        /// <summary>
        ///  将指定字符串中的格式项替换为指定数组中相应对象的字符串表示形式。(扩展)
        /// </summary>
        /// <param name="format">复合格式字符串。</param>
        /// <param name="arg0">要设置格式的第一个对象</param>
        /// <param name="arg1">要设置格式的第二个对象</param>
        /// <param name="arg2">要设置格式的第三个对象。</param>
        /// <returns></returns>
        public static string FormatWith(this string format, object arg0, object arg1, object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 替换指定位置为新字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start">开始位置</param>
        /// <param name="end">结束位置</param>
        /// <param name="newChar">新字符</param>
        /// <returns></returns>
        public static string ReplaceWith(this string s, int start, int end, char newChar)
        {
            string result = string.Empty;
            if (!(s.IsNullOrWhiteSpace()))
            {
                for (int i = 1; i <= s.Length; i++)
                {
                    if (i >= start && i <= end)
                    {
                        result += newChar;
                    }
                    else
                    {
                        result += s[i - 1];
                    }
                }
            }
            return result;
        }
        #endregion

        #region string.Join扩展
        /// <summary>
        /// 串联类型为 System.String 的 System.Collections.Generic.IEnumerable T 构造集合的成员，其中在每个成员之间使用指定的分隔符。(扩展)
        /// </summary>
        /// <param name="separator">要用作分隔符的字符串。</param>
        /// <param name="values">一个包含要串联的字符串的集合。</param>
        /// <returns>一个由 values 的成员组成的字符串，这些成员以 separator 字符串分隔。</returns>
        public static string Join(this object values, string separator)
        {
            if (values.GetType() == typeof(System.Object[]))
            {
                object[] v = (object[])values;
                return string.Join(separator, v);
            }
            else if (values.GetType() == typeof(System.String[]))
            {
                string[] v = (string[])values;
                return string.Join(separator, v);
            }
            else if (values.GetType() == typeof(System.Collections.Generic.List<string>))
            {
                List<string> v = (List<string>)values;
                return string.Join(separator, v);
            }
            return string.Join(separator, values);
        }

        public static string Join(this string separator, params string[] value)
        {
            return string.Join(separator, value);
        }

        //public static string Join(this object[] value, string splitValue)
        //{ 
        //}
        #endregion

        #region string.IsNullOrWhiteSpace扩展
        /// <summary>
        /// 指示指定的字符串是 null、空还是仅由空白字符组成(扩展)。
        /// </summary>
        /// <param name="s">要验证的字符串。</param>
        /// <returns>如果 value 参数为 null 或 System.String.Empty，或者如果 value 仅由空白字符组成，则为 true。</returns>
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }
        public static string DefaultIsNullOrWhiteSpace(this string s, string defalutValue = "")
        {
            if (!string.IsNullOrWhiteSpace(s))
                return s;
            else
                return defalutValue;
        }
        #endregion

        /// <summary>
        /// 连接指定 System.Object 数组中的元素的 System.String 表示形式，包含当前的字符串(扩展)。
        /// </summary>
        /// <param name="s">返回</param>
        /// <param name="args">一个对象数组，其中包含要连接的元素。</param>
        /// <returns>args 中元素的值经过连接的字符串表示形式。</returns>
        public static string ConcatWith(this string s, params object[] args)
        {

            return string.Concat(s, string.Concat(args));
        }

        public static string ConcatWith(this SortedDictionary<string, string> dic)
        {
            string val = string.Empty;
            foreach (var item in dic.Values)
            {
                if (item.IsNullOrWhiteSpace())
                {
                    continue;
                }
                val += item;
            }
            return val;
        }
        public static string ConcatWithSplit(this string s, string split, params object[] args)
        {
            return string.Concat(s, string.Join(split, args));
        }
        public static string ConcatWithCache(this string s, params object[] args)
        {
            return string.Concat(s.ConcatWith("_"), string.Join("_", args));
        }
        //public static string Concat(string split, params object[] args)
        //{
        //    return string.Join(split, args);
        //}
        /// <summary>
        /// 连接两个指定对象的字符串表示形式。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="arg0"></param>
        /// <returns></returns>
        public static string ConcatWith(this string s, object arg0)
        {
            return string.Concat(s, arg0);
        }

        /// <summary>
        /// 连接三个指定对象的字符串表示形式。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public static string ConcatWith(this string s, object arg0, object arg1)
        {
            return string.Concat(s, arg0, arg1);
        }
        public static string ConcatWithSplit(this string s, string split, object arg0, object arg1)
        {
            return string.Concat(s, string.Join(split, new string[] { arg0.ToString(), arg1.ToString() }));
        }
        /// <summary>
        /// 将四个指定对象的 System.String 表示形式与可选变量长度参数列表中指定的任何对象串联起来。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static string ConcatWith(this string s, object arg0, object arg1, object arg2)
        {
            return string.Concat(s, arg0, arg1, arg2);
        }
        public static string ConcatWithSplit(this string s, string split, object arg0, object arg1, object arg2)
        {
            return string.Concat(s, string.Join(split, new string[] { arg0.ToString(), arg1.ToString(), arg2.ToString() }));
            //return string.Concat(s, arg0, arg1, arg2);
        }
        #region 数据类型转换

        public static byte ToByte(this object s, byte defaultValue = default(byte))
        {
            byte value;
            if (s == null) return defaultValue;
            if (byte.TryParse(s.ToString(), out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 转成绝对值
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ToABS(this int s)
        {
            return Math.Abs(s);
        }

        /// <summary>
        /// 将一个对象转换成Int32
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <param name="defaultValue">转换失败返回的值，默认为default(int)</param>
        /// <returns></returns>
        public static int ToInt32(this object obj, int defaultValue = default(int))
        {
            if (obj == null)
            {
                return defaultValue;
            }
            return obj.ToString().ToInt32(defaultValue);
        }

        public static uint ToUInt32(this object obj, uint defaultValue = default(uint))
        {
            if (obj == null) return defaultValue;
            int value = Convert.ToInt32(obj);

            return (uint)Math.Abs(value);
        }

        /// <summary>
        /// 将一个字符串转换成Int32
        /// </summary>
        /// <param name="s">要转换的对象</param>
        /// <param name="defaultValue">转换失败返回的值，默认为default(int)</param>
        /// <returns></returns>
        public static int ToInt32(this string s, int defaultValue = default(int))
        {
            int value;
            if (s == null) return defaultValue;
            if (int.TryParse(s, out value))
            {
                return value;
            }
            return defaultValue;
        }

        public static T ToEnum<T>(this string n)
        {
            T value = default(T);
            GetEnum<T>(n, ref value);
            return value;

        }

        private static void GetEnum<T>(string aName, ref T rValue)
        {
            rValue = (T)Enum.Parse(typeof(T), aName);
        }

        /// <summary>
        /// 将一个对象转换成Int64
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <param name="defaultValue">转换失败返回的值，默认为default(long)</param>
        /// <returns></returns>
        public static long ToInt64(this object obj, long defaultValue = default(long))
        {
            if (obj == null)
            {
                return defaultValue;
            }
            long value;
            if (obj.GetType() == typeof(string))
            {
                if (long.TryParse(obj.ToString(), out value))
                {
                    return value;
                }
                return defaultValue;
            }
            else
                try
                {
                    return Convert.ToInt64(obj);
                }
                catch
                {
                    return defaultValue;
                }
        }

        public static ulong ToUInt64(this object obj, ulong defaultValue = default(ulong))
        {
            if (obj == null)
            {
                return defaultValue;
            }
            ulong value;
            if (ulong.TryParse(obj.ToString(), out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 将一个字符串转换成Int64
        /// </summary>
        /// <param name="s">要转换的对象</param>
        /// <param name="defaultValue">转换失败返回的值，默认为default(long)</param>
        /// <returns></returns>
        public static long ToInt64(this string s, long defaultValue = default(long))
        {
            long value;
            if (s == null) return defaultValue;
            if (long.TryParse(s, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static double ToDouble(this object s, double defaultValue = default(double))
        {
            double value;
            if (s == null) return defaultValue;
            if (double.TryParse(s.ToString(), out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static decimal ToDecimal(this object s, decimal defaultValue = default(decimal))
        {
            decimal value;
            if (s == null) return defaultValue;
            if (decimal.TryParse(s.ToString(), out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static ulong ToUInt64(this string s, ulong defaultValue = default(ulong))
        {
            ulong value;
            if (s == null) return defaultValue;
            if (ulong.TryParse(s, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 将一个对象转换成DateTime
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <param name="defaultValue">转换失败返回的值，默认为default(DateTime)</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object obj, DateTime defaultValue = default(DateTime))
        {
            DateTime value;
            if (obj == null) return defaultValue;
            if (DateTime.TryParse(obj.ToString(), out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 将一个对象转换成bool
        /// </summary>
        /// <param name="value">要转换的对象</param>
        /// <param name="defaultValue">转换失败返回的值，默认为default(bool)</param>
        /// <returns></returns>
        public static bool ToBool(this object value, bool defaultValue = default(bool))
        {
            if (value == null)
                return false;
            bool result;
            if (bool.TryParse(value.ToString(), out result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// 将一个字符串转换成DateTime
        /// </summary>
        /// <param name="s">要转换的对象</param>
        /// <param name="defaultValue">转换失败返回的值，默认为default(DateTime)</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string s, DateTime defaultValue = default(DateTime))
        {
            DateTime value;
            if (s == null) return defaultValue;
            if (DateTime.TryParse(s, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 将一个对象转换成字符串
        /// </summary>
        /// <param name="obj">如果对象为NULL，则返回string.Empty</param>
        /// <returns></returns>
        public static string ToStringEmpty(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return obj.ToString();
        }

        /// <summary>
        /// 将字符串转换成ASCII byte数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ToByteASCII(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public static string ToStringASCII(this byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// 将一个字符串转换成字符串
        /// </summary>
        /// <param name="s">如果对象为NULL，则返回string.Empty</param>
        /// <returns></returns>
        public static string ToStringEmpty(this string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            return s;
        }

        #endregion
        #region 时间转换
        /// <summary>
        /// 字符串时间转成毫秒
        /// </summary>
        /// <param name="nowTime"></param>
        /// <returns></returns>
        public static long ToMilliSeconds(this string nowTime)
        {
            return (long)Convert.ToDateTime(nowTime).Subtract(DateTime.Parse("00:00:00")).TotalMilliseconds;
        }

        /// <summary>
        /// TimeSpan转成毫秒
        /// </summary>
        /// <param name="timSpan"></param>
        /// <returns></returns>
        public static long ToMilliSeconds(this TimeSpan timSpan)
        {
            return (long)timSpan.TotalMilliseconds;
        }

        /// <summary>
        /// 将一个秒数转成HH:mm:ss.ms格式
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string ToDateTimeBySeconds(this int seconds)
        {
            TimeSpan t = new TimeSpan(0, 0, 0, seconds);
            return "{0}:{1}:{2}.{3}".FormatWith(new object[] { t.Hours.ToString().PadLeft(2, '0'), t.Minutes.ToString().PadLeft(2, '0'), t.Seconds.ToString().PadLeft(2, '0'), t.Milliseconds.ToString().PadLeft(3, '0') });

        }

        /// <summary>
        /// 时间转成秒
        /// </summary>
        /// <param name="nowTime"></param>
        /// <returns></returns>
        public static long ToSecond(this DateTime nowTime)
        {
            return (long)nowTime.Subtract(DateTime.Parse("00:00:00")).TotalSeconds;
        }
        #endregion

        public static bool IsBetween(this int value, int minValue, int maxValue)
        {
            return value >= minValue && value <= maxValue;

        }
        public static bool IsBetween(this DateTime value, DateTime minValue, DateTime maxValue)
        {
            return value >= minValue && value <= maxValue;
        }
        public static bool IsBetweenTime(this DateTime value, string minValue, string maxValue)
        {
            value = DateTime.Parse(value.ToShortTimeString());
            return value >= DateTime.Parse(minValue) && value <= DateTime.Parse(maxValue);
        }
        public static bool IsIn(this int value, int value1, int value2)
        {
            return value == value1 || value == value2;

        }
        public static bool IsNotIn(this string value, string[] values)
        {
            return !values.Contains(value);
        }
        public static bool IsIn(this object value, params object[] values)
        {
            foreach (var item in values)
            {
                if (item.ToString() == value.ToString())
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsIn(this string value, string value1, string value2)
        {
            return value == value1 || value == value2;

        }
        public static bool IsIn(this string value, int value1, int value2)
        {
            return value == value1.ToString() || value == value2.ToString();


        }
        public static bool NotIn(this int value, int value1, int value2, int value3)
        {
            return !(value == value1 || value == value2 || value == value3);

        }
        public static bool IsIn(this string value, params string[] values)
        {

            return values.Contains(value);
            //foreach (object v in Values)
            //    if (v.ToString() == Value)
            //        return true;
            //return false;

        }
        public static string PadLeft(this Int32 value, int len, char paddingChar)
        {
            return value.ToString().PadLeft(len, paddingChar);
        }
        public static string ToBit(this bool value)
        {
            return value ? "1" : "0";
        }

        /// <summary>
        /// 获取两个时间毫秒差
        /// </summary>
        /// <param name="nowTime">当前时间</param>
        /// <param name="cdTime">过期时间</param>
        /// <returns></returns>
        public static long ToSpanMilliSeconds(this DateTime nowTime, DateTime cdTime)
        {

            return (long)Math.Ceiling((cdTime - nowTime).TotalMilliseconds);

        }

        /// <summary>
        /// 获取两个时间秒差
        /// </summary>
        /// <param name="nowTime">当前时间</param>
        /// <param name="cdTime">过期时间</param>
        /// <returns>返回大于等于0的秒数</returns>
        public static long ToSpanSeconds(this DateTime nowTime, DateTime cdTime)
        {
            long seconds = (long)Math.Ceiling((cdTime - nowTime).TotalSeconds);
            return seconds > 0 ? seconds : 0;
        }

        /// <summary>
        /// 获取两个时间秒差的无符号值
        /// </summary>
        /// <param name="nowTime">当前时间</param>
        /// <param name="cdTime">过期时间</param>
        /// <returns>返回大于等于0的秒数</returns>
        public static long ToUSpanSeconds(this DateTime nowTime, DateTime cdTime)
        {
            if (nowTime < cdTime)
            {
                return nowTime.ToSpanSeconds(cdTime);
            }
            else
            {
                return cdTime.ToSpanSeconds(nowTime);
            }
        }

        public static IEnumerable<TSource> DistinctByEx<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        //public static string GetDescription(this Enum value)
        //{
        //    Type enumType = value.GetType();
        //    // 寻找枚举值的组合。
        //    EnumCache cache = GetEnumCache(enumType.TypeHandle);
        //    ulong valueUL = ToUInt64(value);
        //    int idx = Array.BinarySearch(cache.Values, valueUL);
        //    if (idx >= 0)
        //    {
        //        // 枚举值已定义，直接返回相应的描述。
        //        return cache.Descriptions[idx];
        //    }
        //    // 不是可组合的枚举，直接返回枚举值得字符串形式。
        //    if (!cache.HasFlagsAttribute)
        //    {
        //        return GetStringValue(enumType, valueUL);
        //    }
        //    List<string> list = new List<string>();
        //    // 从后向前寻找匹配的二进制。
        //    for (int i = cache.Values.Length - 1; i >= 0 && valueUL != 0UL; i--)
        //    {
        //        ulong enumValue = cache.Values[i];
        //        if (enumValue == 0UL)
        //        {
        //            continue;
        //        }
        //        if ((valueUL & enumValue) == enumValue)
        //        {
        //            valueUL -= enumValue;
        //            list.Add(cache.Descriptions[i]);
        //        }
        //    }
        //    list.Reverse();
        //    // 添加最后剩余的未定义值。
        //    if (list.Count == 0 || valueUL != 0UL)
        //    {
        //        list.Add(GetStringValue(enumType, valueUL));
        //    }
        //    return string.Join(", ", list);
        //}

        public static List<T> ToList<T>(this DataTable dt)
        {
            //定义集合
            List<T> ts = System.Activator.CreateInstance<List<T>>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return ts;
            }
            T t = System.Activator.CreateInstance<T>();
            string tempName = "";
            //获取此模型的公共属性
            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (DataRow row in dt.Rows)
            {
                t = System.Activator.CreateInstance<T>();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;
                    //检查DataTable是否包含此列
                    if (dt.Columns.Contains(tempName))
                    {
                        //判断此属性是否有set
                        if (!pi.CanWrite)
                            continue;
                        object value = row[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }

        public static T ToModel<T>(this DataRow dr) where T : new()
        {
            if (dr == null)
            {
                return default(T);
            }
            T t = new T();
            string tempName = "";
            //获取此模型的公共属性
            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                tempName = pi.Name;
                //检查DataTable是否包含此列
                if (dr.Table.Columns.Contains(tempName))
                {
                    //判断此属性是否有set
                    if (!pi.CanWrite)
                        continue;
                    object value = dr[tempName];
                    if (value != DBNull.Value)
                        pi.SetValue(t, value, null);
                }
            }

            return t;
        }

        /// <summary>
        /// 把一个实体填充到DataRow
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="dr"></param>
        public static void FillDataRow<T>(this T model, DataRow dr, PropertyInfo[] propertys) where T : new()
        {
            if (model == null)
            {
                return;
            }
            //获取此模型的公共属性
            //PropertyInfo[] propertys = model.GetType().GetProperties();
            foreach (var item in dr.Table.Columns)
            {
                var pi = propertys.FirstOrDefault((x) => x.CanWrite && x.Name.Equals(item.ToString(), StringComparison.InvariantCultureIgnoreCase));
                dr[item.ToString()] = pi.GetValue(model);
            }
        }

        /// <summary>
        /// 实体类转换成DataTable结构
        /// </summary>
        /// <param name="modelList">实体类列表</param>
        /// <returns></returns>
        public static DataTable FillDataTable<T>(this T model)
        {
            if (model == null)
            {
                return null;
            }
            DataTable dt = ToCreateData(model);

            DataRow dataRow = dt.NewRow();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                dataRow[propertyInfo.Name] = propertyInfo.GetValue(model, null);
            }
            dt.Rows.Add(dataRow);
            return dt;
        }

        /// <summary>
        /// 根据实体类得到表结构
        /// </summary>
        /// <param name="t">实体类</param>
        /// <returns></returns>
        public static DataTable ToCreateData<T>(this T t)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            var result = from p in t.GetType().GetProperties()
                         let attr = p.GetCustomAttributes(typeof(EntityFieldAttribute), false)
                         where attr.Length > 0
                         select p;

            foreach (PropertyInfo propertyInfo in result)
            {
                dataTable.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
            }
            return dataTable;
        }

        public static List<T> ToList<T>(this DataRow[] drs) where T : new()
        {
            //定义集合
            List<T> ts = new List<T>();
            if (drs.Length == 0)
            {
                return ts;
            }
            T t = new T();
            string tempName = "";
            //获取此模型的公共属性
            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (DataRow row in drs)
            {
                t = new T();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;
                    //检查DataTable是否包含此列
                    if (row.Table.Columns.Contains(tempName))
                    {
                        //判断此属性是否有set
                        if (!pi.CanWrite)
                            continue;
                        object value = row[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }

        /// <summary>
        /// 正则验证
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool IsMatch(this string input, string pattern)
        {
            return new Regex(pattern).IsMatch(input);
        }

        #region GUID
        /// <summary>
        /// GUID去掉分隔符
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string To32String(this Guid guid)
        {
            return guid.ToString("N");
        }

        /// <summary>
        /// 根据GUID获取16位的唯一字符串  
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string To16String(this Guid guid)
        {
            long i = 1;
            foreach (byte b in guid.ToByteArray())
                i *= ((int)b + 1);
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        /// <summary>  
        /// 根据GUID获取19位的唯一数字序列  
        /// </summary>  
        /// <returns></returns>  
        public static long ToLong(this Guid guid)
        {
            byte[] buffer = guid.ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
        #endregion

        /// <summary>
        /// 计算地球上任意两点距离
        /// </summary>
        /// <param name="long1">经度1</param>
        /// <param name="lat1">纬度1</param>
        /// <param name="long2">经度2</param>
        /// <param name="lat2">纬度2</param>
        /// <returns>返回长度单位是米</returns>
        public static double Distance(double long1, double lat1, double long2, double lat2)
        {
            double a, b, R;
            R = 6378137; //地球半径
            lat1 = lat1 * Math.PI / 180.0;
            lat2 = lat2 * Math.PI / 180.0;
            a = lat1 - lat2;
            b = (long1 - long2) * Math.PI / 180.0;
            double d;
            double sa2, sb2;
            sa2 = Math.Sin(a / 2.0);
            sb2 = Math.Sin(b / 2.0);
            d = 2 * R * Math.Asin(Math.Sqrt(sa2 * sa2 + Math.Cos(lat1) * Math.Cos(lat2) * sb2 * sb2));
            return d;
        }
        /// <summary>
        /// 转换成Base64
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Base64To(this string val)
        {
            byte[] b = Encoding.UTF8.GetBytes(val);
            return Convert.ToBase64String(b);
        }
        /// <summary>
        /// 从Base64 返回
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Base64From(this string val)
        {
            byte[] c = Convert.FromBase64String(val);
            return Encoding.UTF8.GetString(c);
        }

        /// <summary>
        /// 将枚举转成List集合
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<AppEnmu> EnumToList(Type t)
        {
            List<AppEnmu> arr = new List<AppEnmu>();
            foreach (var value in Enum.GetValues(t))
            {
                AppEnmu el = new AppEnmu() { Value = value.ToInt32(), Name = value.ToString() };
                arr.Add(el);
                object[] objAttrs = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (objAttrs != null &&
                    objAttrs.Length > 0)
                {
                    DescriptionAttribute descAttr = objAttrs[0] as DescriptionAttribute;

                    el.DescAttr = descAttr.Description;
                }
            }
            return arr;
        }
        #region 进制转
        /// <summary> 
        /// 字符串转16进制字节数组 
        /// </summary> 
        /// <param name="hexString"></param> 
        /// <returns></returns> 
        public static byte[] ToHexByte(this string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
        public static string ToHexStr(this byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        /// <summary> 
        /// 从汉字转换到16进制 
        /// </summary> 
        /// <param name="s"></param> 
        /// <param name="enc">编码,如"utf-8","gb2312"</param> 
        /// <param name="isSplit">是否每字符用逗号分隔</param> 
        /// <returns></returns> 
        public static string ToHex(this string s, Encoding enc, bool isSplit)
        {
            if ((s.Length % 2) != 0)
            {
                s += " ";//空格 
                         //throw new ArgumentException("s is not valid chinese string!"); 
            }
            System.Text.Encoding chs = enc;
            byte[] bytes = chs.GetBytes(s);
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format("{0:X}", bytes[i]);
                if (isSplit && (i != bytes.Length - 1))
                {
                    str += string.Format("{0}", ",");
                }
            }
            return str.ToLower();
        }

        ///<summary> 
        /// 从16进制转换成汉字 
        /// </summary> 
        /// <param name="hex"></param> 
        /// <param name="enc">编码,如"utf-8","gb2312"</param> 
        /// <returns></returns> 
        public static string UnHex(this string hex, Encoding enc)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            hex = hex.Replace(",", "");
            hex = hex.Replace("\n", "");
            hex = hex.Replace("\\", "");
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                hex += "20";//空格 
            }
            // 需要将 hex 转换成 byte 数组。 
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。 
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message. 
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }
            System.Text.Encoding chs = enc;
            return chs.GetString(bytes);
        }
        #endregion

        public static string GetMD5HashFromFile(this Stream stream)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] retVal = md5.ComputeHash(stream);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
                /*
                ToString("X2") 为C#中的字符串格式控制符

                X为     十六进制 
                2为     每次都是两位数

                比如   0x0A ，若没有2,就只会输出0xA 
                假设有两个数10和26，正常情况十六进制显示0xA、0x1A，这样看起来不整齐，为了好看，可以指定"X2"，这样显示出来就是：0x0A、0x1A。 
                  */
            }
            return sb.ToString();
        }
    }
    [Serializable]
    public class AppEnmu
    {
        public string DescAttr { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}

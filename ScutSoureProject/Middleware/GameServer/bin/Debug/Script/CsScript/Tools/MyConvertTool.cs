using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GameServer.Script.CsScript.Action
{
    public  class MyConvertTool
    {
        public static string GetstrFromBase64(string base64)
        {
            byte[] base64byte = Convert.FromBase64String(base64);
            string json = Encoding.UTF8.GetString(base64byte);
            return json;
        }
        public static string GetBase64Fromstr(string json)
        {
            byte[] base64byte = Encoding.UTF8.GetBytes(json);
            string base64 = Convert.ToBase64String(base64byte);
            return base64;
        }
         
        /// <summary>
        /// 整形数据转换成JSON write by jsw 201206081655
        /// 本来可以用序列化与反序列化的功能 的， 但与Flash交互 具体 不知道 Flash端 的序列化与反序列相不相同 
        /// 所以自己定义格式
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static string IntArrtoStr(int[] arr)
        {
            StringBuilder Json = new StringBuilder(); 
            
            for (int j = 0; j < arr.Length; j++)
            {
                Json.Append(arr[j]);
                if(j < arr.Length - 1)
                    Json.Append("|");                
            }
            return Json.ToString();
        }    

        /// <summary>
        /// json转成List    集合中不能出现 ","与"|"
        /// List<MyDic>
        /// </summary>
        /// <param name="strJson"></param>
        /// <returns>返回字典集合好使用些</returns>
        public static Dictionary<string, string> JsonToDic(string strJson)
        {
            Dictionary<string, string> dicList = new Dictionary<string, string>();            
            if (strJson.IndexOf("[{") != -1 && strJson.IndexOf("}]") != -1)
            {
                string strContent = strJson.Substring(2, strJson.Length - 4);
                //再去掉二级的JSON格式， 只有机器人调用， 暂时忽略二级的格式
                while(strContent.IndexOf("[{") != -1)
                { 
                    int startIndex = strContent.IndexOf("[{");
                    int endIndex = strContent.IndexOf("}]");
                    string strTemp = strContent.Substring(startIndex, endIndex - startIndex + 2);//2 表示  “}]”的长度
                    strContent = strContent.Replace(strTemp, "");
                }
                string[] strArr = strContent.Split(',');
                foreach (string strTemp in strArr)
                {
                    string strTemp02 = strTemp.Replace("\"", "");
                    string[] strArr02 = strTemp02.Split(':');
                    if (strArr02.Length == 2)
                    {
                        if (!dicList.ContainsKey(strArr02[0]))
                        {
                            dicList[strArr02[0]] = strArr02[1];
                        }
                        else
                        {
                            ErrorRecord.Record(" 201206071114 发现重复：" + strArr02[0] );
                        }
                    }
                }
            }
            return dicList;
        } 
       
        /// <summary> 
        /// 对象转换为Json字符串 
        /// </summary> 
        /// <param name="jsonObject">对象</param> 
        /// <returns>Json字符串</returns> 
        public static string ToJson(object jsonObject)
        {
            string jsonString = "{";
            PropertyInfo[] propertyInfo = jsonObject.GetType().GetProperties();
            for (int i = 0; i < propertyInfo.Length; i++)
            {
                object objectValue = propertyInfo[i].GetGetMethod().Invoke(jsonObject, null);
                string value = string.Empty;
                if (objectValue is DateTime || objectValue is Guid || objectValue is TimeSpan)
                {
                    value = "'" + objectValue.ToString() + "'";
                }
                else if (objectValue is string)
                {
                    value = "'" + ToJson(objectValue.ToString()) + "'";
                }
                else if (objectValue is IEnumerable<object>)
                {
                    value = ToJson((IEnumerable<object>)objectValue);
                }
                else
                {
                    value = ToJson(objectValue.ToString());
                }
                jsonString += "\"" + ToJson(propertyInfo[i].Name) + "\":" + value + ",";
            }
            jsonString.Remove(jsonString.Length - 1, jsonString.Length);
            return jsonString + "}";
        }
       

        /// <summary> 
        /// 对象集合转换Json 
        /// </summary> 
        /// <param name="array">集合对象</param> 
        /// <returns>Json字符串</returns> 
        public static string ToJson(IEnumerable<object> array)
        {
            string jsonString = "[";
            foreach (object item in array)
            {
                jsonString += ToJson(item) + ",";
            }
            jsonString.Remove(jsonString.Length - 1, jsonString.Length);
            return jsonString + "]";
        }

        /// <summary>
        /// 过滤特殊字符
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string String2Json(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }      
    }
}
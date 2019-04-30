using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GameServer
{
    /// <summary>
    /// 序列化
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Json序列化
        /// </summary>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            //var jsonSet = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string json = JsonConvert.SerializeObject(obj);//, Formatting.None, jsonSet
            return json;
        }

        /// <summary>
        /// Jason反序列化
        /// </summary>
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            T instance = JsonConvert.DeserializeObject<T>(json);
            return instance;
        }

        public static List<T> FromJsonList<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ArraySegment<byte> SerializeString(string value)
        {
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes((string)value));
        }

        private static ConcurrentDictionary<Type, XmlSerializer> _cache;
        private static XmlSerializerNamespaces _defaultNamespace;

        static SerializationHelper()
        {
            _defaultNamespace = new XmlSerializerNamespaces();
            _defaultNamespace.Add(string.Empty, string.Empty);

            _cache = new ConcurrentDictionary<Type, XmlSerializer>();
        }
        private static XmlSerializer GetSerializer<T>()
        {
            var type = typeof(T);
            return _cache.GetOrAdd(type, XmlSerializer.FromTypes(new[] { type }).FirstOrDefault());
        }

        public static string XmlSerialize2<T>(this T obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                GetSerializer<T>().Serialize(memoryStream, obj, _defaultNamespace);
                return Encoding.UTF8.GetString(memoryStream.GetBuffer());
            }
        }

        public static T XmlDeserialize2<T>(this string xml)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                var obj = GetSerializer<T>().Deserialize(memoryStream);
                return obj == null ? default(T) : (T)obj;
            }
        }

        /// <summary>
        /// XML序列化成字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string XmlSerialiaze<T>(T obj)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));

            StringBuilder sbr = new StringBuilder();

            using (TextWriter wr = new StringWriter(sbr))
            {
                xs.Serialize(wr, obj);
                wr.Flush();
                wr.Close();
            }

            return sbr.ToString();
        }
        /// <summary>
        /// XML序列化成字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string XmlSerialiaze<T>(T obj, bool Indent, string NewLineChars)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.Encoding = new UTF8Encoding(false);
            setting.Indent = Indent;
            setting.NewLineChars = NewLineChars;
            using (XmlWriter writer = XmlWriter.Create(stream, setting))
            {
                xs.Serialize(writer, obj);
            }
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        /// <summary>
        /// XML反序列化路径
        /// </summary>
        /// <param name="xmlStr">XML序列化的字符串</param>
        /// <returns>反序列化的对象</returns>
        public static T XmlDeserialize<T>(string xmlStr)
        {
            using (TextReader reader = new StringReader(xmlStr))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    return (T)xs.Deserialize(reader);
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// 序列化到二进制文件
        /// </summary>
        /// <returns></returns>
        public static void ToBinFile(object obj, string fileName)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, obj);
            stream.Close();
        }

        /// <summary>
        /// 从二进制文件反序列化
        /// </summary>
        public static T FromBinFile<T>(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            T obj = (T)formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }
        public static string json2XML(String json)
        {
            return JsonConvert.DeserializeXmlNode(json).OuterXml;
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(IEnumerable<T> list) where T : new()
        {
            T t = new T();
            DataTable result = new DataTable();
            //if (list.Count() > 0)
            //{
            PropertyInfo[] propertys = t.GetType().GetProperties() ;
            //foreach (var item in list)
            //{
            //    propertys = item.GetType().GetProperties();
            //    break;
            //}
            foreach (PropertyInfo pi in propertys)
            {
                Type colType = pi.PropertyType;
                if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    colType = colType.GetGenericArguments()[0];
                
                result.Columns.Add(pi.Name, colType);
            }

            foreach (var item in list)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(item, null);
                        tempList.Add(obj);

                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            //}
            return result;
        }
        public static DataTable ToDataTable<T>(Type type, IEnumerable<T> list)
        {
            DataTable result = new DataTable();
            //if (list.Count() > 0)
            //{
            PropertyInfo[] propertys = type.GetProperties();
          
            foreach (PropertyInfo pi in propertys)
            {
                result.Columns.Add(pi.Name, pi.PropertyType);
            }

            foreach (var item in list)
            {
                ArrayList tempList = new ArrayList();
                foreach (PropertyInfo pi in propertys)
                {
                    object obj = pi.GetValue(item, null);
                    tempList.Add(obj);

                }
                object[] array = tempList.ToArray();
                result.LoadDataRow(array, true);
            }
            //}
            return result;
        }
        public static string ConvertDataTableToXML(DataTable xmlDS)
        {
            MemoryStream stream = null;
            XmlTextWriter writer = null;
            try
            {
                stream = new MemoryStream();
                writer = new XmlTextWriter(stream, Encoding.Default);
                xmlDS.WriteXml(writer);
                int count = (int)stream.Length;
                byte[] arr = new byte[count];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(arr, 0, count);
                UTF8Encoding utf = new UTF8Encoding();
                return utf.GetString(arr).Trim();
            }
            catch
            {
                return String.Empty;
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }
        public static DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch (Exception ex)
            {
                string strTest = ex.Message;
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        //public static List<T> ToList<T>(DataTable dt) where T : new()
        //{
        //    //定义集合
        //    List<T> ts = new List<T>();
        //    T t = new T();
        //    string tempName = "";
        //    //获取此模型的公共属性
        //    PropertyInfo[] propertys = t.GetType().GetProperties();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        t = new T();
        //        foreach (PropertyInfo pi in propertys)
        //        {
        //            tempName = pi.Name;
        //            //检查DataTable是否包含此列
        //            if (dt.Columns.Contains(tempName))
        //            {
        //                //判断此属性是否有set
        //                if (!pi.CanWrite)
        //                    continue;
        //                object value = row[tempName];
        //                if (value != DBNull.Value)
        //                    pi.SetValue(t, value, null);
        //            }
        //        }
        //        ts.Add(t);
        //    }
        //    return ts;
        //}

        public static T ToModel<T>(DataRow dr) where T : new()
        {
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
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
//using JC = Newtonsoft.Json.JsonConvert;

//namespace Vst
//{
//    public class Json
//    {
//        public static T Convert<T>(object value)
//        {
//            return GetObject<T>(GetString(value));
//        }

//        public static string GetString(object value)
//        {
//            return JC.SerializeObject(value);
//        }
//        public static object GetObject(string text)
//        {
//            return JC.DeserializeObject(text);
//        }
//        public static T GetObject<T>(string text)
//        {
//            return JC.DeserializeObject<T>(text);
//        }

//        public static void Save(string fileName, object value)
//        {
//            using (var sw = new System.IO.StreamWriter(fileName))
//            {
//                sw.Write(JC.SerializeObject(value));
//            }
//        }
//        public static T Read<T>(string fileName)
//        {
//            using (var sr = new System.IO.StreamReader(fileName))
//            {
//                return JC.DeserializeObject<T>(sr.ReadToEnd());
//            }
//        }
//    }
//}

using JC = System.Text.Json.JsonSerializer;

namespace Vst
{
    public class Json
    {
        public static T Convert<T>(object value)
        {
            return GetObject<T>(GetString(value));
        }

        public static string GetString(object value)
        {
            return JC.Serialize(value);
        }
        public static object GetObject(string text)
        {
            return GetObject<object>(text);
        }
        public static T GetObject<T>(string text)
        {
            return JC.Deserialize<T>(text);
        }

        public static void Save(string fileName, object value)
        {
            using (var sw = new System.IO.StreamWriter(fileName))
            {
                sw.Write(GetString(value));
            }
        }
        public static T Read<T>(string fileName)
        {
            using (var sr = new System.IO.StreamReader(fileName))
            {
                return GetObject<T>(sr.ReadToEnd());
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Vst
{
    public class Bson
    {
        public static void Write(FileStream stream, object value)
        {

            //using (var bw = new Newtonsoft.Json.Bson.BsonWriter(stream))
            //{
            //    var serializer = new Newtonsoft.Json.JsonSerializer();
            //    serializer.Serialize(bw, value);
            //}
            var bytes = Encoding.String2Bytes(Json.GetString(value));
            stream.Write(bytes, 0, bytes.Length);
        }
        public static T Read<T>(FileStream stream)
        {
            //using (var br = new Newtonsoft.Json.Bson.BsonReader(stream))
            //{
            //    var serializer = new Newtonsoft.Json.JsonSerializer();
            //    return serializer.Deserialize<T>(br);
            //}
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            var s = Encoding.Bytes2String(bytes);

            return Json.GetObject<T>(s);
        }
    }
}

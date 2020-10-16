using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace Xamarin
{
    public class BindingInfo
    {
        public string BindingName { get; set; }
        public string Caption { get; set; }
        public string Input { get; set; }
        public bool AllowNull { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public string FormatString { get; set; }
        public int Width { get; set; }
    }

    public class BindingInfoCollection : Dictionary<string, BindingInfo>
    {
        static Dictionary<string, BindingInfoCollection> _temps;
        public static void Load(string filename)
        {
            _temps = Vst.Json.Read<Dictionary<string, BindingInfoCollection>>(filename);
        }
        public static implicit operator BindingInfoCollection(string name)
        {
            return _temps[name];
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Network
{
    public class Response
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Value { get; set; }

        public T GetObject<T>()
        {
            if (Value == null) return default(T);
            return (T)Value;
        }
    }
}

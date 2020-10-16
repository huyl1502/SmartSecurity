using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsonData
{
    public interface IDocument
    {
        string Id { get; set; }
    }
    public interface IProperty<T>
    {
        T this[string key] { get; set; }
    }

    public interface IBindingDocument
    {
        string ObjectId { get; set; }
    }
    public class BaseDocument<T> : Dictionary<string, T>, IProperty<T>
    {
        new public virtual T this[string key]
        {
            get
            {
                T v;
                TryGetValue(key, out v);
                return v;
            }
            set
            {
                if (ContainsKey(key))
                    base[key] = value;
                else
                    Add(key, value);
            }
        }
        public BaseDocument() { }
        public virtual BaseDocument<T> Delete(string key)
        {
            if (base.ContainsKey(key))
                base.Remove(key);
            return this;
        }
    }
    public class ObjectDocument : BaseDocument<object>
    {
        public ObjectDocument() { }
        public ObjectDocument(object src)
        {
            var props = src.GetType().GetProperties();
            foreach (var p in props)
            {
                if (p.CanWrite)
                {
                    var v = p.GetValue(src);
                    if (v != null && !v.Equals(string.Empty))
                    {
                        base[p.Name] = v;
                    }
                }
            }
        }
        public virtual T GetObject<T>(string key)
            where T : new()
        {
            object v;
            if (base.TryGetValue(key, out v) == false)
                return new T();

            return (T)v;
        }
    }
    public class StringDocument : BaseDocument<string>
    {
        public virtual T GetObject<T>(string key)
            where T : new()
        {
            string v;
            if (base.TryGetValue(key, out v) == false)
                return new T();

            return Vst.Json.GetObject<T>(v);
        }
        public virtual StringDocument SetObject(string key, object value)
        {
            this[key] = Vst.Json.GetString(value);
            return this;
        }
    }

    public class Document : IDocument, IProperty<object>
    {
        public string Id { get; set; }
        public virtual object this[string key]
        {
            get
            {
                return GetType().GetProperty(key)?.GetValue(this);
            }
            set
            {
                GetType().GetProperty(key)?.SetValue(this, value);
            }
        }
        public Document Clone()
        {
            return (Document)this.MemberwiseClone();
        }
        public virtual string GetUniqueId()
        {
            return Id;
        }
    }
    public class DocumentInfo
    {
        public string Id { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastModifyTime { get; set; }
        public long SizeOnDisk { get; set; }
    }
}

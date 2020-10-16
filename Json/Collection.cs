using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Vst;

namespace BsonData
{
    public class Collection
    {

        #region FILE MANAGER

        FileManager _fileManager;
        private class FileManager
        {
            System.IO.DirectoryInfo _folder;
            public FileManager(string path)
            {
                _folder = new System.IO.DirectoryInfo(path);
                if (_folder.Exists == false)
                    _folder.Create();
            }

            public string GetFileName(object id)
            {
                return _folder.FullName + '/' + id;
            }

            public System.IO.FileInfo GetFileInfo(object id)
            {
                return new System.IO.FileInfo(this.GetFileName(id));
            }
            public T ReadCore<T>(System.IO.FileInfo fi)
            {
                using (var s = fi.OpenRead())
                {
                    return Vst.Bson.Read<T>(s);
                }
            }

            public void WriteCore(object id, object value)
            {
                using (var s = this.GetFileInfo(id).Create())
                {
                    Vst.Bson.Write(s, value);
                }
            }

            public System.IO.FileInfo[] GetAllDocument()
            {
                return _folder.GetFiles();
            }
        }
        public List<DocumentInfo> GetDocumentInfo()
        {
            var lst = new List<DocumentInfo>();
            foreach (var fi in _fileManager.GetAllDocument())
            {
                var info = new DocumentInfo
                {
                    Id = fi.Name,
                    CreationTime = fi.CreationTime,
                    LastModifyTime = fi.CreationTime,
                    SizeOnDisk = fi.Length,
                };
                lst.Add(info);
            }
            return lst;
        }
        public DateTime GetLastUpdate(object id)
        {
            var fi = _fileManager.GetFileInfo(id);
            return fi.LastWriteTime;
        }

        public List<string> GetObjectIdList()
        {
            var lst = new List<string>();
            foreach (var fi in _fileManager.GetAllDocument())
            {
                lst.Add(fi.Name);
            }
            return lst;
        }
        #endregion

        public string Name { get; private set; }
        public DataBase DataBase { get; private set; }
        public Collection(string name, DataBase dataBase)
        {
            Name = name;
            DataBase = dataBase;
            _fileManager = new FileManager(dataBase.PhysicalPath + '/' + name);
        }
        protected virtual void GenerateItemCore(string id, object item)
        {
            ((IDocument)item).Id = id;
        }
        public virtual T FindById<T>(string id)
        {
            var fi = _fileManager.GetFileInfo(id);
            if (fi.Exists)
            {
                var item = _fileManager.ReadCore<T>(fi);
                if (item != null)
                {
                    GenerateItemCore(id, item);
                }

                return item;
            }
            return default(T);
        }
        public virtual void FindAndUpdate<T>(string id, Action<T> preAction)
        {
            FindAndUpdate<T>(id, x => {
                preAction?.Invoke(x);
                return true;
            });
        }
        public virtual void FindAndUpdate<T>(string id, Func<T, bool> canUpdate)
        {
            T item = this.FindById<T>(id);
            if (item == null)
            {
                return;
            }
            if (canUpdate(item))
            {
                this.Update(id, item);
            }
        }
        public virtual void FindAndDelete<T>(string id, Action<T> preAction)
        {
            FindAndDelete<T>(id, x => {
                preAction?.Invoke(x);
                return true;
            });
        }
        public virtual void FindAndDelete<T>(string id, Func<T, bool> canDelete)
        {
            if (!string.IsNullOrEmpty(id))
            {
                T item = FindById<T>(id);
                if (item == null)
                {
                    return;
                }
                if (canDelete(item))
                {
                    Delete(id);
                }
            }
        }
        public virtual bool Contains(string id)
        {
            return _fileManager.GetFileInfo(id).Exists;
        }
        public virtual void Insert(object item)
        {
            Insert(new ObjectId().ToString(), item);
        }
        public virtual void Insert(string id, object item)
        {
            _fileManager.WriteCore(id, item);
        }

        public void Update(object item)
        {
            Update(((IDocument)item).Id, item);
        }
        public virtual void Update(string id, object item)
        {
            _fileManager.WriteCore(id, item);
        }
        public void Update(UpdateRequest request)
        {
            var id = request.ObjectId;
            if (id == null)
            {
                if (request.Value == null) return;

                request.Action = UpdateActions.Insert;
                request.ObjectId = id = new ObjectId();
            }
            switch (request.Action)
            {
                case UpdateActions.Delete:
                    Delete(id);
                    return;

                case UpdateActions.Update:
                    Update(id, request.Value);
                    return;

                default:
                    Insert(id, request.Value);
                    return;
            }
        }

        public virtual void Delete(string id)
        {
            var fi = _fileManager.GetFileInfo(id);
            if (fi.Exists)
                fi.Delete();
        }
        public virtual IEnumerable<T> ToList<T>()
        {
            var lst = new List<T>();
            foreach (var fi in _fileManager.GetAllDocument())
            {
                var item = _fileManager.ReadCore<T>(fi);
                GenerateItemCore(fi.Name, item);

                lst.Add(item);
            }
            return lst;
        }
        public Dictionary<string, T> ToDictionary<T>(Func<string, T, bool> condition)
        {
            var map = new Dictionary<string, T>();
            foreach (var fi in _fileManager.GetAllDocument())
            {
                var item = _fileManager.ReadCore<T>(fi);
                if (condition(fi.Name, item))
                    map.Add(fi.Name, item);
            }
            return map;
        }
        public Dictionary<string, T> ToDictionary<T>()
        {
            return ToDictionary<T>((s, i) => {
                GenerateItemCore(s, i);
                return true;
            });
        }
        public Dictionary<object, T> ToDictionary<T>(string keyName)
        {
            var p = typeof(T).GetProperty(keyName);
            var map = new Dictionary<object, T>();
            foreach (T item in ToList<T>())
            {
                map.Add(p.GetValue(item), item);
            }
            return map;
        }
        public DataBase ToDatabase()
        {
            return new DataBase(this.DataBase.ConnectionString, this.Name);
        }
        public virtual IEnumerable<T> Select<T>(Func<T, bool> where)
        {
            var lst = (List<T>)this.ToList<T>();
            return lst.Where(where);
        }
        public virtual IList ObjectList<T>() where T : IBindingDocument
        {
            var lst = new List<T>();
            foreach (var fi in _fileManager.GetAllDocument())
            {
                var item = _fileManager.ReadCore<T>(fi);
                item.ObjectId = fi.Name;
                lst.Add(item);
            }
            return lst;
        }
        public int Count()
        {
            return _fileManager.GetAllDocument().Length;
        }
        public int Count<T>(Func<T,bool> func)
        {
            int count = 0;
            foreach (T e in ToList<T>())
            {
                if (func(e)) ++count;
            }
            return count;
        }
        public void Clear()
        {
            foreach (var e in _fileManager.GetAllDocument())
                e.Delete();
        }
    }

    public class Collection<T> : Collection
    {
        public Collection(string name, DataBase dataBase)
            : base(name, dataBase)
        {

        }
        public Collection(DataBase dataBase)
            : base(typeof(T).Name, dataBase)
        {

        }

        public T FindById(string id)
        {
            return base.FindById<T>(id);
        }
        public List<T> ToList()
        {
            return (List<T>)base.ToList<T>();
        }
        public IEnumerable Select(Func<T, bool> where)
        {
            return base.Select<T>(where);
        }
        public string Insert(T a)
        {
            var id = ((IDocument)a).Id ?? new ObjectId().ToString();
            base.Insert(id, a);

            return id;
        }
    }

    //public class Collection<T> : Collection
    //    where T : Document, new()
    //{

    //    class ItemMap
    //    {
    //        Dictionary<string, T> _dic;
    //        public Dictionary<string, T> GetDictionary(Collection collection)
    //        {
    //            //if (_dic == null)
    //            //{
    //            //    _dic = new Dictionary<string, T>();
    //            //    foreach (var fi in fileManager.GetAllDocument())
    //            //    {
    //            //        var item = fileManager.ReadCore(fi);
    //            //        _dic.Add(fi.Name.ToLower(), item);
    //            //    }
    //            //}
    //            return _dic;
    //        }
    //        public ItemMap()
    //        {
    //        }
    //        public bool Contains(string key)
    //        {
    //            return _dic != null && _dic.ContainsKey(key.ToLower());
    //        }
    //        public void Remove(string key)
    //        {
    //            if (_dic == null) return;
    //            _dic.Remove(key.ToLower());
    //        }
    //        public void Update(string key, T value)
    //        {
    //            if (_dic == null) return;
    //            key = key.ToLower();
    //            if (_dic.ContainsKey(key))
    //            {
    //                _dic[key] = value;
    //            }
    //            else
    //            {
    //                _dic.Add(key, value);
    //            }
    //        }
    //        public T Get(string key)
    //        {
    //            T a;
    //            _dic.TryGetValue(key.ToLower(), out a);
    //            return a;
    //        }
    //        public T Find(string key)
    //        {
    //            if (_dic == null) return null;
    //            return Get(key);
    //        }
    //    }


    //    ItemMap _itemMap;

    //    public Collection(DataBase dataBase)
    //        : this(typeof(T).Name, dataBase)
    //    {

    //    }
    //    public Collection(string name, DataBase dataBase)
    //        : base(name, dataBase)
    //    {
    //        _itemMap = new ItemMap();
    //    }

    //    public Dictionary<string, T> ToDictionary()
    //    {
    //        return _itemMap.GetDictionary(this);
    //    }

    //    public IList ToList()
    //    {
    //        this.ToDictionary();

    //        var lst = new List<T>();
    //        foreach (var p in this.ToDictionary())
    //        {
    //            lst.Add(p.Value);
    //            p.Value.Id = p.Key;
    //        }

    //        return lst;
    //    }
    //    public IList ToList(IEnumerable id)
    //    {
    //        var lst = new List<T>();
    //        this.ToDictionary();
    //        foreach (string i in id)
    //        {
    //            T src = _itemMap.Get(i);
    //            if (src != null)
    //            {
    //                lst.Add(src);
    //            }
    //        }
    //        return lst;
    //    }

    //    public int Count(Func<T, bool> func)
    //    {
    //        return base.Count<T>(func);
    //    }

    //    #region Convert
    //    public IEnumerable<TResult> GetRelation<TResult>(IEnumerable<TResult> elements, Action<T, TResult> action)
    //        where TResult: IDocument
    //    {
    //        foreach (TResult e in elements)
    //        {
    //            var s = FindById(e.Id);
    //            if (s != null)
    //                action(s, e);
    //        }
    //        return elements;
    //    }

    //    PropertyInfo[] _props;
    //    List<PropertyInfo> _getClonePropertiesCore(IEnumerable<PropertyInfo> src, IEnumerable<PropertyInfo> dst)
    //    {
    //        var map = new Dictionary<string, PropertyInfo>();
    //        foreach (var p in src)
    //        {
    //            if (p.CanRead)
    //            {
    //                map.Add(p.Name, p);
    //            }
    //        }

    //        var lst = new List<PropertyInfo>();
    //        foreach (var p in dst)
    //        {
    //            if (p.CanWrite && map.ContainsKey(p.Name))
    //            {
    //                lst.Add(map[p.Name]);
    //            }
    //        }
    //        return lst;
    //    }
    //    List<PropertyInfo> _getCloneProperties(Type type)
    //    {
    //        if (_props == null)
    //        {
    //            _props = typeof(T).GetProperties();
    //        }
    //        return _getClonePropertiesCore(_props, type.GetProperties());
    //    }
    //    List<PropertyInfo> _getCopyProperties<TSource>()
    //    {
    //        if (_props == null)
    //        {
    //            _props = typeof(T).GetProperties();
    //        }
    //        return _getClonePropertiesCore(typeof(TSource).GetProperties(), _props);
    //    }

    //    void _convertCore<TSource, TDestination>(TSource source, TDestination destination, IEnumerable<PropertyInfo> props)
    //    {
    //        foreach (var p in props)
    //        {
    //            var v = p.GetValue(source);
    //            if (v != null)
    //            {
    //                p.SetValue(destination, p.GetValue(source));
    //            }
    //        }
    //    }
    //    public List<TDestination> ToList<TDestination>(IEnumerable<string> id) where TDestination : IDocument, new()
    //    {
    //        var lst = new List<TDestination>();
    //        var props = _getCloneProperties(typeof(TDestination));
    //        this.ToDictionary();
    //        foreach (string i in id)
    //        {
    //            T src = _itemMap.Get(i);
    //            if (src != null)
    //            {
    //                var dst = new TDestination { Id = i };
    //                _convertCore(src, dst, props);
    //                lst.Add(dst);
    //            }
    //        }
    //        return lst;
    //    }
    //    //public List<TDestination> ToList<TDestination>() where TDestination : IDocument, new()
    //    //{
    //    //    this.ToDictionary();

    //    //    var lst = new List<TDestination>();
    //    //    var props = _getCloneProperties(typeof(TDestination));
    //    //    foreach (var p in this.ToDictionary())
    //    //    {
    //    //        TDestination dst = new TDestination { Id = p.Key };
    //    //        _convertCore(p.Value, dst, props);

    //    //        lst.Add(dst);
    //    //    }

    //    //    return lst;
    //    //}
    //    public IEnumerable<TDestination> ToList<TDestination>(Func<T, bool> selector) where TDestination : IDocument, new()
    //    {
    //        this.ToDictionary();

    //        var lst = new List<TDestination>();
    //        var props = _getCloneProperties(typeof(TDestination));
    //        foreach (var p in this.ToDictionary())
    //        {
    //            if (selector != null && !selector(p.Value)) continue;

    //            TDestination dst = new TDestination { Id = p.Key };
    //            _convertCore(p.Value, dst, props);

    //            lst.Add(dst);
    //        }

    //        return lst;
    //    }
    //    //public TDestination FindById<TDestination>(string id) where TDestination : IDocument, new()
    //    //{
    //    //    var src = this.FindById(id);
    //    //    if (src == null)
    //    //    {
    //    //        return default(TDestination);
    //    //    }
    //    //    var dst = new TDestination { Id = id };
    //    //    _convertCore(src, dst, _getCloneProperties(typeof(TDestination)));

    //    //    return dst;
    //    //}

    //    public T Insert<TSource>(string id, TSource item) where TSource : Document
    //    {
    //        var a = new T { Id = id };
    //        _convertCore<TSource, T>(item, a, _getCopyProperties<TSource>());

    //        this.Insert(id, a);
    //        return a;
    //    }
    //    public T Insert<TSource>(TSource item) where TSource : Document
    //    {
    //        var a = new T();
    //        _convertCore<TSource, T>(item, a, _getCopyProperties<TSource>());

    //        a.Id = this.Insert(a);
    //        return a;
    //    }

    //    public void Update<TSource>(string id, TSource item) where TSource : IDocument
    //    {
    //        var a = new T();
    //        _convertCore<TSource, T>(item, a, _getCopyProperties<TSource>());

    //        this.Update(id, a);
    //    }
    //    public void Update<TSource>(TSource item) where TSource : IDocument
    //    {
    //        this.Update<TSource>(item.Id, item);
    //    }
    //    #endregion
    //    public string Insert(string id, T item)
    //    {
    //        if (id == null)
    //        {
    //            id = new ObjectId().ToString();
    //        }
    //        _itemMap.Update(id, item);
    //        _fileManager.WriteCore(id, item);

    //        return id;
    //    }
    //    public string Insert(T item)
    //    {
    //        return this.Insert(null, item);
    //    }

    //    public void Update(string id, T item)
    //    {
    //        _itemMap.Update(id, item);
    //        _fileManager.WriteCore(id, item);
    //    }
    //    public void Update(T item)
    //    {
    //        this.Update(item.Id, item);
    //    }

    //    public T FindById(string id)
    //    {
    //        T value = _itemMap.Find(id);
    //        if (value == null)
    //        {
    //            var fi = _fileManager.GetFileInfo(id);
    //            if (!fi.Exists)
    //                return null;

    //            value = _fileManager.ReadCore(fi);
    //            value.Id = id;
    //        }
    //        else
    //        {
    //            if (value.Id == null)
    //                value.Id = id;
    //        }
    //        return value;
    //    }
    //    public void Move<TDestination>(TDestination item, Collection<TDestination> destination)
    //        where TDestination : Document, new()
    //    {
    //        destination.Insert(item.Id, item);
    //        Delete(item.Id);
    //    }
    //    public bool Contains(string id)
    //    {
    //        if (_itemMap.Contains(id))
    //        {
    //            return true;
    //        }

    //        return _fileManager.GetFileInfo(id).Exists;
    //    }
    //}
}

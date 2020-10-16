using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using SmartSecurity.Models;
using Newtonsoft.Json;

namespace SmartSecurity.Controllers
{
    public class Response
    {
        public long Status { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
        public object Data { get; set; }

        [JsonIgnore]
        public bool IsError { get => Status < 0; }
    }
    public class WebController : BaseController
    {
        public static event EventHandler ServerError;

        string _apiDomain { get; set; }
        public string Domain
        {
            get { return _apiDomain; }
        }
        public Response Response { get; private set; }
        public T Convert<T>()
        {
            var res = this.Response;
            if (res == null || res.IsError || res.Data == null)
            {
                return default(T);
            }
            return (T)(Response.Data = JsonConvert.DeserializeObject<T>(res.Data.ToString()));
        }
        public T Post<T>(string actionName, params object[] value)
        {
            if (Post(actionName, value))
            {
                return Convert<T>();
            }
            return default(T);
        }
        public T Get<T>(string actionName, params object[] param)
        {
            Get(actionName, param);
            return Convert<T>();
        }
        void Load(WebRequest request)
        {
            this.Response = null;

            var response = request.GetResponse() as HttpWebResponse;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                {
                    String responseString = reader.ReadToEnd();
                    this.Response = JsonConvert.DeserializeObject<Response>(responseString);
                }
            }
        }
        public WebController()
        {
            _apiDomain = "http://canhbao.tecvietnam.com/";
            //_apiDomain = "http://slock.aks.vn/";
        }
        protected virtual string GetApiUrl(string prefix, string actionName)
        {
            return _apiDomain + '/' + prefix + '/' + actionName;
        }
        protected string GetApiUrl(string actionName)
        {
            return this.GetApiUrl("api", actionName);
        }
        public bool Post(string actionName, params object[] value)
        {
            var request = WebRequest.Create(this.GetApiUrl(actionName));
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000;

                object v = null;
                if (value.Length > 0)
                {
                    v = value[0];
                }
                using (var sw = new System.IO.StreamWriter(request.GetRequestStream()))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(sw, v);
                }
                Load(request);
                return true;
            }
            catch (Exception e)
            {
                Response = new Response { Status = -101, Message = e.Message };
                ServerError?.Invoke(Response, null);
            }
            return false;
        }
        public void Get(string actionName, params object[] param)
        {
            string uri = GetApiUrl(actionName);
            if (param.Length > 0)
            {
                uri += '?' + string.Join("&", param);
            }
            Load(WebRequest.Create(uri));
        }

        //public System.Mvc.ActionResult Demo()
        //{
        //    var nv = Data.Base.GetCollection("NhanVien").FindById<Models.NhanVienRequest>("0989154248");
        //    return View(nv);
        //}

        //public System.Mvc.ActionResult Demo(Models.NhanVienRequest model)
        //{
        //    Post("GiamSatNhanVien/Send", model);
        //    return Done();
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JSViewer_MVC_Core.model
{
    public class ProccessResult
    {
        public bool Flag { get; set; } = true;

        public string Msg { get; set; }

        public static ProccessResult OnOk() => new ProccessResult();
        public static ProccessResult<T> OnOk<T>(T data, string msg = null) where T : class => new ProccessResult<T>() { Msg = msg, Data = data };

        public static ProccessResult OnFailed(string msg) => new ProccessResult() { Msg = msg, Flag = false };
        public static ProccessResult<T> OnFailed<T>(T data, string msg) where T : class => new ProccessResult<T>() { Msg = msg, Data = data, Flag = false };
    }

    public class ProccessResult<T> : ProccessResult where T : class
    {
        public T Data { get; set; }
    }
}
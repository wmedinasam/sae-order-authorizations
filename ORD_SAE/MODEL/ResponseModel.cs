using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    public class ResponseModel<T>
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public T Object { get; set; }
    }
}

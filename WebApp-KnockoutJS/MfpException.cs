using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp_KnockoutJS
{
    public class MfpException : Exception
    {
        public object Value { get; set; }

        public MfpException(string message, object value) : base(message)
        {
            this.Value = value;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using Citi.MyCitigoldFP.Common;
using System.Configuration;
using System.Web.Helpers;
using Citi.MyCitigoldFP.Common.Web.Auth;
using System.Net;
using System.Collections.Concurrent;

namespace JOEJY
{
    public class BackgroundLogger<T>
    {
        private readonly ConcurrentQueue<T> _logs = new ConcurrentQueue<T>();
        private readonly System.Threading.Timer _scheduler;
        private readonly Action<List<T>> _action;
        private int _doingCount = 0;

        private BackgroundLogger(Action<List<T>> action, int period)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (period <= 0)
                throw new ArgumentException("'period' must be greater than 0 (recommand 1000 = 1s)");
                
            _action = action;
            _scheduler = new System.Threading.Timer(new System.Threading.TimerCallback(Callback), null, (int)1000, period);
        }                
     
        private void Callback(object obj)
        {
            if (_doingCount == 1) return;
            _doingCount++;
            
            if (_logs.Count > 0)
            {
                List<T> list = new List<T>();
                T log;
                while(_logs.TryDequeue(out log))
                {
                    list.Add(log);
                }

                _action(list);
            }
            
            _doingCount--;
        }

        public void SetLog(T log)
        {
            _logs.Enqueue(log);
        }

        public static BackgroundLogger<T> GetLogger(Action<List<T>> action, int period)
        {
            return new BackgroundLogger<T>(action, period);
        }
    }
}

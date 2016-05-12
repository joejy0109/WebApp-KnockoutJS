using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JOEJY
{
    public class BackgroundTask<T>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<T> _workList = new ConcurrentQueue<T>();
        private readonly System.Threading.Timer _scheduler;
        private readonly Action<List<T>> _action;
        private int _doingCount = 0;

        private BackgroundTask(Action<List<T>> action, int period)
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

            if (_workList.Count > 0)
            {
               //List<T> list = new List<T>();
                //T item;
                //while (_workList.TryDequeue(out item))
                //    list.Add(item);

                int listCount = _workList.Count;
                int workingCount = listCount > 1000 ? 1000 : listCount;
                    
                //List<T> list = new List<T>(workingCount);
                //for (int i = 0; i < workingCount; i++)
                //{
                //    T item;
                //    if(_workList.TryDequeue(out item))
                //        list.Add(item);
                //}

                T[] list = new T[workingCount];
                for (int i = 0; i < workingCount; i++)
                {
                    T item;
                    if (_workList.TryDequeue(out item))
                        list[i] = item;
                }
               
                try { _action(list); }
                catch (Exception ex) { logger.Error(ex.ToString()); }
            }
            _doingCount--;
        }

        public void SetWork(T log)
        {
            _workList.Enqueue(log);
        }

        public static BackgroundTask<T> GetTask(Action<List<T>> action, int period)
        {
            return new BackgroundTask<T>(action, period);
        }
    }
}

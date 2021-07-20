using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);    
    }

    public class JobQueue :IJobQueue
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object Lock = new object();
        bool _flush = false;

        public void Push(Action job)
        {
            bool flush = false;
            lock (Lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false)
                {
                    flush = _flush = true;
                }
            }

            if (flush)
            {
                Flush();
            }
        }

        //딱 하나의 쓰레드만 이 함수를 실행함
        void Flush()
        {
            while (true)
            {
                Action action = Pop();
                //더이상 일감이 없을 때
                if (action == null) return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock (Lock)
            {

                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }

                return _jobQueue.Dequeue();
            }
        }
    }


}

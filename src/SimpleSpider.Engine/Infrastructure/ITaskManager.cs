namespace SimpleSpider.Engine.Infrastructure
{
    using System;

    public interface ITaskManager
    {
        int MaxConcurrency { get; }

        void RunTask(Action<object> method, object state);

        void StopAll();
    }
}

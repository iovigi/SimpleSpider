namespace SimpleSpider.Engine.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TaskManager : ITaskManager
    {
        private CancellationTokenSource cancellationTokenSource;

        private readonly TaskFactory factory;

        public TaskManager(int maxConcurrency)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.MaxConcurrency = maxConcurrency;
            this.factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(this.MaxConcurrency));
        }

        public int MaxConcurrency { get; }

        public void RunTask(Action<object> method, object state)
        {
            this.factory.StartNew(method, state, this.cancellationTokenSource.Token);
        }

        public void StopAll()
        {
            this.cancellationTokenSource.Cancel(false);
            this.cancellationTokenSource = new CancellationTokenSource();
        }
    }
}

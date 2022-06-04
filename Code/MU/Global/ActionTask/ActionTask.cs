using System;
using System.Threading;
using System.Threading.Tasks;

namespace MU.Global.ActionTask
{
    public class ActionTask
    {
        protected Task _actionTask;
        private CancellationTokenSource _cancelTokenSource;

        public bool IsRunning
        {
            get
            {
                return _actionTask.IsCompleted;
            }
        }

        public event TaskCompleted OnTaskCompleted;
        public delegate void TaskCompleted(ActionTask taskCompleted);

        public ActionTask(Action methodToCall, float durationMs = 1000, Action onCompleted = null)
        {
            _cancelTokenSource = new CancellationTokenSource();
            _actionTask = new Task(async () =>
            {
                methodToCall?.Invoke();
                await Task.Delay(TimeSpan.FromMilliseconds(durationMs));
                onCompleted?.Invoke();
                OnTaskCompleted?.Invoke(this);
            }, _cancelTokenSource.Token);
        }

        public void StartTask()
        {
            if (_actionTask.Status == TaskStatus.Created)
            {
                _actionTask.Start();
            }
        }

        public void CancelTask()
        {
            if (!_actionTask.IsCanceled)
            {
                _cancelTokenSource.Cancel();
            }
        }
    }
}

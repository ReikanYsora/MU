using MU.Global.ActionTask;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MU.Managers
{
    public class MUManager : INotifyPropertyChanged
    {
        #region ATTRIBUTES
        private const string BASE_IMAGE_URI = "ms-appx://MU/Resources/Icons/Managers/";
        private string _icon_URI;
        private ObservableCollection<ActionTask> _actionTask;
        #endregion

        #region PROPERTIES
        public string Name { protected set; get; }
        public string Version { protected set; get; }
        public string IconUri
        {
            protected set
            {
                _icon_URI = BASE_IMAGE_URI + value;
            }
            get
            {
                return _icon_URI;
            }
        }
        public bool Status { protected set; get; }
        #endregion

        #region EVENTS
        public event FunctionInitialized OnFunctionInitialized;
        public delegate void FunctionInitialized(object sender, string name, string version, string iconuri);

        public event ExceptionOccured OnExceptionOccured;
        public delegate void ExceptionOccured(object sender, string name, string version, string iconuri, Exception e);

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region CONSTRUCTOR
        public MUManager()
        {
            _actionTask = new ObservableCollection<ActionTask>();
        }
        #endregion

        #region METHODS
        public virtual Task InitializeAsync()
        {
            return Task.FromResult(default(object));
        }

        public void AddTask(ActionTask task)
        {
            if (task != null)
            {
                task.OnTaskCompleted += CBTaskCompleted;
            }
            _actionTask.Add(task);
            CheckAndStartFirstTask();
        }

        private void CBTaskCompleted(ActionTask taskCompleted)
        {
            _actionTask.Remove(taskCompleted);
            CheckAndStartFirstTask();
        }

        private void CheckAndStartFirstTask()
        {
            int taskRunningCount = _actionTask.Where(x => x.IsRunning).ToList().Count;
            if ((taskRunningCount == 0) && (_actionTask.Count > 0))
            {
                _actionTask[0].StartTask();
            }
        }

        public void CancelAllTask()
        {
            _actionTask.Where(x => x.IsRunning).ToList().ForEach(x => x.CancelTask());
            _actionTask.Clear();
        }
        #endregion

        #region CALLBACKS
        protected void RaiseOnFunctionInitialized()
        {
            Status = true;
            OnFunctionInitialized?.Invoke(this, Name, Version, IconUri);
        }

        protected void RaiseOnExceptionOccured(Exception e)
        {
            Status = false;
            OnExceptionOccured?.Invoke(this, Name, Version, IconUri, e);
        }

        protected void RaiseOnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

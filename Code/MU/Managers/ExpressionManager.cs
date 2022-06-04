using MU.Global;
using MU.Global.ActionTask;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace MU.Managers
{
    public class ExpressionManager : MUManager, IDisposable
    {
        #region ATTRIBUTES
        public const string CORE_NAME = "ExpressionManager";
        public const string CORE_VERSION = "1.3";
        public const string ICON_NAME = "MU_MANAGER_EXPRESSIONS.png";

        private Thickness _eyesDirection;
        private Expression _actualExpression;
        #endregion

        #region PROPERTIES
        public Expression ActualExpression
        {
            set
            {
                if (_actualExpression != value)
                {
                    _actualExpression = value;
                    RaiseOnPropertyChanged("ActualExpression");
                }
            }
            get
            {
                return _actualExpression;
            }
        }

        public Thickness EyesDirection
        {
            set
            {
                if (_eyesDirection != value)
                {
                    _eyesDirection = value;
                    RaiseOnPropertyChanged("EyesDirection");
                }
            }
            get
            {
                return _eyesDirection;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public ExpressionManager()
        {
            Name = CORE_NAME;
            Version = CORE_VERSION;
            IconUri = ICON_NAME;
        }
        #endregion

        #region METHODS
        public async override Task InitializeAsync()
        {
            try
            {
                CheckForNextExpression();
                await Task.Delay(100);

                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void CheckForNextExpression()
        {
            Random rnd = new Random();
            int blinkValue = rnd.Next(0, 100);
            if (blinkValue <= 15)
            {
                AddTask(new ActionTask(BlinkEyes, 300, CheckForNextExpression));
            }
            else
            {
                AddTask(new ActionTask(ActualExpressionAsync, 300, CheckForNextExpression));
            }
        }

        #region EYES POSITION
        private async void ActualExpressionAsync()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                ActualExpression = BehaviorDatas.ActualExpression;
            });
        }
        #endregion

        #region BLINK EYES
        private async void BlinkEyes()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                ActualExpression = Expression.EyesClosed;
                EyesDirection = new Thickness(0, 0, 0, 0);
            });
        }
        #endregion

        public void Dispose()
        {
            try
            {
                if (Status)
                {
                    Status = false;
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }
        #endregion
    }

    [DataContract(Name = "Expression")]
    public enum Expression
    {
        [EnumMember] Normal,
        [EnumMember] EyesClosed,
        [EnumMember] Happy,
        [EnumMember] Angry,
        [EnumMember] Sad,
        [EnumMember] Surprised,
        [EnumMember] Jaded,
        [EnumMember] Skeptic
    }
}

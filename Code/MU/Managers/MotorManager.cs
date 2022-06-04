using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace MU.Managers
{
    public class MotorManager : MUManager, IDisposable
    {
        #region ATTRIBUTES
        private const string CORE_NAME = "MotorManager";
        private const string CORE_VERSION = "1.1";
        public const string ICON_NAME = "MU_MANAGER_MOTOR.png";

        private const short MOTOR_G_1 = 24;
        private const short MOTOR_G_2 = 25;
        private const short MOTOR_D_1 = 4;
        private const short MOTOR_D_2 = 18;

        private GpioPin MOTOR_G_1Pin;
        private GpioPin MOTOR_G_2Pin;
        private GpioPin MOTOR_D_1Pin;
        private GpioPin MOTOR_D_2Pin;

        private DispatcherTimer _actionExecutionTimer;

        private MotorPosition _actualMotorPosition;
        #endregion

        #region PROPERTIES
        public MotorPosition ActualMotorPosition
        {
            set
            {
                if (_actualMotorPosition != value)
                {
                    _actualMotorPosition = value;

                    RaiseOnPropertyChanged("ActualMotorPosition");
                }
            }
            get
            {
                return _actualMotorPosition;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public MotorManager()
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
                GpioController gpio = GpioController.GetDefault();

                if (gpio == null)
                {
                    MOTOR_G_1Pin = null;
                    MOTOR_G_2Pin = null;
                    MOTOR_D_1Pin = null;
                    MOTOR_D_2Pin = null;

                    throw new Exception("No GPIO controller");
                }

                MOTOR_G_1Pin = gpio.OpenPin(MOTOR_G_1);
                MOTOR_G_1Pin.Write(GpioPinValue.Low);
                MOTOR_G_1Pin.SetDriveMode(GpioPinDriveMode.Output);

                MOTOR_G_2Pin = gpio.OpenPin(MOTOR_G_2);
                MOTOR_G_2Pin.Write(GpioPinValue.Low);
                MOTOR_G_2Pin.SetDriveMode(GpioPinDriveMode.Output);

                MOTOR_D_1Pin = gpio.OpenPin(MOTOR_D_1);
                MOTOR_D_1Pin.Write(GpioPinValue.Low);
                MOTOR_D_1Pin.SetDriveMode(GpioPinDriveMode.Output);

                MOTOR_D_2Pin = gpio.OpenPin(MOTOR_D_2);
                MOTOR_D_2Pin.Write(GpioPinValue.Low);
                MOTOR_D_2Pin.SetDriveMode(GpioPinDriveMode.Output);

                ActualMotorPosition = MotorPosition.Shutdown;

                _actionExecutionTimer = new DispatcherTimer();
                _actionExecutionTimer.Interval = TimeSpan.FromMilliseconds(10);
                _actionExecutionTimer.Tick += CBActionExecution;
                _actionExecutionTimer.Start();

                await Task.Delay(100);

                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void CBActionExecution(object sender, object e)
        {
            switch (_actualMotorPosition)
            {
                case MotorPosition.Shutdown:
                    ShutDown();
                    break;
                case MotorPosition.Front:
                    MoveFront();
                    break;
                case MotorPosition.Rear:
                    MoveRear();
                    break;
                case MotorPosition.Left:
                    MoveLeft();
                    break;
                case MotorPosition.Right:
                    MoveRight();
                    break;
            }
        }

        private void MoveFront()
        {
            try
            {
                if (Status)
                {
                    MOTOR_G_1Pin.Write(GpioPinValue.Low);
                    MOTOR_G_2Pin.Write(GpioPinValue.High);
                    MOTOR_D_1Pin.Write(GpioPinValue.High);
                    MOTOR_D_2Pin.Write(GpioPinValue.Low);
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void MoveRear()
        {
            try
            {
                if (Status)
                {
                    MOTOR_G_1Pin.Write(GpioPinValue.High);
                    MOTOR_G_2Pin.Write(GpioPinValue.Low);
                    MOTOR_D_1Pin.Write(GpioPinValue.Low);
                    MOTOR_D_2Pin.Write(GpioPinValue.High);
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void MoveRight()
        {
            try
            {
                if (Status)
                {
                    MOTOR_G_1Pin.Write(GpioPinValue.High);
                    MOTOR_G_2Pin.Write(GpioPinValue.Low);
                    MOTOR_D_1Pin.Write(GpioPinValue.High);
                    MOTOR_D_2Pin.Write(GpioPinValue.Low);
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void MoveLeft()
        {
            try
            {
                if (Status)
                {
                    MOTOR_G_1Pin.Write(GpioPinValue.Low);
                    MOTOR_G_2Pin.Write(GpioPinValue.High);
                    MOTOR_D_1Pin.Write(GpioPinValue.Low);
                    MOTOR_D_2Pin.Write(GpioPinValue.High);
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void ShutDown()
        {
            try
            {
                if (Status)
                {
                    MOTOR_G_1Pin.Write(GpioPinValue.Low);
                    MOTOR_G_2Pin.Write(GpioPinValue.Low);
                    MOTOR_D_1Pin.Write(GpioPinValue.Low);
                    MOTOR_D_2Pin.Write(GpioPinValue.Low);
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        public void Dispose()
        {
            try
            {
                if (Status)
                {
                    Status = false;

                    if (_actionExecutionTimer != null)
                    {
                        _actionExecutionTimer.Stop();
                        _actionExecutionTimer = null;
                    }

                    if (MOTOR_G_1Pin != null)
                    {
                        MOTOR_G_1Pin.Write(GpioPinValue.Low);
                    }

                    if (MOTOR_G_2Pin != null)
                    {
                        MOTOR_G_2Pin.Write(GpioPinValue.Low);
                    }

                    if (MOTOR_D_1Pin != null)
                    {
                        MOTOR_D_1Pin.Write(GpioPinValue.Low);
                    }

                    if (MOTOR_D_2Pin != null)
                    {
                        MOTOR_D_2Pin.Write(GpioPinValue.Low);
                    }
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }
        #endregion
    }

    public enum MotorPosition
    {
        Shutdown = 0,
        Front = 1,
        Rear = 2,
        Left = 3,
        Right = 4
    }
}

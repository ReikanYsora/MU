using MU.Global.ActionTask;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace MU.Managers
{
    public class UltraSonicManager : MUManager, IDisposable
    {
        #region ATTRIBUTES
        public const string CORE_NAME = "UltraSonicManager";
        public const string CORE_VERSION = "1.2";
        public const string ICON_NAME = "MU_MANAGER_US.png";

        private const short TRIG_PIN_NUMBER = 27;
        private const short ECHO_PIN_NUMBER = 17;

        private GpioPin TRIGGER_PIN;
        private GpioPin ECHO_PIN;
        
        private double _distance;

        #region EVENTS
        public event DataCalculated OnDataCalculated;
        public delegate void DataCalculated(object sender, double data);
        #endregion
        #endregion

        #region PROPERTIES
        public double Distance
        {
            set
            {
                if (value != _distance)
                {
                    _distance = value;
                    RaiseOnPropertyChanged("Distance");
                }
            }
            get
            {
                return _distance;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public UltraSonicManager()
        {
            Name = CORE_NAME;
            Version = CORE_VERSION;
            IconUri = ICON_NAME;
            Distance = 0.0f;
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
                    TRIGGER_PIN = null;
                    ECHO_PIN = null;

                    throw new Exception("No GPIO controller");
                }

                TRIGGER_PIN = gpio.OpenPin(TRIG_PIN_NUMBER);
                TRIGGER_PIN.SetDriveMode(GpioPinDriveMode.Output);

                ECHO_PIN = gpio.OpenPin(ECHO_PIN_NUMBER);
                ECHO_PIN.SetDriveMode(GpioPinDriveMode.Input);

                AddDetectionTask();
                await Task.Delay(100);

                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        public void AddDetectionTask()
        {
            AddTask(new ActionTask(CalculateDistance, 500));
        }

        private void CalculateDistance()
        {
            try
            {
                var mre = new ManualResetEvent(false);
                var sw = new Stopwatch();
                var timeout = new Stopwatch();

                if (Status)
                {
                    if (ECHO_PIN.Read() == GpioPinValue.Low)
                    {
                        TRIGGER_PIN.Write(GpioPinValue.High);
                        mre.WaitOne(10);
                        TRIGGER_PIN.Write(GpioPinValue.Low);
                    }

                    if (ECHO_PIN.Read() == GpioPinValue.Low)
                    {
                        timeout.Start();
                        while (ECHO_PIN.Read() != GpioPinValue.High)
                        {
                            if (timeout.Elapsed.TotalSeconds >= 2)
                            {
                                timeout.Stop();
                            }
                        }
                    }
                    timeout.Reset();
                    timeout.Start();
                    sw.Start();
                    while (ECHO_PIN.Read() == GpioPinValue.High)
                    {
                        if (timeout.Elapsed.TotalSeconds >= 2)
                        {
                            timeout.Stop();
                        }
                    }
                    sw.Stop();
                }

                Distance = sw.Elapsed.TotalSeconds * 17014;
                OnDataCalculated?.Invoke(this, Distance);
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

                    if (TRIGGER_PIN != null)
                    {
                        TRIGGER_PIN.Write(GpioPinValue.Low);
                    }

                    if (ECHO_PIN != null)
                    {
                        ECHO_PIN.Write(GpioPinValue.Low);
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
}

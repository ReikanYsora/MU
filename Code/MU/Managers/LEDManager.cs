using MU.Global.ActionTask;
using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace MU.Managers
{
    public class LEDManager : MUManager, IDisposable
    {
        #region ATTRIBUTES
        public const string CORE_NAME = "LEDManager";
        public const string CORE_VERSION = "1.1";
        public const string ICON_NAME = "MU_MANAGER_LED.png";

        private const short LED_G_1 = 13;
        private const short LED_G_2 = 19;
        private const short LED_G_3 = 26;
        private const short LED_D_1 = 21;
        private const short LED_D_2 = 16;
        private const short LED_D_3 = 20;

        private GpioPin LED_G_1Pin;
        private GpioPin LED_G_2Pin;
        private GpioPin LED_G_3Pin;
        private GpioPin LED_D_1Pin;
        private GpioPin LED_D_2Pin;
        private GpioPin LED_D_3Pin;
        #endregion
        
        #region CONSTRUCTOR
        public LEDManager()
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
                    LED_G_1Pin = null;
                    LED_G_2Pin = null;
                    LED_G_3Pin = null;
                    LED_D_1Pin = null;
                    LED_D_2Pin = null;
                    LED_D_3Pin = null;

                    throw new Exception("No GPIO controller");
                }

                LED_G_1Pin = gpio.OpenPin(LED_G_1);
                LED_G_1Pin.Write(GpioPinValue.Low);
                LED_G_1Pin.SetDriveMode(GpioPinDriveMode.Output);

                LED_G_2Pin = gpio.OpenPin(LED_G_2);
                LED_G_2Pin.Write(GpioPinValue.Low);
                LED_G_2Pin.SetDriveMode(GpioPinDriveMode.Output);

                LED_G_3Pin = gpio.OpenPin(LED_G_3);
                LED_G_3Pin.Write(GpioPinValue.Low);
                LED_G_3Pin.SetDriveMode(GpioPinDriveMode.Output);

                LED_D_1Pin = gpio.OpenPin(LED_D_1);
                LED_D_1Pin.Write(GpioPinValue.Low);
                LED_D_1Pin.SetDriveMode(GpioPinDriveMode.Output);

                LED_D_2Pin = gpio.OpenPin(LED_D_2);
                LED_D_2Pin.Write(GpioPinValue.Low);
                LED_D_2Pin.SetDriveMode(GpioPinDriveMode.Output);

                LED_D_3Pin = gpio.OpenPin(LED_D_3);
                LED_D_3Pin.Write(GpioPinValue.Low);
                LED_D_3Pin.SetDriveMode(GpioPinDriveMode.Output);                

                await Task.Delay(100);

                RaiseOnFunctionInitialized();

                LightAll();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void ChangeLedsValues(bool g1, bool g2, bool g3, bool d3, bool d2, bool d1)
        {
            if (!g1)
            {
                LED_G_1Pin.Write(GpioPinValue.Low);
            }
            else
            {
                LED_G_1Pin.Write(GpioPinValue.High);
            }

            if (!g2)
            {
                LED_G_2Pin.Write(GpioPinValue.Low);
            }
            else
            {
                LED_G_2Pin.Write(GpioPinValue.High);
            }

            if (!g3)
            {
                LED_G_3Pin.Write(GpioPinValue.Low);
            }
            else
            {
                LED_G_3Pin.Write(GpioPinValue.High);
            }

            if (!d3)
            {
                LED_D_3Pin.Write(GpioPinValue.Low);
            }
            else
            {
                LED_D_3Pin.Write(GpioPinValue.High);
            }

            if (!d2)
            {
                LED_D_2Pin.Write(GpioPinValue.Low);
            }
            else
            {
                LED_D_2Pin.Write(GpioPinValue.High);
            }

            if (!d1)
            {
                LED_D_1Pin.Write(GpioPinValue.Low);
            }
            else
            {
                LED_D_1Pin.Write(GpioPinValue.High);
            }
        }

        public void Dispose()
        {
            try
            {
                if (Status)
                {
                    Status = false;

                    if (LED_G_1Pin != null)
                    {
                        LED_G_1Pin.Write(GpioPinValue.Low);
                    }

                    if (LED_D_1Pin != null)
                    {
                        LED_D_1Pin.Write(GpioPinValue.Low);
                    }

                    if (LED_G_2Pin != null)
                    {
                        LED_G_2Pin.Write(GpioPinValue.Low);
                    }

                    if (LED_D_2Pin != null)
                    {
                        LED_D_2Pin.Write(GpioPinValue.Low);
                    }

                    if (LED_G_3Pin != null)
                    {
                        LED_G_3Pin.Write(GpioPinValue.Low);
                    }

                    if (LED_D_3Pin != null)
                    {
                        LED_D_3Pin.Write(GpioPinValue.Low);
                    }
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        #region LIGHT MODE
        #region LIGHT ALL
        public void LightAll()
        {
            CancelAllTask();
            AddTask(new ActionTask(LightAllStep1, 100));
        }

        private void LightAllStep1()
        {
            ChangeLedsValues(true, true, true, true, true, true);
        }
        #endregion

        #region UNLIGHT ALL
        public void UnlightAll()
        {
            CancelAllTask();
            AddTask(new ActionTask(UnlightAllStep1, 100));
        }

        private void UnlightAllStep1()
        {
            ChangeLedsValues(false, false, false, false, false, false);
        }
        #endregion

        #region BLINK MODE
        public void Blink(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddTask(new ActionTask(LightAllStep1, 50));
                AddTask(new ActionTask(UnlightAllStep1, 50));
            }
        }
        #endregion

        #region SCROLL
        public void ScrollAll()
        {
            AddTask(new ActionTask(ScrollStep1, 100));
            AddTask(new ActionTask(ScrollStep2, 100));
            AddTask(new ActionTask(ScrollStep3, 100));
            AddTask(new ActionTask(ScrollStep4, 100));
            AddTask(new ActionTask(ScrollStep5, 100));
            AddTask(new ActionTask(ScrollStep6, 100));
            AddTask(new ActionTask(ScrollStep1, 100));
            AddTask(new ActionTask(ScrollStep2, 100));
            AddTask(new ActionTask(ScrollStep3, 100));
            AddTask(new ActionTask(ScrollStep4, 100));
            AddTask(new ActionTask(ScrollStep5, 100));
            AddTask(new ActionTask(ScrollStep6, 100));
            AddTask(new ActionTask(ScrollStep1, 100));
            AddTask(new ActionTask(ScrollStep2, 100));
            AddTask(new ActionTask(ScrollStep3, 100));
            AddTask(new ActionTask(ScrollStep4, 100));
            AddTask(new ActionTask(ScrollStep5, 100));
            AddTask(new ActionTask(ScrollStep6, 100));
            AddTask(new ActionTask(LightAllStep1, 100));
        }

        private void ScrollStep1()
        {
            ChangeLedsValues(true, false, false, false, false, true);
        }

        private void ScrollStep2()
        {
            ChangeLedsValues(true, true, false, false, true, true);
        }

        private void ScrollStep3()
        {
            ChangeLedsValues(true, true, true, true, true, true);
        }

        private void ScrollStep4()
        {
            ChangeLedsValues(false, true, true, true, true, false);
        }

        private void ScrollStep5()
        {
            ChangeLedsValues(false, false, true, true, false, false);
        }

        private void ScrollStep6()
        {
            ChangeLedsValues(false, false, false, false, false, false);
        }
        #endregion
        #endregion

        #endregion
    }
}

using MU.Devices;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace MU.Managers
{
    public class CompassManager : MUManager, IDisposable
    {

        #region ATTRIBUTES
        public const string CORE_NAME = "CompassManager";
        public const string CORE_VERSION = "1.1";
        public const string ICON_NAME = "MU_MANAGER_COMPASS.png";
        private HMC5883L _compass;
        private DispatcherTimer _getValuesTimer;
        private int _x;
        private int _y;
        private int _z;
        #endregion

        #region PROPERTIES
        public int X
        {
            private set
            {
                if (_x != value)
                {
                    _x = value;
                    RaiseOnPropertyChanged("X");
                }
            }
            get
            {
                return _x;
            }
        }
    
        public int Y
        {
            private set
            {
                if (_y != value)
                {
                    _y = value;
                    RaiseOnPropertyChanged("Y");
                }
            }
            get
            {
                return _y;
            }
        }

        public int Z
        {
            private set
            {
                if (_z != value)
                {
                    _z = value;
                    RaiseOnPropertyChanged("Z");
                }
            }
            get
            {
                return _z;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public CompassManager()
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
                _compass = new HMC5883L(MeasurementMode.Continuous);
                await _compass.InitializeAsync();

                _getValuesTimer = new DispatcherTimer();
                _getValuesTimer.Interval = new TimeSpan(0, 0, 1);
                _getValuesTimer.Tick += CBCheckValues;
                _getValuesTimer.Start();

                await Task.Delay(100);

                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void CBCheckValues(object sender, object e)
        {
            if (_compass != null)
            {
                var direction = _compass.ReadRaw();
                X = direction.X_Axis;
                Y = direction.Y_Axis;
                Z = direction.Z_Axis;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Status)
                {
                    Status = false;

                    if (_getValuesTimer != null)
                    {
                        _getValuesTimer.Stop();
                        _getValuesTimer = null;
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

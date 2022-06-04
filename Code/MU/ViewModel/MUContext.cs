using MU.Configuration;
using MU.Global;
using MU.Global.ActionTask;
using MU.Global.Tools;
using MU.Managers;
using MU.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.FaceAnalysis;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace MU.ViewModel
{
    public class MUContext : MUManager
    {
        #region ATTRIBUTES
        #region CONFIGURATION
        public const string CORE_NAME = "BehaviorManager";
        public const string CORE_VERSION = "1.0";
        public const string ICON_NAME = "MU_MANAGER_BEHAVIOR.png";

        #region CONFIGURATION
        private const string CONFIG_FILE_PATH = "MU_config.cfg";
        private const string CONFIG_DEFAULT_LANGUAGE = "fr-FR";
        private const string CONFIG_DEFAULT_SSID = "";
        private const string CONFIG_DEFAULT_SSID_PASSWORD = "";
        private ConfigFileInfos _config;
        #endregion

        private MUMainPage _mainView;
        #endregion

        #region MANAGERS
        private WiFiManager _wifiManager;
        private MotorManager _motorManager;
        private LEDManager _LEDManager;
        private UltraSonicManager _ultraSonicManager;
        private WebServerManager _webServerManager;
        private ExpressionManager _expressionManager;
        private VisionManager _visionManager;
        private SpeechRecognitionManager _speechReconManager;
        #endregion
        #endregion

        #region PROPERTIES
        public ExpressionManager ExpressionManager
        {
            get
            {
                return _expressionManager;
            }
        }

        public void LEDScroll()
        {
            if (_LEDManager != null)
            {
                _LEDManager.ScrollAll();
            }
        }

        public void LEDNormal()
        {
            if (_LEDManager != null)
            {
                _LEDManager.AddTask(new ActionTask(_LEDManager.LightAll, 200));
            }
        }

        public UltraSonicManager UltraSonicManager
        {
            get
            {
                return _ultraSonicManager;
            }
        }

        public VisionManager CameraManager
        {
            get
            {
                return _visionManager;
            }
        }

        public Visibility VisionViewVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public MUContext(MUMainPage mainView)
        {
            Name = CORE_NAME;
            Version = CORE_VERSION;
            IconUri = ICON_NAME;

            _mainView = mainView;

            BehaviorDatas.Managers = new List<MUManager>();
            _LEDManager = new LEDManager();
            _wifiManager = new WiFiManager();
            _motorManager = new MotorManager();
            _ultraSonicManager = new UltraSonicManager();
            _ultraSonicManager.OnDataCalculated += BehaviorDatas.CBUltraSonicManager_OnDataCalculated;
            _expressionManager = new ExpressionManager();
            _visionManager = new VisionManager();
            _visionManager.OnFaceDetected += CBVisionManager_OnFaceDetected;

            _speechReconManager = new SpeechRecognitionManager();
            _speechReconManager.OnSpeechDetected += (async (sender, tagDatas, textDatas) =>
            {
                await CBSpeechReconManager_OnSpeechDetectedAsync(sender, tagDatas, textDatas);
            });

            BehaviorDatas.Managers.Add(_LEDManager);
            BehaviorDatas.Managers.Add(_wifiManager);
            BehaviorDatas.Managers.Add(_motorManager);
            BehaviorDatas.Managers.Add(_ultraSonicManager);
            BehaviorDatas.Managers.Add(_expressionManager);
            BehaviorDatas.Managers.Add(_visionManager);
            BehaviorDatas.Managers.Add(_speechReconManager);
            BehaviorDatas.Managers.Add(_webServerManager);
        }
        #endregion

        #region BEHAVIOR
        #endregion

        #region METHODS
        public async Task InitializeContextASync()
        {
            await LoadConfigurationAsync();
            await InitializeManagersAsync();
            await LoadNetworkAsync();
        }

        private async Task LoadConfigurationAsync()
        {
            _config = await Serializer<ConfigFileInfos>.Load(CONFIG_FILE_PATH);

            if (_config == null)
            {
                _config = new ConfigFileInfos
                {
                    Language = CONFIG_DEFAULT_LANGUAGE,
                    WiFi_Password = CONFIG_DEFAULT_SSID_PASSWORD,
                    WiFi_SSID = CONFIG_DEFAULT_SSID
                };
                Serializer<ConfigFileInfos>.Save(CONFIG_FILE_PATH, _config);
            }
        }

        private async Task LoadNetworkAsync()
        {
            await _wifiManager.ConnectAsync(_config.WiFi_SSID, _config.WiFi_Password);
            _webServerManager = new WebServerManager(_wifiManager, _config, CONFIG_FILE_PATH);
            _webServerManager.OnConfigurationChanged += CBWebServerManager_ConfigurationChanged;
        }

        private async Task InitializeManagersAsync()
        {
            try
            {
                foreach (MUManager m in BehaviorDatas.Managers)
                {
                    m.OnExceptionOccured += async (sender, name, version, iconuri, e) =>
                    {
                        await CBManager_OnExceptionOccured(sender, name, version, iconuri, e);
                    };
                    await m.InitializeAsync();
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region CALLBACKS
        #region EXCEPTION MANAGEMENT
        private async Task CBManager_OnExceptionOccured(object sender, string name, string version, string iconuri, Exception e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                _mainView.ShowToast("Error encountered in " + name + " (v." + version + ")", e.GetBaseException().Message, new Uri(iconuri));
            });
        }
        #endregion

        #region FACIAL RECOGNITION
        private void CBVisionManager_OnFaceDetected(object sender, IList<DetectedFace> faces, FaceOffset mainFacePosition)
        {            
            if (_expressionManager != null)
            {
                double left = 0;
                double top = 0;
                double right = 0;
                double bottom = 0;

                if (mainFacePosition.X > 0)
                {
                    right += mainFacePosition.X * 300;
                }
                else if (mainFacePosition.X < 0)
                {
                    left += mainFacePosition.X * -300;
                }

                if (mainFacePosition.Y > 0)
                {
                    top += mainFacePosition.Y * 300;
                }
                else if (mainFacePosition.Y < 0)
                {
                    bottom += mainFacePosition.Y * -300;
                }

                _expressionManager.EyesDirection = new Thickness(left, top, right, bottom);
            }
        }
        #endregion

        #region SPEECH RECOGNITION
        private async Task CBSpeechReconManager_OnSpeechDetectedAsync(object sender, string tagDatas, string textDatas)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                if (tagDatas.Contains("display"))
                {
                    if (tagDatas.Contains("mainPanel"))
                    {
                        _mainView.DisplayMainPanel();
                    }
                }
                else if (tagDatas.Contains("hide"))
                {
                    if (tagDatas.Contains("mainPanel"))
                    {
                        _mainView.HideMainPanel();
                    }
                }
            });
        }
        #endregion

        private void CBWebServerManager_ConfigurationChanged(object sender, string name, string version)
        {

        }
        #endregion
    }
}
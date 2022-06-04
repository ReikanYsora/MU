using MU.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.UI.Core;

namespace MU.Managers
{
    public class WiFiManager : MUManager
    {
        #region ATTRIBUTES
        public const string CORE_NAME = "WiFiManager";
        public const string CORE_VERSION = "1.0";
        public const string ICON_NAME = "MU_MANAGER_WIFI.png";
        
        private WiFiConnectionStatus _connectionStatus;
        #endregion

        #region PROPERTIES
        public WiFiConnectionStatus ConnectionStatus
        {
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    RaiseOnPropertyChanged("ConnectionStatus");
                }
            }
            get
            {
                return _connectionStatus;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public WiFiManager()
        {
            Name = CORE_NAME;
            Version = CORE_VERSION;
            IconUri = ICON_NAME;
            ConnectionStatus = WiFiConnectionStatus.NetworkNotAvailable;
        }
        #endregion

        #region METHODS
        public async override Task InitializeAsync()
        {
            try
            {

                await Task.Delay(100);

                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }
        
        public async Task ConnectAsync(string ssid, string password)
        {
            try
            {
                DeviceInformationCollection result = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                WiFiAdapter adapter = await WiFiAdapter.FromIdAsync(result[0].Id);

                if (adapter != null)
                {
                    adapter.Disconnect();
                }
                
                await adapter.ScanAsync();

                if ((adapter.NetworkReport != null) && (adapter.NetworkReport.AvailableNetworks != null))
                {
                    foreach (WiFiAvailableNetwork an in adapter.NetworkReport.AvailableNetworks)
                    {
                        if (an.Ssid == ssid)
                        {
                            WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Automatic;
                            WiFiConnectionResult connectionResult = null;

                            if ((an.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211) && (an.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None))
                            {
                                connectionResult = await adapter.ConnectAsync(an, reconnectionKind);
                            }
                            else
                            {
                                var credential = new PasswordCredential();

                                if (!string.IsNullOrEmpty(password))
                                {
                                    credential.Password = password;
                                }

                                connectionResult = await adapter.ConnectAsync(an, reconnectionKind, credential);
                            }

                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                ConnectionStatus = connectionResult.ConnectionStatus;
                            });
                            break;
                        }
                    }
                }
                else
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ConnectionStatus = WiFiConnectionStatus.NetworkNotAvailable;
                    });
                }
            }
            catch (Exception e)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    ConnectionStatus = WiFiConnectionStatus.NetworkNotAvailable;
                });

                RaiseOnExceptionOccured(e);
            }
        }

        public async Task<List<WiFiInformation>> GetNetworkSSID()
        {
            List<WiFiInformation> results = new List<WiFiInformation>();

            try
            {
                DeviceInformationCollection result = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                WiFiAdapter adapter = await WiFiAdapter.FromIdAsync(result[0].Id);

                if (adapter != null)
                {
                    adapter.Disconnect();
                }

                await adapter.ScanAsync();

                if ((adapter.NetworkReport != null) && (adapter.NetworkReport.AvailableNetworks != null))
                {
                    foreach (WiFiAvailableNetwork an in adapter.NetworkReport.AvailableNetworks)
                    {
                        int signal = Convert.ToInt16(an.SignalBars);
                        string signalColor = string.Empty;
                        switch (signal)
                        {
                            case 0:
                            default:
                                signalColor = "bgcolor='#cc0000'";
                                break;
                            case 1:
                                signalColor = "bgcolor='#ff6600'";
                                break;
                            case 2:
                                signalColor = "bgcolor='#ffcc00'";
                                break;
                            case 3:
                                signalColor = "bgcolor='#ccff66'";
                                break;
                            case 4:
                                signalColor = "bgcolor='#66ff66'";
                                break;
                        }
                        results.Add(new WiFiInformation
                        {
                            SSID = an.Ssid,
                            SignalColor = signalColor,
                            Signal = signal
                        });
                    }
                }
                else
                {
                    throw new Exception("Adapter error");
                }
            }
            catch (Exception)
            {
                results.Clear();
            }

            return results;
        }
        #endregion
    }

    public class WiFiInformation
    {
        public string SSID { set; get; }
        public string SignalColor { set; get; }
        public int Signal { set; get; }
    }
}

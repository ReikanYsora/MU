using MU.Configuration;
using MU.Global.Tools;
using MU.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MU.Managers
{
    public class WebServerManager : MUManager, IDisposable
    {
        #region ATTRIBUTES
        public const string CORE_NAME = "WebServerManager";
        public const string CORE_VERSION = "1.0";
        public const string ICON_NAME = "MU_MANAGER_WEBSERVER.png";

        private const uint BufferSize = 8192;

        private string _configFilePath;
        private ConfigFileInfos _fileInfos;
        private WiFiManager _wifiManager;
        private StreamSocketListener listener;
        #endregion

        #region EVENTS
        public event ConfigurationChanged OnConfigurationChanged;
        public delegate void ConfigurationChanged(object sender, string name, string version);
        #endregion

        #region CONSTRUCTOR
        public WebServerManager(WiFiManager wifiManager, ConfigFileInfos fileInfos, string configFilePath)
        {
            Name = CORE_NAME;
            Version = CORE_VERSION;
            IconUri = ICON_NAME;

            _wifiManager = wifiManager;
            _fileInfos = fileInfos;
            _configFilePath = configFilePath;
        }
        #endregion

        #region METHODS
        public async override Task InitializeAsync()
        {
            try
            {
                listener = new StreamSocketListener();

                await listener.BindServiceNameAsync("90");

                listener.ConnectionReceived += (sender, args) =>
                {
                    CBHandleRequest(sender, args);
                };

                await Task.Delay(100);

                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }
        
        public async void CBHandleRequest(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                StringBuilder request = new StringBuilder();

                using (IInputStream input = args.Socket.InputStream)
                {
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = BufferSize;
                    while (dataRead == BufferSize)
                    {
                        await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                        request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;
                    }
                }

                PostParams parametersSended = GetQuery(request);

                if (parametersSended != null)
                {
                    _fileInfos.Language = parametersSended.LangChoice;
                    _fileInfos.WiFi_Password = parametersSended.Password;
                    _fileInfos.WiFi_SSID = parametersSended.SSID;

                    Serializer<ConfigFileInfos>.Save(_configFilePath, _fileInfos);

                    RaiseOnConfigurationChanged();
                }

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        string page = "";
                        var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                        var file = await folder.GetFileAsync("Configuration\\HTML\\index.html");

                        IBuffer buffer = await FileIO.ReadBufferAsync(file);
                        DataReader reader = DataReader.FromBuffer(buffer);
                        byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                        reader.ReadBytes(fileContent);
                        string readFile = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);

                        foreach (var line in readFile)
                        {
                            page += line;
                        }

                        if (_fileInfos != null)
                        {
                            page = page.Replace("#PASSWORD_VALUE#", _fileInfos.WiFi_Password);

                            if (_fileInfos.Language == "en-US")
                            {
                                page = page.Replace("name='langChoice' value='en-US'", "name='langChoice' value='en-US' checked='checked'");
                            }
                            else if (_fileInfos.Language == "fr-FR")
                            {
                                page = page.Replace("name='langChoice' value='fr-FR'", "name='langChoice' value='fr-FR' checked='checked'");
                            }
                        }

                        page = page.Replace("#TABLE-GENERATOR#", await GenerateSSIDListAsync());

                        byte[] bodyArray = Encoding.UTF8.GetBytes(page);
                        var bodyStream = new MemoryStream(bodyArray);

                        var header = $"HTTP/1.1 200 OK\r\n Content-Length: {bodyStream.Length}\r\nConnection: close\r\n\r\n";
                        byte[] headerArray = Encoding.UTF8.GetBytes(header);

                        await response.WriteAsync(headerArray, 0, headerArray.Length);
                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private async Task<string> GenerateSSIDListAsync()
        {
            string htmlFormat = "<table>{0}</table>";
            string tempContent = string.Empty;

            if (_wifiManager != null)
            {
                try
                {
                    List<string> filterSSIDListStr = new List<string>();
                    List<WiFiInformation> ssidList = await _wifiManager.GetNetworkSSID();
                    List<WiFiInformation> filterSSIDList = new List<WiFiInformation>();

                    foreach (WiFiInformation wi in ssidList)
                    {
                        if (!filterSSIDListStr.Contains(wi.SSID))
                        {
                            filterSSIDListStr.Add(wi.SSID);
                            filterSSIDList.Add(wi);
                        }
                    }

                    filterSSIDList = filterSSIDList.OrderByDescending(y => y.Signal).ToList();

                    int i = 1;
                    foreach (WiFiInformation ssid in filterSSIDList)
                    {
                        string ssidName = string.Empty;
                        if (string.IsNullOrEmpty(ssid.SSID))
                        {
                            ssidName = "Non disponible";
                        }
                        else
                        {
                            ssidName = ssid.SSID;
                        }

                        if (ssidName == _fileInfos.WiFi_SSID)
                        {
                            tempContent += string.Format(@"<tr><td><input type='radio' id='contactChoice{0}' name='ssid' value='{2}' checked='checked'></td><td width='250px'><label class='labelLong' for='contactChoice{3}'>{4}</label></td><td class='tdSignal' {5}></td></tr>", i, i, ssidName, i, ssidName, ssid.SignalColor);
                        }
                        else
                        {
                            tempContent += string.Format(@"<tr><td><input type='radio' id='contactChoice{0}' name='ssid' value='{2}'></td><td width='250px'><label class='labelLong' for='contactChoice{3}'>{4}</label></td><td class='tdSignal' {5}></td></tr>", i, i, ssidName, i, ssidName, ssid.SignalColor);
                        }
                        i++;
                    }
                }
                catch (Exception e)
                {
                    RaiseOnExceptionOccured(e);
                }
            }

            return string.Format(htmlFormat, tempContent);
        }

        private PostParams GetQuery(StringBuilder request)
        {
            string requestFormat = request.ToString();
            PostParams resultParams = null;

            try
            {
                int index = requestFormat.IndexOf("langChoice=");

                if (index != -1)
                {
                    string[] result = requestFormat.Remove(0, index).TrimEnd('\0').Split('&');
                    resultParams = new PostParams();

                    foreach (string s in result)
                    {
                        if (s.Contains("langChoice="))
                        {
                            resultParams.LangChoice = s.Replace("langChoice=", "");
                        }

                        if (s.Contains("ssid="))
                        {
                            resultParams.SSID = s.Replace("ssid=", "").Replace("+", " ");
                        }

                        if (s.Contains("pass="))
                        {
                            resultParams.Password = s.Replace("pass=", "");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }

            return resultParams;
        }

        private void RaiseOnConfigurationChanged()
        {
            OnConfigurationChanged?.Invoke(this, Name, Version);
        }

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

    public class PostParams
    {
        public string LangChoice { set; get; }
        public string SSID { set; get; }
        public string Password { set; get; }
    }
}

using System.Runtime.Serialization;

namespace MU.Configuration
{
    [DataContract]
    public class ConfigFileInfos
    {
        [DataMember]
        public string Language { set; get; }

        [DataMember]
        public string WiFi_SSID { set; get; }

        [DataMember]
        public string WiFi_Password { get; set; }
    }
}

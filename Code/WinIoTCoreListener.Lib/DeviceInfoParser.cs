using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	internal static class DeviceInfoParser
	{
		public static bool TryParse(byte[] buffer, out DeviceInfo deviceInfo)
		{
			deviceInfo = null;

            if (buffer.Length != 526)
			{
				return false;
			}

            string text = Encoding.Unicode.GetString(buffer);

            string[] parts = text.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 7)
			{
				return false;
			}

			string name = parts[0];

			IPAddress ipAddress;
			if (!IPAddress.TryParse(parts[1], out ipAddress))
			{
				return false;
			}

			string macAddressString = parts[2];

			string[] macAddressByteStrings = macAddressString.Split(':');
			if (macAddressByteStrings.Length != 8)
			{
				return false;
			}

            string machineType = parts[4];
            string IOTVersion = parts[5];

			byte[] macAddressBytes = macAddressByteStrings
				.Select(hex => (byte)Convert.ToInt32(hex, 16))
				.ToArray();

			deviceInfo = new DeviceInfo(name, ipAddress, macAddressString, macAddressBytes, machineType, IOTVersion);
			return true;
		}
	}
}
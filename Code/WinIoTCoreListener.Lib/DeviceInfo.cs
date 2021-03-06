using System.Net;

namespace Laserbrain.WinIoTCoreListener.Lib
{
	/// <summary>
	/// Information regarding a WinIotCore device.
	/// </summary>
	public class DeviceInfo
	{
		internal DeviceInfo(
			string machineName,
			IPAddress ipAddress,
			string macAddressString,
			byte[] macAddressBytes,
            string machineType,
            string iotversion)
		{
			MachineName = machineName;
			IpAddress = ipAddress;
			MacAddressString = macAddressString;
			MacAddressBytes = macAddressBytes;
            MachineType = machineType;
            IOTVersion = iotversion;
        }

		/// <summary>
		/// The name of the device.
		/// </summary>
		public string MachineName { get; }

		/// <summary>
		/// The IP address of the device.
		/// </summary>
		public IPAddress IpAddress { get; }

		/// <summary>
		/// The MAC address as a string.
		/// </summary>
		public string MacAddressString { get; }

		/// <summary>
		/// The MAC address as a byte array.
		/// </summary>
		public byte[] MacAddressBytes { get; }

        /// <summary>
        /// The type of the device.
        /// </summary>
        public string MachineType { get; }

        /// <summary>
        /// The version of IOT windows
        /// </summary>
        public string IOTVersion { get; }

        /// <summary>
        /// Equals.
        /// </summary>
        public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj.GetType() == GetType() && Equals((DeviceInfo)obj);
		}

		private bool Equals(DeviceInfo other)
		{
			return
				string.Equals(MacAddressString, other.MacAddressString) &&
				IpAddress.Equals(other.IpAddress) &&
				string.Equals(MachineName, other.MachineName) && (string.Equals(MachineType, other.MachineType));
		}

		/// <summary>
		/// GetHashCode.
		/// </summary>
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		/// <summary>
		/// To string.
		/// </summary>
		public override string ToString()
		{
			return $"\"{MachineName}\" - {MachineType} @ {IpAddress} [{MacAddressString}]";
		}
	}
}
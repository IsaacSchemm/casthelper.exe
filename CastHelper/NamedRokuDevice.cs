using RokuDotNet.Client;
using RokuDotNet.Client.Input;
using RokuDotNet.Client.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public class NamedRokuDevice : IRokuDevice {
		private IRokuDevice _device;
		private string _name;

		public NamedRokuDevice(IRokuDevice device, string name) {
			_device = device ?? throw new ArgumentNullException(nameof(device));
			_name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public string Id => _device.Id;
		public IRokuDeviceInput Input => _device.Input;
		public IRokuDeviceQuery Query => _device.Query;
		public Uri Location => (_device as RokuDevice)?.Location;

		public override string ToString() {
			return _name;
		}
	}
}

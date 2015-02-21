using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace March {
	public class CommandEntry {
		public string CommandName { get; set; }
		public string Usage { get; set; }
		public string Example { get; set; }
		public string HelpMessage { get; set; }
		public string CommandClass { get; set; }
		public string CommandMethod { get; set; }

		[System.ComponentModel.DefaultValue(false)]
		public bool PrivateCommand { get; set; }

		[System.ComponentModel.DefaultValue(false)]
		public bool Disabled { get; set; }

	} // End class CommandEntry
}

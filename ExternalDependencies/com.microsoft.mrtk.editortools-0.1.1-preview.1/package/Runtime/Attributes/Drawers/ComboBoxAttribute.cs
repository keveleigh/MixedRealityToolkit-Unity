using System.Collections.Generic;
using System.Linq;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class ComboBoxAttribute : PropertyTraitAttribute
	{
		public List<string> Options { get; private set; }
		public string OptionsSource { get; private set; }
		public bool AutoUpdate { get; private set; }

		public ComboBoxAttribute(string[] options) : base(ControlPhase, InPhaseOrder)
		{
			Options = options.ToList();
		}

		public ComboBoxAttribute(string optionsSource, bool autoUpdate = true) : base(ControlPhase, InPhaseOrder)
		{
			OptionsSource = optionsSource;
			AutoUpdate = autoUpdate;
		}
	}
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class IconButton : Image
	{
		#region Log Messages

		private const string _missingIconWarning = "Unknown icon '{0}' for IconButton: the icon could not be found";

		#endregion 

		#region Class Names

		public const string Stylesheet = "IconButton.uss";
		public const string UssClassName = "uxtools-icon-button";

		#endregion

		#region Private Members

		private Clickable _clickable;

		#endregion

		#region Public Interface

		public event Action Clicked
		{
			add
			{
				if (_clickable == null)
				{
					_clickable = new Clickable(value);
					this.AddManipulator(_clickable);
				}
				else
				{
					_clickable.clicked += value;
				}
			}
			remove
			{
				if (_clickable != null)
					_clickable.clicked -= value;
			}
		}

		public IconButton() : this(null)
		{
		}

		public IconButton(Action clickEvent)
		{
			Clicked += clickEvent;

			AddToClassList(UssClassName);
			this.AddStyleSheet(Stylesheet);
		}

		public void SetIcon(string iconName)
		{
			if (_icons.TryGetValue(iconName, out var icon))
				SetIcon(icon);
			else
				Debug.LogWarningFormat(_missingIconWarning, iconName);
		}

		public void SetIcon(Icon icon)
		{
			image = icon.Texture;
		}

		#endregion

		#region UXML Support

		private static readonly Dictionary<string, Icon> _icons = new Dictionary<string, Icon>
		{
			{ nameof(Icon.Add), Icon.Add },
			{ nameof(Icon.CustomAdd), Icon.CustomAdd },
			{ nameof(Icon.Remove), Icon.Remove },
			{ nameof(Icon.Inspect), Icon.Inspect },
			{ nameof(Icon.Expanded), Icon.Expanded },
			{ nameof(Icon.Collapsed), Icon.Collapsed },
			{ nameof(Icon.Refresh), Icon.Refresh },
			{ nameof(Icon.Load), Icon.Load },
			{ nameof(Icon.Unload), Icon.Unload },
			{ nameof(Icon.Close), Icon.Close },
			{ nameof(Icon.LeftArrow), Icon.LeftArrow },
			{ nameof(Icon.RightArrow), Icon.RightArrow },
			{ nameof(Icon.Info), Icon.Info },
			{ nameof(Icon.Warning), Icon.Warning },
			{ nameof(Icon.Error), Icon.Error },
			{ nameof(Icon.Settings), Icon.Settings },
			{ nameof(Icon.View), Icon.View },
			{ nameof(Icon.Locked), Icon.Locked },
			{ nameof(Icon.Unlocked), Icon.Unlocked }
		};

		public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits> { }
		public new class UxmlTraits : Image.UxmlTraits
		{
			private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = "icon", use = UxmlAttributeDescription.Use.Required };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);

				var button = ve as IconButton;
				var name = _icon.GetValueFromBag(bag, cc);

				button.SetIcon(name);
			}
		}

		#endregion
	}
}

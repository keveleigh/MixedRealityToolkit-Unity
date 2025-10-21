using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[InitializeOnLoad]
	public class ReferenceSearchWindow : EditorWindow
	{
		#region Class Names

		public const string Stylesheet = "ReferenceSearchStyle.uss";
		public const string UssClassName = "uxtools-reference-search";
		public const string ContentUssClassName = UssClassName + "__content";
		public const string EmptyUssClassName = UssClassName + "__empty";
		public const string TypePickerUssClassNam = UssClassName + "__type-picker";
		public const string SearchButtonUssClassNam = UssClassName + "__search-button";
		public const string ListUssClassName = UssClassName + "__list";
		public const string ListItemUssClassName = ListUssClassName + "__item";
		public const string ListItemIconUssClassName = ListItemUssClassName + "__icon";
		public const string ListItemLabelUssClassName = ListItemUssClassName + "__label";

		#endregion

		#region Window Names

		private const string _windowMenu = "UX Tools/Reference Search";

		#endregion

		#region Icons

		private static readonly Icon _icon = Icon.BuiltIn("UnityEditor.FindDependencies");

		#endregion

		#region Members

		private TypePickerField _picker;
		private IconButton _searchButton;
		private ListView _listView;
		private Label _emptyLabel;

		private List<Component> _searchResults = new List<Component>();

		#endregion

		#region Window Management

		[MenuItem(_windowMenu)]
		public static void Open()
		{
			var window = GetWindow<ReferenceSearchWindow>();
			window.titleContent = new GUIContent("Find References", _icon.Texture);
			window.Show();
		}

		#endregion

		#region ListView Management

		private void OnEnable()
		{
			_searchResults.Clear();

			rootVisualElement.AddStyleSheet(Stylesheet);
			rootVisualElement.AddToClassList(UssClassName);

			var content = new VisualElement();
			content.AddToClassList(ContentUssClassName);

			_picker = new TypePickerField("Type", typeof(Component));
			_picker.RegisterValueChangedCallback(value => UpdateSearchButton());
			_picker.AddToClassList(TypePickerUssClassNam);

			_searchButton = new IconButton(Search);
			_searchButton.SetIcon(Icon.Search);
			_searchButton.AddToClassList(SearchButtonUssClassNam);

			_listView = new ListView(_searchResults, 21, MakeItem, BindItem);
			_listView.AddToClassList(ListUssClassName);
			_listView.selectionType = SelectionType.Single;
			_listView.onItemsChosen += items => Highlight();
			_listView.onSelectionChange += selection => Highlight();
			_listView.Rebuild();

			_emptyLabel = new Label("No references found");
			_emptyLabel.AddToClassList(EmptyUssClassName);

			content.Add(_picker);
			content.Add(_searchButton);
			rootVisualElement.Add(content);
			rootVisualElement.Add(_listView);
			rootVisualElement.Add(_emptyLabel);

			Refresh();
			UpdateSearchButton();
		}

		private void UpdateSearchButton()
		{
			_searchButton.SetEnabled(!string.IsNullOrEmpty(_picker.value));
		}

		private void Search()
		{
			_searchResults.Clear();

			var type = TypeHelper.FindType(_picker.value);
			var objs = FindObjectsOfType(type, true);

			foreach (var obj in objs)
			{
				if (obj is Component component)
					_searchResults.Add(component);
			}

			Refresh();
		}

		private void Refresh()
		{
			var isEmpty = _searchResults.Count == 0;

			_emptyLabel.SetDisplayed(isEmpty && !string.IsNullOrEmpty(_picker.value));
			_listView.SetDisplayed(!isEmpty);
			_listView.Rebuild();
		}

		private VisualElement MakeItem()
		{
			return new ReferenceItem();
		}

		private void BindItem(VisualElement item, int index)
		{
			if (item is ReferenceItem i && _searchResults[index] is Component component)
				i.Bind(component);
		}

		private void Highlight()
		{
			if (_listView.selectedItem is Component component)
				EditorGUIUtility.PingObject(component);
		}

		private class ReferenceItem : VisualElement
		{
			private readonly Image _icon;
			private readonly Label _label;

			public ReferenceItem()
			{
				_icon = new Image();
				_icon.AddToClassList(ListItemIconUssClassName);

				_label = new Label { pickingMode = PickingMode.Ignore };
				_label.AddToClassList(ListItemLabelUssClassName);
				
				Add(_icon);
				Add(_label);
				AddToClassList(ListItemUssClassName);
			}

			public void Bind(Component component)
			{
				_icon.image = AssetPreview.GetMiniThumbnail(component);
				_label.text = component.name;
			}
		}

		#endregion
	}
}
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    public class ListField : BindableElement
    {
        #region Events

        public class ItemAddedEvent : EventBase<ItemAddedEvent>
        {
            public static ItemAddedEvent GetPooled(int index)
            {
                var e = GetPooled();
                e.Index = index;
                return e;
            }

            public int Index { get; private set; }

            public ItemAddedEvent()
            {
                LocalInit();
            }

            protected override void Init()
            {
                base.Init();
                LocalInit();
            }

            void LocalInit()
            {
                Index = 0;
            }
        }

        public class ItemRemovedEvent : EventBase<ItemRemovedEvent>
        {
            public static ItemRemovedEvent GetPooled(int index)
            {
                var e = GetPooled();
                e.Index = index;
                return e;
            }

            public int Index { get; private set; }

            public ItemRemovedEvent()
            {
                LocalInit();
            }

            protected override void Init()
            {
                base.Init();
                LocalInit();
            }

            void LocalInit()
            {
                Index = 0;
            }
        }

        public class ItemReorderedEvent : EventBase<ItemReorderedEvent>
        {
            public static ItemReorderedEvent GetPooled(int from, int to)
            {
                var e = GetPooled();
                e.FromIndex = from;
                e.ToIndex = to;
                return e;
            }

            public int FromIndex { get; private set; }
            public int ToIndex { get; private set; }

            public ItemReorderedEvent()
            {
                LocalInit();
            }

            protected override void Init()
            {
                base.Init();
                LocalInit();
            }

            void LocalInit()
            {
                FromIndex = 0;
                ToIndex = 0;
            }
        }

        public class ItemsChangedEvent : EventBase<ItemsChangedEvent>
        {
        }

        #endregion

        #region Log Messages

        private const string _invalidBindingError = "Invalid binding '{0}' for ListField: property '{1}' is type '{2}' but should be an array";
        private const string _invalidTypeError = "Invalid item type '{0}' for ListField: the item type must be a default constructable class when used with allowDerived = false";
        private const string _failedAddError = "Failed to add item of type '{0}' to the ListField: the item type must be a value type or default constructable class that is compatible with the list";
        private const string _unspecifiedType = "(unspecified)";

        #endregion

        #region Class Names

        public const string Stylesheet = "ListStyle.uss";
        public const string UssClassName = "uxtools-list-field";

        public const string AddDisabledUssClassName = UssClassName + "--add-disabled";
        public const string RemoveDisabledUssClassName = UssClassName + "--remove-disabled";
        public const string ReorderDisabledUssClassName = UssClassName + "--reorder-disabled";
        public const string AddButtonUssClassName = UssClassName + "__add-button";
        public const string RemoveButtonUssClassName = UssClassName + "__remove-button";
        public const string DragHandleUssClassName = UssClassName + "__drag-handle";
        public const string DragHandleBarUssClassName = DragHandleUssClassName + "__bar";
        public const string ItemUssClassName = UssClassName + "__item";
        public const string ItemContentUssClassName = ItemUssClassName + "__content";

        #endregion

        #region Defaults

        public const string AddTooltip = "Add an item to this list";
        public const string RemoveTooltip = "Remove this item from the list";
        public const string ReorderTooltip = "Move this item within the list";

        public const bool DefaultAllowAdd = true;
        public const bool DefaultAllowRemove = true;
        public const bool DefaultAllowReorder = true;

        #endregion

        #region Icons

        private static readonly Icon _addIcon = Icon.Add;
        private static readonly Icon _addReferenceIcon = Icon.CustomAdd;
        private static readonly Icon _removeIcon = Icon.Remove;

        #endregion

        #region Private Members

        private bool _allowAdd = DefaultAllowAdd;
        private bool _allowRemove = DefaultAllowRemove;
        private bool _allowReorder = DefaultAllowReorder;

        private string _label = null;

        private class TypeProvider : PickerProvider<Type> { }
        private TypeProvider _typeProvider;

        private ListView _listView;

        private IconButton _addButton;
        private UQueryState<IconButton> _removeButtons;

        #endregion

        #region Public Interface

        public bool AllowAdd
        {
            get => _allowAdd;
            set { _allowAdd = value; UpdateAddState(); }
        }

        public bool AllowRemove
        {
            get => _allowRemove;
            set { _allowRemove = value; UpdateRemoveState(); }
        }

        public bool AllowReorder
        {
            get => _allowReorder;
            set { _allowReorder = value; UpdateReorderState(); }
        }
        
        public string Label
        {
            get => _label;
            set { _label = value; UpdateLabel(); }
        }

        public IListProxy Proxy { get; private set; }
        public Type ItemType { get; private set; }
        public bool AllowDerived { get; private set; }

        public ListField()
        {
            BuildUi();
        }

        public void SetProxy(IListProxy proxy, Type itemType, bool allowDerived)
        {
            if (itemType != null && !allowDerived && !itemType.IsCreatable())
            {
                Debug.LogWarningFormat(_invalidTypeError, itemType.FullName);
                return;
            }

            Proxy = proxy;
            ItemType = itemType;
            AllowDerived = allowDerived && itemType != null;

            UpdateProxy();
            UpdateItemType();
        }

        #endregion

        #region Ui

        private void BuildUi()
        {
            _listView = new ListView
            {
                makeItem = MakeItem,
                bindItem = BindItem,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                selectionType = SelectionType.None,
                showAddRemoveFooter = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBorder = true,
                showFoldoutHeader = true,
                showBoundCollectionSize = true
            };

            _listView.itemIndexChanged += OnReorderItem;

            AddToClassList(UssClassName);
            this.AddStyleSheet(Stylesheet);

            _addButton = CreateButton(_addIcon.Texture, AddTooltip, AddButtonUssClassName, DoAdd);
            _removeButtons = _listView.Query<IconButton>(className: RemoveButtonUssClassName).Build();

            _typeProvider = ScriptableObject.CreateInstance<TypeProvider>();

            UpdateLabel();
            UpdateAddState();
            UpdateRemoveState();
            UpdateReorderState();

            _listView.Q(className: ListView.arraySizeFieldUssClassName).Add(_addButton);
            Add(_listView);
        }

        private void UpdateLabel()
        {
            _listView.headerTitle = _label;
        }

        private void UpdateAddState()
        {
            EnableInClassList(AddDisabledUssClassName, !_allowAdd);
        }

        private void UpdateRemoveState()
        {
            EnableInClassList(RemoveDisabledUssClassName, !_allowRemove);
        }

        private void UpdateReorderState()
        {
            // Unity currently has a bug where you can reorder things regardless if this is false or not
            _listView.reorderable = _allowReorder;

            EnableInClassList(ReorderDisabledUssClassName, !_allowReorder);
        }

        private void UpdateProxy()
        {
            UpdateItemsWithoutNotify();
        }

        private void UpdateItemType()
        {
            if (ItemType != null)
            {
                var types = TypeHelper.GetTypeList(ItemType, false);
                _typeProvider.Setup(ItemType.Name, types.Paths, types.Types, GetIcon, AddItem);
                _addButton.SetIcon(_addReferenceIcon);
            }
            else
            {
                _addButton.SetIcon(_addIcon);
            }
        }

        private void UpdateItems()
        {
            UpdateItemsWithoutNotify();

            using (var e = ItemsChangedEvent.GetPooled())
            {
                e.target = this;
                SendEvent(e);
            }
        }

        private void UpdateItemsWithoutNotify()
        {
            _removeButtons.ForEach(button =>
            {
                var index = GetIndex(button.parent);
                var removable = Proxy.CanRemove(index);

                button.SetEnabled(removable);
            });

            var validAdd = Proxy.CanAdd();
            var validType = AllowDerived || Proxy.CanAdd(ItemType);

            _addButton.SetEnabled(validAdd && validType);
        }

        private VisualElement MakeItem()
        {
            var item = new VisualElement();
            item.AddToClassList(ItemUssClassName);

            var dragHandle = new VisualElement { tooltip = ReorderTooltip };
            dragHandle.AddToClassList(DragHandleUssClassName);

            var topBar = new VisualElement();
            topBar.AddToClassList(DragHandleBarUssClassName);

            var bottomBar = new VisualElement();
            bottomBar.AddToClassList(DragHandleBarUssClassName);

            dragHandle.Add(topBar);
            dragHandle.Add(bottomBar);

            item.Add(dragHandle);

            return item;
        }

        private void BindItem(VisualElement item, int index)
        {
            // We do this instead of using Unbind because ListView has a bug and they aren't exactly mirrored
            if (item.childCount > 1)
            {
                item.RemoveAt(2);
                item.RemoveAt(1);
            }

            var content = Proxy.CreateElement(index);
            content.AddToClassList(ItemContentUssClassName);

            var remove = CreateButton(_removeIcon.Texture, RemoveTooltip, RemoveButtonUssClassName, () => RemoveItem(index));

            item.Add(content);
            item.Add(remove);
        }

        private IconButton CreateButton(Texture icon, string tooltip, string ussClassName, Action action)
        {
            var button = new IconButton(action) { image = icon, tooltip = tooltip };
            button.AddToClassList(ussClassName);

            return button;
        }

        #endregion

        #region Binding

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);

            if (this.TryGetPropertyBindEvent(evt, out var property))
            {
                var arrayProperty = property.FindPropertyRelative("Array.size");

                if (arrayProperty != null)
                {
                    var sizeBinding = new ChangeTrigger<int>(null, (_, oldSize, newSize) => UpdateItems());
                    sizeBinding.Watch(arrayProperty);

                    Add(sizeBinding);
                }
                else
                {
                    Debug.LogErrorFormat(_invalidBindingError, bindingPath, property.propertyPath, property.propertyType);
                }

                _listView.BindProperty(property);
                Label = _label ?? property.displayName;

                evt.StopPropagation();
            }
        }

        #endregion

        #region Item Management

        private int GetIndex(VisualElement element)
        {
            return element.parent.IndexOf(element);
        }

        private void DoAdd()
        {
            if (AllowDerived)
                SelectType();
            else
                AddItem(ItemType);
        }

        private void SelectType()
        {
            if (_allowAdd && Proxy.CanAdd())
            {
                var position = new Vector2(_addButton.worldBound.center.x, _addButton.worldBound.yMax + _addButton.worldBound.height * 0.5f);
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position)), _typeProvider);
            }
        }

        private Texture GetIcon(Type type)
        {
            return AssetPreview.GetMiniTypeThumbnail(type);
        }

        private void AddItem(Type selected)
        {
            if (_allowAdd && Proxy.CanAdd() && Proxy.CanAdd(selected))
            {
                if (Proxy.AddItem(selected))
                {
                    UpdateItemsWithoutNotify();

                    using (var e = ItemAddedEvent.GetPooled(Proxy.Count - 1))
                    {
                        e.target = this;
                        SendEvent(e);
                    }
                }
                else
                {
                    Debug.LogErrorFormat(_failedAddError, selected != null ? selected.FullName : _unspecifiedType);
                }
            }
        }

        private void RemoveItem(int index)
        {
            if (_allowRemove && Proxy.CanRemove(index))
            {
                Proxy.RemoveItem(index);
                UpdateItemsWithoutNotify();

                using (var e = ItemRemovedEvent.GetPooled(index))
                {
                    e.target = this;
                    SendEvent(e);
                }
            }
        }

        private void OnReorderItem(int from, int to)
        {
            UpdateItemsWithoutNotify();

            using (var e = ItemReorderedEvent.GetPooled(from, to))
            {
                e.target = this;
                SendEvent(e);
            }
        }

        #endregion

        #region UXML Support

        public new class UxmlFactory : UxmlFactory<ListField, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new() { name = "label" };
            private readonly UxmlBoolAttributeDescription _allowAdd = new() { name = "allow-add", defaultValue = DefaultAllowAdd };
            private readonly UxmlBoolAttributeDescription _allowRemove = new() { name = "allow-remove", defaultValue = DefaultAllowRemove };
            private readonly UxmlBoolAttributeDescription _allowReorder = new() { name = "allow-reorder", defaultValue = DefaultAllowReorder };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var list = ve as ListField;

                list.Label = _label.GetValueFromBag(bag, cc);
                list.AllowAdd = _allowAdd.GetValueFromBag(bag, cc);
                list.AllowRemove = _allowRemove.GetValueFromBag(bag, cc);
                list.AllowReorder = _allowReorder.GetValueFromBag(bag, cc);
            }
        }

        #endregion
    }
}

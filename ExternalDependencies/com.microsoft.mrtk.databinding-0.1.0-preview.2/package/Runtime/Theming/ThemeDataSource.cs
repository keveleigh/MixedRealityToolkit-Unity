using Microsoft.MixedReality.Toolkit.DataBinding;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    public class ThemeDataSource : BaseDataSource
    {
        public readonly static string SourceNamePrefix = "Theme"; 

        private List<int> availableIds = null;
        private List<ThemeRecord> registeredRecords = null;
        private List<ThemeItemCollection> activeDefinitions = null;

        private Dictionary<string, int> itemNameToIdLookup = null;
        private Dictionary<int, ThemeItem> registeredItemLookup = null;

        public ThemeDataSource() : base(SourceNamePrefix) 
        {
            availableIds = new List<int>(200);
            registeredRecords = new List<ThemeRecord>(50);
            activeDefinitions = new List<ThemeItemCollection>(30);
            itemNameToIdLookup = new Dictionary<string, int>(100);
            registeredItemLookup = new Dictionary<int, ThemeItem>(200);
        }

        public override void Initialize()
        {
            base.Initialize();

            if (Application.isPlaying)
            {
                IsReadyForSubscribers = true;
            }
        }

        public void AddItems(ThemeItemCollection items)
        {
            items.Initialize();
            for (int i = 0; i < items.ThemeItems.Count; i++)
            {
                ThemeItem item = items.ThemeItems[i];
                if (item != null)
                {
                    availableIds.Add(item.Id);
                    itemNameToIdLookup[item.Name] = item.Id;
                    registeredItemLookup[item.Id] = item;
                    TryRegisterAsSource(item.Record);
                    OnItemUpdated(item.Name);
                }
            }

            items.ThemeValuesChanged += OnThemeValuesChanged;
        }

        public void RemoveItems(ThemeItemCollection items)
        {
            for (int i = 0; i < items.ThemeItems.Count; i++)
            {
                ThemeItem item = items.ThemeItems[i];
                if (item != null && registeredItemLookup.TryGetValue(item.Id, out ThemeItem cachedItem) && item == cachedItem)
                {
                    availableIds.Remove(item.Id);
                    itemNameToIdLookup.Remove(item.Name);
                    registeredItemLookup.Remove(item.Id);
                }
            }

            items.ThemeValuesChanged -= OnThemeValuesChanged;
        }

        public void RefreshAllItems()
        {
            foreach (int id in availableIds)
            {
                OnItemUpdated(registeredItemLookup[id].Name);
            }
        }

        public override bool TryGetValue<T>(string itemName, out T itemValue)
        {
            if (itemNameToIdLookup.TryGetValue(itemName, out int id) && registeredItemLookup.TryGetValue(id, out ThemeItem item) &&
                typeof(T) == typeof(ThemeItemValue))
            {
                ThemeType themeType = ThemeManager.Instance.CurrentThemeType;
                if (themeType == ThemeType.Dark && item.DarkValue is T darkVal)
                {
                    itemValue = darkVal;
                    return true;
                }
                else if (themeType == ThemeType.Light && item.LightValue is T lightVal)
                {
                    itemValue = lightVal;
                    return true;
                }
                else if (themeType == ThemeType.HighContrast && item.HighContrast is T hcVal)
                {
                    itemValue = hcVal;
                    return true;
                }
            }
            itemValue = default(T);
            return false;
        }

        public override bool TryGetValue<T>(int itemId, out T itemValue)
        {
            if (registeredItemLookup.TryGetValue(itemId, out ThemeItem item) &&
                typeof(T) == typeof(ThemeItemValue))
            {
                ThemeType themeType = ThemeManager.Instance.CurrentThemeType;
                if (themeType == ThemeType.Dark && item.DarkValue is T darkVal)
                {
                    itemValue = darkVal;
                    return true;
                }
                else if (themeType == ThemeType.Light && item.LightValue is T lightVal)
                {
                    itemValue = lightVal;
                    return true;
                }
                else if (themeType == ThemeType.HighContrast && item.HighContrast is T hcVal)
                {
                    itemValue = hcVal;
                    return true;
                }
            }
            itemValue = default(T);
            return false;
        }

        public override bool TryGetIdForName(string name, out int id)
        {
            return itemNameToIdLookup.TryGetValue(name, out id);
        }

        public override bool TryGetNameForId(int id, out string name)
        {
            name = string.Empty;
            if (registeredItemLookup.TryGetValue(id, out ThemeItem item))
            {
                name = item.Name;
                return true;
            }
            return false;
        }

        private void TryRegisterAsSource(ThemeRecord record)
        {
            if (!registeredRecords.Contains(record) && !record.GetName(out string recordName))
            {
                registeredRecords.Add(record);
            }
        }

        private void OnThemeValuesChanged(ThemeItemCollection collection)
        {
            foreach (ThemeItem item in collection.ThemeItems)
            {
                OnItemUpdated(item.Name);
            }
        }

        public override bool TrySetValue<T>(string itemName, T itemValue)
        {
            // theming does not currently support two-way binding
            return false;
        }

        public override bool TrySetValue<T>(int itemId, T itemValue)
        {
            // theming does not currently support two-way binding
            return false;
        }

        public override bool TryGetValueAtIndex<T>(string listName, int index, out T itemValue)
        {
            // theming does not need list support at the moment
            itemValue= default(T);
            return false;
        }

        public override bool TryGetValueAtIndex<T>(int listId, int index, out T itemValue)
        {
            // theming does not need list support at the moment
            itemValue = default(T);
            return false;
        }

        protected override string GetItemNameForCollectionItemAtIndex(string listName, int itemIndex)
        {
            return string.Empty;
        }

        protected override BaseDataSourceRecord CreateRecord(string recordPath)
        {
            // not needed for theming
            return null;
        }

        protected override bool TryLoadRecord(string recordPath, out BaseDataSourceRecord record)
        {
            // not needed
            record = null;
            return true;
        }

        public override void UpdateRecord(bool force = false, bool clean = false)
        {
            // not needed here
        }

        public override bool TryGetCollectionCount(string collectionName, out int count)
        {
            // not needed here
            count = 0;
            return false;
        }

    }
}

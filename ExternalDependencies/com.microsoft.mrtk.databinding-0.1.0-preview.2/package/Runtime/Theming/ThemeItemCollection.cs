using Microsoft.MixedReality.Toolkit.EditorTools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [CreateAssetMenu(fileName ="Theme Item Collection", menuName = "UXTools/Theming/Theme Item Collection", order = 0)]
    public class ThemeItemCollection : ScriptableObject
    {
        [List(AddCallback = nameof(Editor_OnItemAdded))]
        [Inline(InlineDisplayMode.MembersOnly)]
        [SerializeField]
        private SerializedList<ThemeItem> themeItems = null;

        private Dictionary<string, int> nameToIdLookup = null;
        private Dictionary<int, ThemeItem> idBasedLookup = null;

        private ThemeType activeThemeType = ThemeType.Dark;

        public List<ThemeItem> ThemeItems => themeItems.List;

        public event Action<ThemeItemCollection> ThemeValuesChanged = null;

        public void Initialize()
        {
            // set the theme type on each item so they can properly resolve their data item name
            nameToIdLookup ??= new Dictionary<string, int>(themeItems.Count);
            idBasedLookup ??= new Dictionary<int, ThemeItem>(themeItems.Count);

            foreach (ThemeItem item in themeItems)
            {
                nameToIdLookup[item.Name] = item.Id;
                idBasedLookup[item.Id] = item;
            }
        }

        public bool TryGetIdForName(string name, out int id)
        {
            return nameToIdLookup.TryGetValue(name, out id);
        }

        public bool TryGetNameForId(int id, out string name)
        {
            name = string.Empty;
            if (idBasedLookup.TryGetValue(id, out ThemeItem item))
            {
                name = item.Name;
            }
            return !string.IsNullOrWhiteSpace(name);
        }

        public bool RetrieveValue<T>(string itemName, out T itemValue)
        {
            if (TryGetIdForName(itemName, out int id) && idBasedLookup.TryGetValue(id, out ThemeItem item))
            {
                if (activeThemeType == ThemeType.Dark && item.DarkValue is T darkVal)
                {
                    itemValue = darkVal;
                    return true;
                }
                else if (activeThemeType == ThemeType.Light && item.LightValue is T lightVal)
                {
                    itemValue = lightVal;
                    return true;
                }
                else if (activeThemeType == ThemeType.HighContrast && item.HighContrast is T hcVal)
                {
                    itemValue = hcVal;
                    return true;
                }
            }

            itemValue = default(T);
            return false;
        }

        private void Editor_OnItemAdded()
        {
            foreach (var item in themeItems)
            {
                 item.DoFirstNameRetrieval();
            }
        }
    }
}

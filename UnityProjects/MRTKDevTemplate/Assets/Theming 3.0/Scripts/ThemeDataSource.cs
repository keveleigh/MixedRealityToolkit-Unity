using UnityEngine;
using UnityEngine.Events;

namespace MixedReality.Toolkit.Themes
{
    [CreateAssetMenu(fileName = "Theme Data Source", menuName = "MRTK/Theming/Theme Data Source", order = 0)]
    public class ThemeDataSource : ScriptableObject
    {
        [field: SerializeField]
        public Theme ActiveTheme { get; private set; }

        [field: SerializeField]
        private UnityEvent<ThemeDataSource> onThemeChanged = new();

        public void AddListener(UnityAction<ThemeDataSource> action)
        {
            onThemeChanged.AddListener(action);
            action.Invoke(this);
        }

        public void RemoveListener(UnityAction<ThemeDataSource> action)
        {
            onThemeChanged.RemoveListener(action);
        }

        public bool TryGetValue<T>(string itemName, out T itemValue)
        {
            foreach (var themeItem in ActiveTheme.ThemeItems)
            {
                if (themeItem.ItemName == itemName && themeItem.ThemeItemData is T themeItemData)
                {
                    itemValue = themeItemData;
                    return true;
                }
            }

                //    if (registeredItemLookup.TryGetValue(itemId, out ThemeItem item) &&
                //        typeof(T) == typeof(ThemeItemValue))
                //    {
                //        ThemeType themeType = ThemeManager.Instance.CurrentThemeType;
                //        if (themeType == ThemeType.Dark && item.DarkValue is T darkVal)
                //        {
                //            itemValue = darkVal;
                //            return true;
                //        }
                //        else if (themeType == ThemeType.Light && item.LightValue is T lightVal)
                //        {
                //            itemValue = lightVal;
                //            return true;
                //        }
                //        else if (themeType == ThemeType.HighContrast && item.HighContrast is T hcVal)
                //        {
                //            itemValue = hcVal;
                //            return true;
                //        }
                //    }
                itemValue = default(T);
            return false;
        }

        public bool TrySetActiveTheme(Theme newTheme)
        {
            if (newTheme.ThemeDefinition != ActiveTheme.ThemeDefinition)
            {
                Debug.LogError($"New theme's definition ({newTheme.ThemeDefinition.name}) does not match this data source's active definition ({ActiveTheme.ThemeDefinition.name})");
                return false;
            }

            ActiveTheme = newTheme;
            onThemeChanged.Invoke(this);
            return true;
        }
    }
}

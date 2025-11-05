using Microsoft.MixedReality.Toolkit.EditorTools;
using System;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    public enum ThemeType
    {
        Dark,
        Light,
        HighContrast,
        Custom // deprecated, but needs left around for a little bit longer
    }

    /// <summary>
    /// Manages what themes are available and handles changing theming at runtime
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        public static bool IsAvailable => Instance != null;
        public static ThemeManager Instance { get; private set; } = null;

        protected ThemeDefinition loadedTheme = null;
        protected ThemeType currentThemeType = ThemeType.Dark;
        public ThemeType CurrentThemeType => currentThemeType;
        public event Action CurrentThemeChanged = null;

        private ThemeDataSource themeDataSource = null;

        protected virtual void Awake()
        {
            Instance = this;

            themeDataSource = new ThemeDataSource();
            themeDataSource.Initialize();
        }

        public void LoadThemeDefinition(ThemeDefinition theme)
        {
            loadedTheme = theme;
            foreach (ThemeItemCollection collection in theme.themeDefinitionList)
            {
                themeDataSource.AddItems(collection);
            }
        }

        public virtual void LoadThemeType(ThemeType themeType)
        {
            if (currentThemeType != themeType)
            {
                currentThemeType = themeType;
                themeDataSource.RefreshAllItems();
                CurrentThemeChanged?.Invoke();
            }
        }

        public void UnloadThemeDefinition(ThemeDefinition theme)
        {
            foreach (ThemeItemCollection collection in theme.themeDefinitionList)
            {
                themeDataSource.RemoveItems(collection);
            }
        }

        [MethodButton(Label ="Load Dark Theme")]
        private void Editor_SwapToDark()
        {
            LoadThemeType(ThemeType.Dark);
        }

        [MethodButton(Label = "Load Light Theme")]
        private void Editor_SwapToLight()
        {
            LoadThemeType(ThemeType.Light);
        }

        [MethodButton(Label = "Load High Contrast Theme")]
        private void Editor_SwapToHighContrast()
        {
            LoadThemeType(ThemeType.HighContrast);
        }
    }
}

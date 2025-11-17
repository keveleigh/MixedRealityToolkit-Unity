using System;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public abstract class BaseThemeBinder<T> : BaseThemeBinder
    {
        protected abstract void Apply(BaseThemeItemData<T> themeItemData);

        protected override void OnThemeChanged(ThemeDataSource themeDataSource)
        {
            if (themeDataSource.TryGetValue(ThemeDefinitionItemName, out BaseThemeItemData<T> value))
            {
                Apply(value);
            }
        }
    }

    public abstract class BaseThemeBinder : IBinder
    {
        [field: SerializeField]
        public ThemeDefinition ThemeDefinition { get; set; }

        [field: SerializeField, HideInInspector]
        public string ThemeDefinitionItemName { get; set; }

        protected abstract void OnThemeChanged(ThemeDataSource themeDataSource);

        void IBinder.Subscribe(ThemeDataSource themeDataSource)
        {
            if (themeDataSource != null)
            {
                themeDataSource.AddListener(OnThemeChanged);
            }
        }

        void IBinder.Unsubscribe(ThemeDataSource themeDataSource)
        {
            if (themeDataSource != null)
            {
                themeDataSource.RemoveListener(OnThemeChanged);
            }
        }
    }
}

// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UIElements;

namespace MixedReality.Toolkit.Theming
{
    public abstract class BaseThemeBinder<T, K> : IBinder
    {
        [field: SerializeReference]
        protected K Target { get; private set; }

        [field: SerializeField]
        public ThemeDefinition ThemeDefinition { get; private set; }

        [field: SerializeField, HideInInspector]
        public string ThemeDefinitionItemName { get; private set; }

        protected abstract void Apply(BaseThemeItemData<T> themeItemData);

        protected void OnThemeChanged(ChangeEvent<Theme> changeEvent)
        {
            if (changeEvent.newValue.TryGetItemData(ThemeDefinitionItemName, out BaseThemeItemData<T> value))
            {
                Apply(value);
            }
        }

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

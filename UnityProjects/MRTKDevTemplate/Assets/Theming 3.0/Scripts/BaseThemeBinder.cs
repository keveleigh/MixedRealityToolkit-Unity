using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public abstract class BaseThemeBinder<T> : IBinder where T : BaseThemeItemData
    {
        [field: SerializeField]
        public ThemeDefinition ThemeDefinition { get; set; }

        [field: SerializeReference]
        public T ThemedItemData { get; set; }

        protected abstract void Apply();

        void IBinder.Subscribe()
        {
            throw new System.NotImplementedException();
        }

        void IBinder.Unsubscribe()
        {
            throw new System.NotImplementedException();
        }
    }
}

// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace MixedReality.Toolkit.Theming
{
    [CreateAssetMenu(fileName = "Theme Data Source", menuName = "MRTK/Theming/Theme Data Source", order = 0)]
    public class ThemeDataSource : ScriptableObject, IBindable, INotifyValueChanged<Theme>, IEventHandler
    {
        [SerializeField]
        private Theme activeTheme;

        [SerializeField]
        private UnityEvent<ChangeEvent<Theme>> onThemeChanged = new();

        #region IBindable

        IBinding IBindable.binding { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        string IBindable.bindingPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        #endregion IBindable

        #region INotifyValueChanged<Theme>

        public Theme value
        {
            get => activeTheme;
            set
            {
                if (value.ThemeDefinition != activeTheme.ThemeDefinition)
                {
                    Debug.LogError($"New theme's definition ({value.ThemeDefinition.name}) does not match this data source's active definition ({activeTheme.ThemeDefinition.name})");
                }

                using (ChangeEvent<Theme> changeEvent = ChangeEvent<Theme>.GetPooled(activeTheme, value))
                {
                    changeEvent.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(changeEvent);
                }
            }
        }

        public void SetValueWithoutNotify(Theme newValue)
        {
            activeTheme = newValue;
        }

        #endregion INotifyValueChanged<Theme>

        #region IEventHandler

        public void SendEvent(EventBase e)
        {
            if (e is ChangeEvent<Theme> changeEvent)
            {
                onThemeChanged.Invoke(changeEvent);
            }
        }

        void IEventHandler.HandleEvent(EventBase evt) { }
        bool IEventHandler.HasTrickleDownHandlers() => false;
        bool IEventHandler.HasBubbleUpHandlers() => false;

        #endregion IEventHandler

        public void AddListener(UnityAction<ChangeEvent<Theme>> action)
        {
            onThemeChanged.AddListener(action);
            using ChangeEvent<Theme> changeEvent = ChangeEvent<Theme>.GetPooled(null, activeTheme);
            changeEvent.target = this;
            SendEvent(changeEvent);
        }

        public void RemoveListener(UnityAction<ChangeEvent<Theme>> action)
        {
            onThemeChanged.RemoveListener(action);
        }
    }
}

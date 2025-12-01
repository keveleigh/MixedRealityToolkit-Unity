// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    public class DataBinding : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The theme data source manager.")]
        private ThemeDataSource themeDataSource;

        [SerializeReference, InterfaceSelector]
        [Tooltip("The list of bound theme entries.")]
        private IBinder[] binders;

        protected void OnEnable()
        {
            foreach (IBinder binder in binders)
            {
                binder.Subscribe(themeDataSource);
            }
        }

        protected void OnDisable()
        {
            foreach (IBinder binder in binders)
            {
                binder.Unsubscribe(themeDataSource);
            }
        }
    }
}

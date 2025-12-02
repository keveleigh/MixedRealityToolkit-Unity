// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [CreateAssetMenu(fileName = "Theme", menuName = "MRTK/Theming/Theme", order = 0)]
    public class Theme : ScriptableObject
    {
        [SerializeField]
        private ThemeDefinition themeDefinition;

        public ThemeDefinition ThemeDefinition => themeDefinition;

        [SerializeReference]
        [Tooltip("The pose source representing the poke pose")]
        public List<ThemeItem> ThemeItems;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemName"></param>
        /// <param name="itemValue"></param>
        /// <returns></returns>
        public bool TryGetItemData<T>(string itemName, out T itemValue)
        {
            foreach (var themeItem in ThemeItems)
            {
                if (themeItem.Name == itemName && themeItem.Data is T themeItemData)
                {
                    itemValue = themeItemData;
                    return true;
                }
            }

            itemValue = default;
            return false;
        }

        [Serializable]
        public class ThemeItem
        {
            [field: SerializeField, HideInInspector]
            public string Name { get; private set; }

            [field: SerializeReference]
            public object Data { get; private set; }

            public ThemeItem(string name, object data)
            {
                Name = name;
                Data = data;
            }
        }
    }
}

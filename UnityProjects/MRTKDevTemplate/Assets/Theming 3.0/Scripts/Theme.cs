using System;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
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

        [Serializable]
        public class ThemeItem
        {
            [field: SerializeField]
            public string ItemName { get; set; }

            [field: SerializeReference]
            public object ThemeItemData { get; set; }
        }
    }
}

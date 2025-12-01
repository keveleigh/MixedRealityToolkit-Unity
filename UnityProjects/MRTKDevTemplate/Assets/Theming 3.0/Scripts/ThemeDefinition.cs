using MixedReality.Toolkit;
using MixedReality.Toolkit.Themes;
// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme Definition", menuName = "MRTK/Theming/Theme Definition", order = 0)]
public class ThemeDefinition : ScriptableObject
{
    [field: SerializeField]
    [Tooltip("The pose source representing the poke pose")]
    public ThemeDefinitionItem[] ThemeDefinitionItems { get; private set; }

    [Serializable]
    public class ThemeDefinitionItem
    {
        [field: SerializeField]
        public string ItemName { get; set; }

        [field: SerializeField, Extends(typeof(BaseThemeItemData<>), TypeGrouping.ByNamespaceFlat, AllowGenericTypeDefinition = true)]
        public SystemType ThemeItemData { get; set; }
    }
}

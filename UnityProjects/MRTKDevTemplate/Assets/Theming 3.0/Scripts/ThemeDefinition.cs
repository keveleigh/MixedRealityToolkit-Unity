using MixedReality.Toolkit;
using MixedReality.Toolkit.Themes;
using System;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Theme Definition", menuName = "MRTK/Theming/Theme Definition", order = 0)]
public class ThemeDefinition : ScriptableObject, IBindable
{
    [field: SerializeReference, InterfaceSelector]
    [Tooltip("The pose source representing the poke pose")]
    public IBinder[] Binders { get; set; }

    //[field: SerializeReference, InterfaceSelector]
    //[Tooltip("The pose source representing the poke pose")]
    //public BaseThemeItemData[] ThemeItems { get; private set; }

    [field: SerializeField]
    [Tooltip("The pose source representing the poke pose")]
    public ThemeDefinitionItem[] ThemeDefinitionItems { get; private set; }

    IBinding IBindable.binding { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    string IBindable.bindingPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    [Serializable]
    public class ThemeDefinitionItem
    {
        [field: SerializeField]
        public string ItemName { get; set; }

        [field: SerializeField, Extends(typeof(BaseThemeItemData<>), TypeGrouping.ByNamespaceFlat, AllowGenericTypeDefinition = true)]
        public SystemType ThemeItemData { get; set; }
    }
}

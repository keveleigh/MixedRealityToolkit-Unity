using MixedReality.Toolkit;
using MixedReality.Toolkit.Themes;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Theme Definition", menuName = "MRTK/Theming/Theme Definition", order = 0)]
public class ThemeDefinition : ScriptableObject, IBindable
{
    [field: SerializeField, Extends(typeof(BaseThemeItemData), TypeGrouping.ByNamespaceFlat)]
    [Tooltip("The list of subsystems intended to be started at runtime.")]
    public SystemType[] ThemeItemDefinitions { get; set; }

    [field: SerializeReference, InterfaceSelector]
    [Tooltip("The pose source representing the poke pose")]
    public IBinder[] Binders { get; set; }

    [field: SerializeReference, InterfaceSelector]
    [Tooltip("The pose source representing the poke pose")]
    public BaseThemeItemData[] ThemeItems { get; set; }

    IBinding IBindable.binding { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    string IBindable.bindingPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}

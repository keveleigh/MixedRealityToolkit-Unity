using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    [CreateAssetMenu(fileName = "Theme", menuName = "MRTK/Theming/Theme", order = 0)]
    public class Theme : ScriptableObject
    {
        [SerializeField]
        private ThemeDefinition themeDefinition;

        [field: SerializeReference, InterfaceSelector]
        [Tooltip("The pose source representing the poke pose")]
        public BaseThemeItemData[] ThemeItems { get; set; }
    }
}

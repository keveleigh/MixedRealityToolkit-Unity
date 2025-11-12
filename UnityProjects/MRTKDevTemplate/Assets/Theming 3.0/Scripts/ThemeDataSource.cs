using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    [CreateAssetMenu(fileName = "Theme Data Source", menuName = "MRTK/Theming/Theme Data Source", order = 0)]
    public class ThemeDataSource : ScriptableObject
    {
        [SerializeField]
        private ThemeDefinition themeDefinition;

        [SerializeField]
        private Theme activeTheme;
    }
}

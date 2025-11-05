using Microsoft.MixedReality.Toolkit.EditorTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [CreateAssetMenu(fileName = "Theme Definition", menuName = "UXTools/Theming/Theme Definition", order = 0)]
    public class ThemeDefinition : ScriptableObject
    {
        public List<ThemeItemCollection> themeDefinitionList = null;

        [MethodButton(Label = "Load Theme", Tooltip = "")]
        public void Editor_LoadTheme()
        {
            if (Application.isPlaying)
            {
                ThemeManager.Instance.LoadThemeDefinition(this);
            }
        }

        [MethodButton(Label = "Swap to Dark", Tooltip = "")]
        private void SwapToDark()
        {
            ThemeManager.Instance.LoadThemeType(ThemeType.Dark);
        }

        [MethodButton(Label = "Swap to Light", Tooltip = "")]
        private void SwapToLight()
        {
            ThemeManager.Instance.LoadThemeType(ThemeType.Light);
        }

        [MethodButton(Label = "Swap to High Contrast", Tooltip = "")]
        private void SwapSwapToHighContrast()
        {
            ThemeManager.Instance.LoadThemeType(ThemeType.HighContrast);
        }
    }
}

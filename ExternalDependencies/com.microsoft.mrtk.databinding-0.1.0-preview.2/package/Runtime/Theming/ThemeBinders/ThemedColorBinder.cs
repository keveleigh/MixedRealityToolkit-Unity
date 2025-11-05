using Microsoft.MixedReality.Toolkit.EditorTools;
using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedColorBinder : BaseThemeBinder<Color>
    {
        [DrawIf(nameof(Editor_IsRecordValid))]
        public Graphic ColorSwapTarget = null;
        [DrawIf(nameof(Editor_IsRecordValid))]
        public bool AlsoSetAlpha = false;

        protected override void PropagateEffect(ThemeItemValue themeValue)
        {
            if (ColorSwapTarget != null)
            {
                if (!AlsoSetAlpha)
                {
                    ColorSwapTarget.color = new Color(themeValue.ColorValue.r, themeValue.ColorValue.g, themeValue.ColorValue.b, ColorSwapTarget.color.a);
                }
                else
                {
                    ColorSwapTarget.color = themeValue.ColorValue;
                }
            }
        }
    }
}

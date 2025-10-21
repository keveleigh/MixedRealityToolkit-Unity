using Microsoft.MixedReality.Toolkit.EditorTools;
using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.MixedReality.Toolkit.Theming
{
    public class ThemedColorBinder : BaseThemeBinder<Color>
    {
        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
        public Graphic ColorSwapTarget = null;
        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
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

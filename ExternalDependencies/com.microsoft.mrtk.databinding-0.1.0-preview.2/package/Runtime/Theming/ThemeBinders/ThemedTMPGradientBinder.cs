using Microsoft.MixedReality.Toolkit.EditorTools;
using TMPro;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedTMPGradientBinder : BaseThemeBinder<TMP_ColorGradient>
    {
        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
        public TMP_Text Text;

        protected override void PropagateEffect(ThemeItemValue themeValue)
        {
            if (Text != null)
            {
                Text.enableVertexGradient = true;
                Text.colorGradientPreset = themeValue.TmpGradientValue;
            }
        }
    }
}

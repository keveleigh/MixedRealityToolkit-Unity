using MixedReality.Toolkit;
using TMPro;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedTMPGradientBinder : BaseThemeBinder<TMP_ColorGradient>
    {
        [DrawIf(nameof(Editor_IsRecordValid))]
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

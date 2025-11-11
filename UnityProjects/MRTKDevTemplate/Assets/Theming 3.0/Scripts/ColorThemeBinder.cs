using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Themes
{
    public class ColorThemeBinder : BaseThemeBinder<ColorThemeItemData>
    {
        [field: SerializeField]
        private Graphic Target { get; set; }

        protected override void Apply()
        {
            Target.color = ThemedItemData.color;
        }
    }
}

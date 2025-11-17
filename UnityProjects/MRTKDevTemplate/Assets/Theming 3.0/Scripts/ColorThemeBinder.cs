using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Themes
{
    public class ColorThemeBinder : BaseThemeBinder<Color>
    {
        [field: SerializeField]
        private Graphic Target { get; set; }

        protected override void Apply(BaseThemeItemData<Color> themeItemData)
        {
            Target.color = themeItemData.Value;
        }
    }
}

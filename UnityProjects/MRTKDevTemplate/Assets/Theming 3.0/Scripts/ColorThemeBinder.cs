using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Themes
{
    public class ColorThemeBinder : BaseThemeBinder<ColorThemeItemData>
    {
        //    [field: SerializeField]
        //    private PropertyPath PropertyPath { get; set; }

        //    [field: SerializeField]
        //    private string PropertyPathStr { get; set; }

        [field: SerializeField]
        private Graphic ColorTarget { get; set; }

        protected override void Apply()
        {
            ColorTarget.color = ThemedItemData.color;
        }
    }
}

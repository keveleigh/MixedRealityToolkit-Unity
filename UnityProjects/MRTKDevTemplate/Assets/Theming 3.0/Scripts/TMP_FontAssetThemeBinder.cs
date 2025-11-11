using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class TMP_FontAssetThemeBinder : BaseThemeBinder<TMP_FontAssetThemeItemData>
    {
        [field: SerializeField]
        private TMP_Text Target { get; set; }

        protected override void Apply()
        {
            Target.font = ThemedItemData.Font;
        }
    }
}

using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class TMP_FontAssetThemeBinder : BaseThemeBinder<TMP_FontAsset>
    {
        [field: SerializeField]
        private TMP_Text Target { get; set; }

        protected override void Apply(BaseThemeItemData<TMP_FontAsset> themeItemData)
        {
            Target.font = themeItemData.Value;
        }
    }
}

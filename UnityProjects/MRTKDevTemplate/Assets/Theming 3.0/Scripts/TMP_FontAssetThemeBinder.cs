// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using TMPro;

namespace MixedReality.Toolkit.Theming
{
    public class TMP_FontAssetThemeBinder : BaseThemeBinder<TMP_FontAsset, TMP_Text>
    {
        protected override void Apply(BaseThemeItemData<TMP_FontAsset> themeItemData)
        {
            if (Target != null)
            {
                Target.font = themeItemData.Value;
            }
        }
    }
}

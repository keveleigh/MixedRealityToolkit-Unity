using MixedReality.Toolkit.UX;
using System;
using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class FontIconSetBinder : BaseThemeBinder<FontIconSetBinder.FontIconSetData>
    {
        [SerializeField]
        private FontIconSelector iconSelector;

        [SerializeField]
        private TMP_Text textMeshProComponent;

        protected override void Apply(BaseThemeItemData<FontIconSetData> themeItemData)
        {
            if (textMeshProComponent != null && themeItemData.Value.FontIconSet.TryGetGlyphIcon(iconSelector.CurrentIconName, out uint unicodeValue))
            {
                // Clear the text to prevent missing character warnings when changing the font
                textMeshProComponent.text = string.Empty;
                textMeshProComponent.font = themeItemData.Value.Font;
                textMeshProComponent.text = FontIconSet.ConvertUnicodeToHexString(unicodeValue);
            }
        }

        [Serializable]
        public class FontIconSetData
        {
            [field: SerializeField]
            public TMP_FontAsset Font { get; private set; }

            [field: SerializeField]
            public FontIconSet FontIconSet { get; private set; }
        }
    }
}

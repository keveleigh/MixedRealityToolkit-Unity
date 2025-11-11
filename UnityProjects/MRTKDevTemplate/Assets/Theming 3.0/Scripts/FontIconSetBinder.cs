using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class FontIconSetBinder : BaseThemeBinder<FontIconSetItemData>
    {
        [SerializeField]
        private FontIconSelector iconSelector;

        [SerializeField]
        private TMP_Text textMeshProComponent;

        protected override void Apply()
        {
            if (textMeshProComponent != null && ThemedItemData.FontIconSet.TryGetGlyphIcon(iconSelector.CurrentIconName, out uint unicodeValue))
            {
                // Clear the text to prevent missing character warnings when changing the font
                textMeshProComponent.text = string.Empty;
                textMeshProComponent.font = ThemedItemData.Font;
                textMeshProComponent.text = FontIconSet.ConvertUnicodeToHexString(unicodeValue);
            }
        }
    }
}

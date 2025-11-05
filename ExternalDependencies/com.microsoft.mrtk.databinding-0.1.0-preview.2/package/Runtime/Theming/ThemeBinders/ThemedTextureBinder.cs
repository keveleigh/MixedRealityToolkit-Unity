using Microsoft.MixedReality.Toolkit.EditorTools;
using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedTextureBinder : BaseThemeBinder<Texture>
    {
        [DrawIf(nameof(Editor_IsRecordValid))]
        public Image ImageSwapTarget = null;
        [DrawIf(nameof(Editor_IsRecordValid))]
        public RawImage RawImageSwapTarget = null;

        protected override void PropagateEffect(ThemeItemValue themeValue)
        {
            if (ImageSwapTarget != null && ImageSwapTarget.sprite.texture != themeValue.TextureValue)
            {
                if (themeValue != null)
                {
                    Rect textureRect = new Rect(Vector2.zero, new Vector2(themeValue.TextureValue.width, themeValue.TextureValue.height));
                    ImageSwapTarget.sprite = Sprite.Create(themeValue.TextureValue as Texture2D, textureRect, Vector2.zero);
                }
                else
                {
                    ImageSwapTarget.sprite = null;
                }
            }
            if (RawImageSwapTarget != null)
            {
                RawImageSwapTarget.texture = themeValue.TextureValue;
            }
        }
    }
}

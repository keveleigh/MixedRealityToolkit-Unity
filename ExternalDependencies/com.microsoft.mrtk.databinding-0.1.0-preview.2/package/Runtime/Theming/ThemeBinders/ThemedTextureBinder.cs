using Microsoft.MixedReality.Toolkit.EditorTools;
using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.MixedReality.Toolkit.Theming
{
    public class ThemedTextureBinder : BaseThemeBinder<Texture>
    {
        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
        public Image ImageSwapTarget = null;
        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
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

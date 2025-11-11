using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Themes
{
    public class MaterialThemeBinder : BaseThemeBinder<MaterialThemeItemData>
    {
        [field: SerializeField]
        public Graphic MaterialSwapTarget = null;

        protected override void Apply()
        {
            MaterialSwapTarget.material = ThemedItemData.Material;
        }
    }
}

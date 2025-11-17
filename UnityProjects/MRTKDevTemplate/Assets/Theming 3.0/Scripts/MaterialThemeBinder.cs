using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Themes
{
    public class MaterialThemeBinder : BaseThemeBinder<Material>
    {
        [field: SerializeField]
        public Graphic MaterialSwapTarget = null;

        protected override void Apply(BaseThemeItemData<Material> themeItemData)
        {
            MaterialSwapTarget.material = themeItemData.Value;
        }
    }
}

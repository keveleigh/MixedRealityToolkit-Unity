using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedMaterialBinder : BaseThemeBinder<Material>
    {
        [DrawIf(nameof(Editor_IsRecordValid))]
        public Graphic MaterialSwapTarget = null;

        protected override void PropagateEffect(ThemeItemValue themeValue)
        {
            if (MaterialSwapTarget != null)
            {
                MaterialSwapTarget.material = themeValue.MaterialValue;
            }
        }
    }
}

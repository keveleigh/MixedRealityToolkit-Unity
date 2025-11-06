using Microsoft.MixedReality.Toolkit.EditorTools;
using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedMaterialBinder : BaseThemeBinder<Material>
    {
        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
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

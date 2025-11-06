using Microsoft.MixedReality.Toolkit.EditorTools;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedVisibilityBinder : BaseThemeBinder<bool>
    {
        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
        public GameObject TargetObject;

        protected override void PropagateEffect(ThemeItemValue themeValue)
        {
            if (TargetObject != null)
            {
                TargetObject.SetActive(themeValue.BoolValue);
            }
        }
    }
}

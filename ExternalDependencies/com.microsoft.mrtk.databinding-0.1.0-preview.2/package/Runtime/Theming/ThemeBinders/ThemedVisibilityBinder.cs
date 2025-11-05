using MixedReality.Toolkit;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    public class ThemedVisibilityBinder : BaseThemeBinder<bool>
    {
        [DrawIf(nameof(Editor_IsRecordValid))]
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

using Microsoft.MixedReality.Toolkit.EditorTools;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Kinds of visual changes we want to effect:
    /// - Material swaps*** (will this work on fonts?)
    /// - Color changes on StateViz
    /// - Sprite swaps
    /// - Feature toggles
    /// - Object vis toggles
    /// </summary>
    public class ThemedElement : MonoBehaviour
    {
        [List]
        [SerializeField]
        private ReferenceList<IThemeBinder> binders = null;

        private void Awake()
        {
            TriggerBinderSubscribes();
        }

        private void TriggerBinderSubscribes()
        {
            foreach (var binder in binders)
            {
                binder.Subscribe();
            }
        }
    }
}

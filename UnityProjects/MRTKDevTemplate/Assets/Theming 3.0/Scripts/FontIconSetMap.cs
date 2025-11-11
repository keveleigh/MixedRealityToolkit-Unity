using MixedReality.Toolkit.UX;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    [CreateAssetMenu(fileName = "MRTK_Theming_FontIconSetMap_New", menuName = "MRTK/Theming/Font Icon Set Map")]
    public class FontIconSetMap : ScriptableObject
    {
        [SerializeField]
        private IconSetDefinition setDefinition;

        [SerializeField]
        private FontIconSet[] fontIconSets;
    }
}

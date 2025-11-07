using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class ColorThemeItemData : BaseThemeItemData
    {
        [SerializeField]
        public Color color; // Maybe generic base?

        [SerializeField]
        public string property; // Needed here? Or probably on binder
    }
}

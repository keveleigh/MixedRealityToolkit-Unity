using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class ColorThemeItemData : BaseThemeItemData<Color>
    {
        [SerializeField]
        public string property; // Needed here? Or probably on binder
    }
}

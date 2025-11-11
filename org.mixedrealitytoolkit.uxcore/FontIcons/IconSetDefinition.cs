using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    [CreateAssetMenu(fileName = "IconSetDefinition", menuName = "MRTK/Theming/Icon Set Definition")]
    public class IconSetDefinition : ScriptableObject
    {
        [SerializeField]
        private string[] iconNames;

        public string[] IconNames => iconNames;
    }
}

using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class ThemeItemDataValue<T>
    {
        [field: SerializeField]
        public T Value { get; set; }
    }
}

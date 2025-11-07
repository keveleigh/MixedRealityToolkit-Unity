namespace MixedReality.Toolkit.Themes
{
    using UnityEngine;
    using UnityEngine.UI;

    // Can this empty class be replaced?
    public class FontThemeBinder : BaseThemeBinder<FontThemeItemData>
    {
        [field: SerializeField]
        private Graphic ColorTarget { get; set; }

        protected override void Apply()
        {
            throw new System.NotImplementedException();
        }
    }
}

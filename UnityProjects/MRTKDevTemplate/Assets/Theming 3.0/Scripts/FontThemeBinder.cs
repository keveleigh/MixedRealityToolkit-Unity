namespace MixedReality.Toolkit.Themes
{
    using UnityEngine;
    using UnityEngine.UI;

    public class FontThemeBinder : BaseThemeBinder<Font>
    {
        [field: SerializeField]
        private Graphic ColorTarget { get; set; }

        protected override void Apply(BaseThemeItemData<Font> themeItemData)
        {
            throw new System.NotImplementedException();
        }
    }
}

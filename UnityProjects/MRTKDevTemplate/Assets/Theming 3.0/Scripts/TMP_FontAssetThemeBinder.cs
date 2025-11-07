using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Themes
{
    public class TMP_FontAssetThemeBinder : BaseThemeBinder<TMP_FontAsset>
    {
        [field: SerializeField]
        private TMP_Text Target { get; set; }

        protected override void Apply()
        {
            throw new System.NotImplementedException();
        }
    }
}

using Microsoft.MixedReality.Toolkit.EditorTools;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    public class ThemeLoader : MonoBehaviour
    {
        [SerializeField]
        private List<ThemeDefinition> themeDefinitions = null;

        private void Start()
        {
            LoadThemeDefinitions();
        }

        private void LoadThemeDefinitions()
        {
            foreach (ThemeDefinition themeDefinition in themeDefinitions)
            {
                ThemeManager.Instance.LoadThemeDefinition(themeDefinition);
            }
        }

        [MethodButton(Label ="Reload Theme")]
        private void Editor_Refresh()
        {
            if (Application.isPlaying)
            {
                LoadThemeDefinitions();
            }
        }
    }
}
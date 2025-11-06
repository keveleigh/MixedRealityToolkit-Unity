using Microsoft.MixedReality.Toolkit.EditorTools;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [Serializable]
    public class ThemeItemValue
    {
        [HideInInspector]
        [SerializeField]
        private ThemeDataType dataItemType = ThemeDataType.Material;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.Material, ShowIfEnum.IsEqual)]
        [NoLabel]
        public Material MaterialValue = null;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.Color, ShowIfEnum.IsEqual)]
        [NoLabel]
        public Color ColorValue = Color.white;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.Gradient, ShowIfEnum.IsEqual)]
        [NoLabel]
        public Gradient GradientValue = null;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.TMPGradient, ShowIfEnum.IsEqual)]
        [NoLabel]
        public TMP_ColorGradient TmpGradientValue = null;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.Bool, ShowIfEnum.IsEqual)]
        [NoLabel]
        public bool BoolValue = false;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.Int, ShowIfEnum.IsEqual)]
        [NoLabel]
        public int IntValue = 0;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.Float, ShowIfEnum.IsEqual)]
        [NoLabel]
        public float FloatValue = 0;

        [Conditional(nameof(dataItemType), (int)ThemeDataType.Texture, ShowIfEnum.IsEqual)]
        [NoLabel]
        public Texture TextureValue = null;

        //[DrawIf(nameof(dataItemType), (int)ThemeDataType.Icon)]
        //[NoLabel]
        //public FontIconSet FontIconSet = null;

        // [DrawIf(nameof(dataItemType), (int)ThemeDataType.Font)]
        // [NoLabel]
        // public Font Font = null;

        public void Editor_SetThemeDataType(ThemeDataType themeDataType)
        {
            dataItemType = themeDataType;
        }
    }

    /// <summary>
    /// Associates a ThemeItemDefinition with the associated value
    /// </summary>
    [Serializable]
    public class ThemeItem
    {
        protected const string invalidRecordMessage = "A valid theme record must be set";
        protected const string emptyNameMessage = "Theme item name must be set";

        public const int InvalidId = int.MinValue;

        [InspectTrigger(nameof(Editor_OnInspectName))]
        [ChangeTrigger(nameof(Editor_OnInspectName))]
        [SerializeField]
        private ThemeRecord themeRecord = null;

        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
        [InspectTrigger(nameof(Editor_OnInspectName))]
        [ChangeTrigger(nameof(Editor_OnNameChanged))]
        [Popup(nameof(namesPopupList))]
        [Validate(nameof(Editor_IsNameValid), invalidRecordMessage)]
        [CustomLabel("Name")]
        public string SelectedName = string.Empty;

        [HideInInspector]
        [ReadOnly]
        [SerializeField]
        private int selectedId = ThemeItem.InvalidId;

        [HideInInspector]
        [SerializeField]
        private ThemeDataType dataItemType;

        [Inline]
        [HorizontalGroup("Theme Values")]
        [SerializeField]
        private ThemeItemValue darkThemeValue;

        [Inline]
        [HorizontalGroup("Theme Values")]
        [SerializeField]
        private ThemeItemValue lightThemeValue;

        [Inline]
        [HorizontalGroup("Theme Values")]
        [SerializeField]
        private ThemeItemValue highContrastThemeValue;

        private bool Editor_IsRecordValid() { return themeRecord != null; }

        private string _itemName = string.Empty;

        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_itemName) && themeRecord != null && selectedId != InvalidId)
                {
                    if (!string.IsNullOrWhiteSpace(SelectedName) || themeRecord.TryGetItemForId(selectedId, out SelectedName, out dataItemType))
                    {
                        /// Item names will be "[record name]-[SelectedName]
                        _itemName = $"{themeRecord.RecordPrefix}.{SelectedName}";
                    }
                }
                return _itemName;
            }
        }

        public ThemeRecord Record => themeRecord;
        public int Id => selectedId;
        public ThemeDataType DataItemType => dataItemType;

        public ThemeItemValue DarkValue => darkThemeValue;
        public ThemeItemValue LightValue => lightThemeValue;
        public ThemeItemValue HighContrast => highContrastThemeValue;

        #region Editor-Only Logic
        protected virtual bool Editor_IsNameValid()
        {
            Editor_OnNameChanged(SelectedName);
            return !string.IsNullOrWhiteSpace(SelectedName);
        }

        private List<string> _namesList = null;
        protected List<string> namesPopupList
        {
            get
            {
                if (_namesList == null)
                {
                    _namesList = Editor_GetNames();
                }
                return _namesList;
            }
            set
            {
                _namesList = value;
            }
        }

        public void DoFirstNameRetrieval()
        {
            namesPopupList = Editor_GetNames();
        }

        protected List<string> Editor_GetNames()
        {
            if (themeRecord != null)
            {
                // if we're storing an ID, then prefer it over the stored name
                if (selectedId != ThemeItem.InvalidId)
                {
                    if (Application.isPlaying)
                    {

                        //if (DataSourceManager.TryGetNameForId(GetSourceName(), selectedId, out string itemName))
                        //{
                        //    SelectedName = itemName;
                        //    name = SelectedName;
                        //}
                    }
                    else
                    {
                        if (themeRecord.TryGetItemForId(selectedId, out string itemName, out ThemeDataType dataType))
                        {
                            SelectedName = itemName;
                            dataItemType = dataType;
                            Editor_SetDataType(dataType);
                        }
                    }
                }

                return themeRecord.GetDataItemNames();
            }
            else
            {
                return new List<string>() { SelectedName };
            }
        }

        protected void Editor_OnInspectName()
        {
            namesPopupList = Editor_GetNames();
        }

        protected virtual void Editor_OnNameChanged(string name)
        {
            if (themeRecord != null)
            {
                if (themeRecord.TryGetIdForName(name, out selectedId))
                {
                    themeRecord.TryGetItemForId(selectedId, out string itemName, out dataItemType);
                    Editor_SetDataType(dataItemType);
                }
            }
        }

        protected void Editor_InspectThemeItemValue()
        {
            Editor_SetDataType(dataItemType);
        }

        protected void Editor_SetDataType(ThemeDataType dataType)
        {
            darkThemeValue.Editor_SetThemeDataType(dataType);
            lightThemeValue.Editor_SetThemeDataType(dataType);
            highContrastThemeValue.Editor_SetThemeDataType(dataType);
        }
        #endregion
    }
}

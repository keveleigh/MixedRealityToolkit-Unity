using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.EditorTools;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.DataBinding;

namespace Microsoft.MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Describes the types of data a ThemeItem can represent
    /// </summary>
    public enum ThemeDataType : int
    {
        Material,
        Color,
        Gradient,
        TMPGradient,
        Bool,
        Int,
        Float,
        Texture,
    }

    /// <summary>
    /// Describes a piece of data that can be defined by a theme
    /// </summary>
    [System.Serializable]
    public class ThemeItemDefinition
    {
        [HideInInspector]
        [ReadOnly]
        public int Id = 0;
        [InspectTrigger(nameof(Editor_AutogenerateId))]
        [ChangeTrigger(nameof(Editor_AutogenerateId))]
        public string ItemName = string.Empty;
        public ThemeDataType Type = ThemeDataType.Material;

        public void AutoGenerateId(bool force = false)
        {
            if (Id == 0 || force)
            {
                Id = UnityEngine.Random.Range(1, int.MaxValue);
            }
        }

        private void Editor_AutogenerateId()
        {
            AutoGenerateId(false);
        }
    }

    /// <summary>
    /// For flexible definition of theming items:
    /// - Need to define the Item with a Name & Type 
    /// - Need to associate an item with the actual data for a given Item when defining a theme
    /// 
    /// What addresses will theme item within a given definition be discoverable at?
    /// The address will be comprised of a few different pieces of information:
    /// - is this ThemeRecord intended to be part of the default theme? 
    /// - what theme type is 
    /// 
    /// </summary>

    [CreateAssetMenu(fileName = "Theme Record", menuName = "UXTools/Theming/Theme Record", order = 0)]
    public class ThemeRecord : BaseDataSourceRecord
    {
        private const string invalidNameError = "Theme Record Name cannot be empty!";

        [Validate(nameof(Editor_ValidateRecordName), invalidNameError, MessageBoxType.Error)]
        [SerializeField]
        private string themeRecordName = string.Empty;

        [Inline(InlineDisplayMode.MembersOnly)]
        [List(AddCallback =nameof(Editor_OnItemAdded))]
        [SerializeField]
        private SerializedList<ThemeItemDefinition> themeItems = null;

        public string RecordPrefix => themeRecordName;

        //protected override bool ShowDefaultInspector() { return false; }

        public override bool GetName(out string name, GameObject instance = null)
        {
            name = ThemeDataSource.SourceNamePrefix;// $"{ThemeDataSource.SourceNamePrefix}.{themeRecordName}";
            return true;
        }

        public override bool TryGetNameForId(int id, out string name)
        {
            name = string.Empty;
            foreach (ThemeItemDefinition item in themeItems)
            {
                if (item.Id == id)
                {
                    name = $"{RecordPrefix}.{item.ItemName}";
                }
            }
            return id != ThemeItem.InvalidId;
        }

        public override bool TryGetIdForName(string name, out int id)
        {
            id = ThemeItem.InvalidId;
            foreach (ThemeItemDefinition item in themeItems)
            {
                if (item.ItemName == name)
                {
                    id = item.Id;
                }
            }
            return id != ThemeItem.InvalidId;
        }

        public bool TryGetItemForId(int id, out string name, out ThemeDataType dataType)
        {
            name = string.Empty;
            dataType = ThemeDataType.Material;
            foreach (ThemeItemDefinition item in themeItems)
            {
                if (item.Id == id)
                {
                    name = item.ItemName;
                    dataType = item.Type;
                    break;
                }
            }

            return !string.IsNullOrWhiteSpace(name);
        }

        public override bool TryGetTypeForName(string name, out string type)
        {
            type = string.Empty;
            foreach (ThemeItemDefinition item in themeItems)
            {
                if (item.ItemName == name)
                {
                    type = GetTypeString(item.Type);
                    break;
                }
            }

            return !string.IsNullOrWhiteSpace(type);
        }

        public override bool TryGetTypeForId(int id, out string type)
        {
            type = string.Empty;
            foreach (ThemeItemDefinition item in themeItems)
            {
                if (item.Id == id)
                {
                    type = GetTypeString(item.Type);
                    break;
                }
            }

            return !string.IsNullOrWhiteSpace(type);
        }

        public override bool ContainsItem(string itemName)
        {
            if (itemNames != null)
            {
                for (int i = 0; i < itemNames.Count; i++)
                {
                    if (itemNames[i] == itemName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool ContainsItem(int id)
        {
            if (itemNames != null)
            {
                foreach (ThemeItemDefinition item in themeItems)
                {
                    if (item.Id == id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override List<string> GetDataItemNames()
        {
            if (itemNames == null || itemNames.Count == 0 || itemNames.Count != themeItems.Count)
            {
                itemNames = new List<string>(themeItems.Count);
            }

            itemNames.Clear();
            foreach (ThemeItemDefinition item in themeItems)
            {
                itemNames.Add(item.ItemName);
            }

            return itemNames;
        }

        public override List<string> GetDataItemNamesForType(string type)
        {
            List<string> typedNames = new List<string>();

            foreach (ThemeItemDefinition item in themeItems)
            {
                if (DoesTypeMatch(type, item.Type))
                {
                    typedNames.Add(item.ItemName);
                }
            }
            return typedNames.Count > 0 ? typedNames : null;
        }

        private bool DoesTypeMatch(string type, ThemeDataType dataType)
        {
            return type == GetTypeString(dataType);
        }

        private string GetTypeString(ThemeDataType dataType)
        {
            switch (dataType)
            {
                case ThemeDataType.Bool:
                    {
                        return typeof(bool).ToString();
                    }
                case ThemeDataType.Int:
                    {
                        return typeof(int).ToString();
                    }
                case ThemeDataType.Float:
                    {
                        return typeof(float).ToString();
                    }
                case ThemeDataType.Material:
                    {
                        return typeof(Material).ToString();
                    }
                case ThemeDataType.Color:
                    {
                        return typeof(Color).ToString();
                    }
                case ThemeDataType.Gradient:
                    {
                        return typeof(Gradient).ToString();
                    }
                case ThemeDataType.TMPGradient:
                    {
                        return typeof(TMP_ColorGradient).ToString();
                    }
                case ThemeDataType.Texture:
                    {
                        return typeof(Texture).ToString();
                    }
            }

            return string.Empty;
        }

        private bool Editor_ValidateRecordName()
        {
            return !string.IsNullOrWhiteSpace(themeRecordName);
        }

        private void Editor_OnItemAdded()
        {
            HashSet<int> existingIds = new HashSet<int>(themeItems.Count);
            foreach (ThemeItemDefinition item in themeItems)
            {
                if (existingIds.Contains(item.Id))
                {
                    item.AutoGenerateId(force: true);
                    existingIds.Add(item.Id);
                }
                else
                {
                    existingIds.Add(item.Id);
                }
                Debug.Log($"Item Id: {item.Id}");
            }
        }
    }
}
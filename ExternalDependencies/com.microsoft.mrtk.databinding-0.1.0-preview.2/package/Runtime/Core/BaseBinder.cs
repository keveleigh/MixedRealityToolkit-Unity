using Microsoft.MixedReality.Toolkit.EditorTools;
using Microsoft.MixedReality.Toolkit.Theming;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.DataBinding
{
    public abstract class BaseBinder : IBinder
    {
        protected const string invalidRecordMessage = "A valid data source record must be set";

        [InspectTrigger(nameof(Editor_OnInspectRecord))]
        [ChangeTrigger(nameof(Editor_OnInspectRecord))]
        [Validate(nameof(Editor_IsRecordValid), invalidRecordMessage, MessageBoxType.Error)]
        [SerializeField]
        protected BaseDataSourceRecord dataSourceRecord = null;

        [Conditional(nameof(Editor_IsRecordValid), ShowIfBool.IsTrue)]
        [InspectTrigger(nameof(Editor_OnInspectName))]
        [Popup(nameof(ValueNamesForEditor))]
        [ChangeTrigger(nameof(Editor_OnTargetValueNameUpdate))]
        [Validate(nameof(Editor_ValidateTargetValueName), "ItemName not found in DataSourceRecord", MessageBoxType.Error)]
        [SerializeField]
        protected string itemName = null;

        [HideInInspector]
        [ReadOnly]
        [SerializeField]
        protected int targetValueId = BaseDataSourceRecord.InvalidId;

        private List<string> _nameList = null;
        public List<string> ValueNamesForEditor
        {
            get
            {
                if (_nameList == null)
                {
                    return new List<string>() { itemName };
                }

                return _nameList;
            }
            set
            {
                if (_nameList != value)
                {
                    _nameList = value;
                    if (_nameList != null && _nameList.Count == 1 && string.IsNullOrWhiteSpace(itemName))
                    {
                        itemName = _nameList[0];
                    }
                }
            }
        }

        public void Subscribe()
        {
            if (dataSourceRecord != null)
            {
                dataSourceRecord.GetName(out string sourceName);
                dataSourceRecord.TryGetNameForId(targetValueId, out string targetName);
                DataSourceManager.SubscribeToDataItem(sourceName, targetName, OnItemUpdate);

#if UNITY_EDITOR
                ValueNamesForEditor = Editor_GetNames();
#endif
            }
        }

        public void Unsubscribe()
        {
            if (dataSourceRecord != null)
            {
                dataSourceRecord.GetName(out string sourceName);
                dataSourceRecord.TryGetNameForId(targetValueId, out string targetName);
                DataSourceManager.UnsubscribeFromDataItem(sourceName, targetName, OnItemUpdate);
            }
        }

        public bool TryGetValue<T>(out T value)
        {
            bool success = false;
            value = default(T);

            if (dataSourceRecord != null && dataSourceRecord.GetName(out string dataSourceName) &&
                DataSourceManager.TryGetDataSource(dataSourceName, out IDataSource dataSource) &&
                dataSource.TryGetValue<T>(targetValueId, out T retrievedValue))
            {
                value = retrievedValue;
                success = true;
            }

            return success;
        }

        protected abstract void OnItemUpdate(IDataSource dataSource, string itemName);

        private void Editor_OnInspectRecord()
        {
            if (dataSourceRecord != null)
            {
                ValueNamesForEditor = dataSourceRecord.GetDataItemNames();
                if (!string.IsNullOrWhiteSpace(itemName) && targetValueId == BaseDataSourceRecord.InvalidId)
                {
                    Editor_OnTargetValueNameUpdate();
                }
                else if (dataSourceRecord.TryGetIdForName(itemName, out int id) && id != targetValueId)
                {
                    Editor_OnTargetValueNameUpdate();
                }
            }
            else
            {
                ValueNamesForEditor = Editor_GetNames();
            }
        }

        protected virtual bool Editor_IsRecordValid()
        {
            return dataSourceRecord != null;
        }

        protected void Editor_OnInspectName()
        {
            _nameList = Editor_GetNames();
        }

        protected virtual List<string> Editor_GetNames()
        {
            if (dataSourceRecord != null)
            {
                // if we're storing an ID, then prefer it over the stored name
                if (targetValueId != ThemeItem.InvalidId)
                {
                    if (Application.isPlaying)
                    {
                        if (dataSourceRecord.GetName(out string sourceName) && DataSourceManager.TryGetNameForId(sourceName, targetValueId, out string itemName))
                        {
                            this.itemName = itemName;
                        }
                    }
                    else
                    {
                        if (dataSourceRecord is ThemeRecord record && record.TryGetItemForId(targetValueId, out string itemName, out ThemeDataType dataType))
                        {
                            this.itemName = itemName;
                        }
                    }
                }

                return dataSourceRecord.GetDataItemNames();
            }
            else
            {
                return new List<string>() { itemName };
            }
        }

        private void Editor_OnTargetValueNameUpdate()
        {
            if (dataSourceRecord != null && dataSourceRecord.TryGetIdForName(itemName, out int id))
            {
                targetValueId = id;
            }
            else
            {
                targetValueId = ThemeItem.InvalidId;
            }
        }

        private bool Editor_ValidateTargetValueName()
        {
            return dataSourceRecord != null ? dataSourceRecord.ContainsItem(itemName) : false;
        }
    }
}
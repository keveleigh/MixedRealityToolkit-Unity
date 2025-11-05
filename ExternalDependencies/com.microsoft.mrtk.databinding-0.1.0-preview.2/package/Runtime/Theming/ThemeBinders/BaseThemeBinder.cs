using Microsoft.MixedReality.Toolkit.DataBinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [Serializable]
    public abstract class BaseThemeBinder<T> : BaseBinder, IThemeBinder
    {
        protected virtual void RetrieveValue(IDataSource dataSource, string itemName)
        {
            if (dataSource.TryGetValue<ThemeItemValue>(itemName, out ThemeItemValue itemValue))
            {
                PropagateEffect(itemValue);
            }
        }

        protected override void OnItemUpdate(IDataSource dataSource, string itemName)
        {
            RetrieveValue(dataSource, itemName);
        }

        protected abstract void PropagateEffect(ThemeItemValue themeValue);

        protected override List<string> Editor_GetNames()
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

                return dataSourceRecord is ThemeRecord themeRecord ? themeRecord.GetDataItemNamesForType(typeof(T).ToString()) : dataSourceRecord.GetDataItemNames();
            }
            else
            {
                return new List<string>() { itemName };
            }
        }
    }
}
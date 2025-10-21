using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.DataBinding
{
    /// <summary>
    /// An IDataSourceRecord contains information about the associated data source, as well
    /// as the ID, name, and type information for each data item available for binding. IDataSourceRecords
    /// are created by <see cref="IDataSource"/> implementations at edit time and used to by subscribers to
    /// bind to the correct data at runtime.
    /// </summary>
    internal interface IDataSourceRecord
    {
        /// <summary>
        /// Obsolete- use <see cref="GetName(out string, GameObject)"/> instead
        /// </summary>
        string SourceName { get; }
        bool IsInstanced { get; }
        bool GetName(out string name, GameObject instance = null);

        List<string> GetDataItemNames();
        List<string> GetDataItemNamesForType(string type);

        bool TryGetNameForId(int id, out string name);
        bool TryGetIdForName(string name, out int id);

        bool TryGetTypeForName(string name, out string type);
        bool TryGetTypeForId(int id, out string type);

        bool ContainsItem(int id);
        bool ContainsItem(string name);
    }
}
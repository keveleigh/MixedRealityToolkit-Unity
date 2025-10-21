using Microsoft.MixedReality.Toolkit.EditorTools;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.DataBinding
{
    /// <summary>
    /// Recommended base class to inherit from when creating a custom <see cref="ScriptableObject"/>-based <see cref="IDataSourceRecord"/> for a
    /// new <see cref="BaseDataSource"/> implementation.
    /// </summary>
    public abstract class BaseDataSourceRecord : ScriptableObject, IDataSourceRecord
    {
        public static readonly int InvalidId = int.MinValue;

        [ReadOnly]
        [SerializeField]
        protected string sourceName = string.Empty;

        protected List<string> itemNames = null;

        public string SourceName => sourceName;
        public virtual bool IsInstanced => false;

        public abstract bool GetName(out string name, GameObject instance = null);

        public abstract List<string> GetDataItemNames();
        public abstract List<string> GetDataItemNamesForType(string type);

        public abstract bool TryGetNameForId(int id, out string name);
        public abstract bool TryGetIdForName(string name, out int id);

        public abstract bool TryGetTypeForName(string name, out string type);
        public abstract bool TryGetTypeForId(int id, out string type);

        public abstract bool ContainsItem(int id);
        public abstract bool ContainsItem(string name);
    }
}
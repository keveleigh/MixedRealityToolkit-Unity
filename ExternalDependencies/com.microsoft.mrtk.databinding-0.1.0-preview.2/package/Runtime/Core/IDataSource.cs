namespace Microsoft.MixedReality.Toolkit.DataBinding
{
    public delegate void DataItemUpdate(IDataSource dataSource, string itemName);

    /// <summary>
    /// Represents a collection of values at runtime, and publishes changes to those values to subscribers.
    /// 
    /// <br/>
    /// It is recommended that you do not implement this interface yourself unless totally necessary. Instead, 
    /// inherit from <see cref="BaseDataSource"/>.
    /// </summary>
    public interface IDataSource
    {
        string DataSourceName { get; }
        bool IsReadyForSubscribers { get; }

        void Initialize();
        void CleanUp();
        bool TrySubscribeToItem(string itemName, DataItemUpdate onUpdate);
        bool TrySubscribeToItemInCollection(string listName, int index, DataItemUpdate onUpdate);
        void UnsubscribeFromItem(string itemName, DataItemUpdate onUpdate);

        void UpdateRecord(bool force = false, bool clean = false);

        bool TryGetIdForName(string name, out int id);
        bool TryGetNameForId(int id, out string name);

        bool TryGetValue<T>(string itemName, out T itemValue);
        bool TryGetValue<T>(int id, out T itemValue);

        bool TryGetValueAtIndex<T>(string listName, int index, out T itemValue);
        bool TryGetValueAtIndex<T>(int listId, int index, out T itemValue);

        bool TrySetValue<T>(string itemName, T itemValue);
        bool TrySetValue<T>(int itemId, T itemValue);

        bool TryGetCollectionCount(string collectionName, out int count);
    }
}
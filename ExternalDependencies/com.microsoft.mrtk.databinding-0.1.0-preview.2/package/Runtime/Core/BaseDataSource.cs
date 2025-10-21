using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.DataBinding
{
    /// <summary>
    /// The recommended base class to inherit from when creating a custom <see cref="IDataSource"/> implementation.
    /// This class comes with built-in support for <see cref="ScriptableObject"/>-based <see cref="BaseDataSourceRecord"/> 
    /// implementations, which work seamlessly with the Unity Inspector-based data binding workflow.
    /// 
    /// <br/><br/>
    /// Each <see cref="BaseDataSource"/> contains a <see cref="BaseDataSourceRecord"/> that contains items comprised of IDs, names,
    /// and types. At editor-time, the <see cref="BaseDataSourceRecord"/> object is used to populate the Inspector UI for consumers to
    /// choose what item to bind a component to. At runtime, this class will publish value changes to subscribers.
    /// </summary>    [Serializable]
    public abstract class BaseDataSource : IDataSource
    {
        private const int defaultItemCapacity = 50;

        public string DataSourceName { get; protected set; }

        public bool IsReadyForSubscribers { get; protected set; }

        [SerializeField]
        protected BaseDataSourceRecord record = null;

        protected Dictionary<string, List<DataItemUpdate>> itemSubscribers = null;

        public BaseDataSource(string name)
        {
            DataSourceName = name;
            if (Application.isPlaying)
            {
                itemSubscribers = new Dictionary<string, List<DataItemUpdate>>(defaultItemCapacity);
            }
        }

        public virtual void Initialize()
        {
            if (Application.isPlaying)
            {
                itemSubscribers = new Dictionary<string, List<DataItemUpdate>>(defaultItemCapacity);
                DataSourceManager.RegisterDataSource(this);
            }
        }

        public void CleanUp()
        {
            DataSourceManager.UnregisterDataSource(this);
        }

        /// <summary>
        /// Adds <paramref name="onUpdate"/> as a function to be called when the value corresponding to <paramref name="itemName"/> in this <see cref="BaseDataSource"/> changes.
        /// </summary>
        /// 
        /// <remarks>
        /// <paramref name="onUpdate"/> will be triggered immediately at the end of this function call.
        /// </remarks>
        /// <param name="itemName">The key of the item in this <see cref="BaseDataSource"/></param>
        /// <param name="onUpdate">A delegate function that should be run when the corresponding value of <paramref name="itemName"/> changes.</param>
        public bool TrySubscribeToItem(string itemName, DataItemUpdate onUpdate)
        {
            if (!itemSubscribers.TryGetValue(itemName, out List<DataItemUpdate> subscribers))
            {
                subscribers = new List<DataItemUpdate>();
                itemSubscribers[itemName] = subscribers;
            }
            subscribers.Add(onUpdate);

            onUpdate(this, itemName);

            // for now this always returns true just in case the data source loads the item in later
            return true;
        }

        /// <summary>
        /// Adds <paramref name="onUpdate"/> as a function to be called when the <paramref name="index"/> element of the list corresponding to <paramref name="listName"/>'s value changes.
        /// </summary>
        ///
        /// <remarks>
        /// <paramref name="onUpdate"/> will be triggered immediately at the end of this function call.
        /// </remarks>
        /// <param name="listName">The key of the list in this <see cref="BaseDataSource"/></param>
        /// <param name="index">The index of the item to listen to for value changes</param>
        /// <param name="onUpdate">A delegate function that should be run when the value of the selected element of the list corresponding to <paramref name="listName"/> changes.</param>
        /// <returns>Whether <paramref name="onUpdate"/> was successfully added as a subscriber.</returns>
        public bool TrySubscribeToItemInCollection(string listName, int index, DataItemUpdate onUpdate)
        {
            bool success = false;
            string itemName = GetItemNameForCollectionItemAtIndex(listName, index);
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                if (!itemSubscribers.TryGetValue(itemName, out List<DataItemUpdate> subscriberList) || subscriberList == null)
                {
                    subscriberList = new List<DataItemUpdate>();
                    itemSubscribers[itemName] = subscriberList;
                }
                subscriberList.Add(onUpdate);
                onUpdate(this, itemName);
                success = true;
            }

            return success;
        }

        public void UnsubscribeFromItem(string itemName, DataItemUpdate onUpdate)
        {
            if (itemSubscribers.TryGetValue(itemName, out List<DataItemUpdate> subscribers))
            {
                subscribers.Remove(onUpdate);
            }
        }

        protected void OnItemUpdated(string itemName)
        {
            //Debug.Log($"[BaseDataSource] OnItemUpdate called for {itemName} in data source {DataSourceName}");
            if (itemSubscribers.TryGetValue(itemName, out List<DataItemUpdate> subscriberList) && subscriberList != null)
            {
                for (int i = 0; i < subscriberList.Count; i++)
                {
                    subscriberList[i]?.Invoke(this, itemName);
                }
            }
        }

        /// <summary>
        /// Given the <paramref name="name"/> of an item in this <see cref="BaseDataSource"/>, retrieves its corresponding numeric <paramref name="id"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// The inverse of this function is <see cref="TryGetNameForId(int, out string)"/>
        /// </remarks>
        /// 
        /// <param name="name">The name of the item in this <see cref="BaseDataSource"/></param>
        /// <param name="id">The numeric ID of the item in the backing <see cref="BaseDataSourceRecord"/></param>
        /// <returns>Whether an ID was found or not.</returns>
        public abstract bool TryGetIdForName(string name, out int id);

        /// <summary>
        /// Given the numeric <paramref name="id"/> of an item in this <see cref="BaseDataSource"/>, retrieves its corresponding <paramref name="name"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// The inverse of this function is <see cref="TryGetIdForName(string, out int)"/>
        /// </remarks>
        /// 
        /// <param name="id">The numeric ID of the item in the backing <see cref="BaseDataSourceRecord"/></param>
        /// <param name="name">The name of the item in this <see cref="BaseDataSource"/></param>
        /// <returns>Whether a name was found or not.</returns>
        public abstract bool TryGetNameForId(int id, out string name);

        /// <summary>
        /// Updates the values of a <see cref="BaseDataSourceRecord"/> based off the available data items/>.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="clean"></param>
        public abstract void UpdateRecord(bool force = false, bool clean = false);

        /// <summary>
        /// Given the numeric <paramref name="id"/> of an item in this <see cref="BaseDataSource"/>, retrieves its corresponding <paramref name="name"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// The inverse of this function is <see cref="TryGetIdForName(string, out int)"/>
        /// </remarks>
        /// 
        /// <param name="itemName">The human readable name of the item in the backing <see cref="BaseDataSourceRecord"/></param>
        /// <param name="itemValue">The value of the item</param>
        /// <returns>Whether a value was found or not.</returns>
        public abstract bool TryGetValue<T>(string itemName, out T itemValue);

        /// <summary>
        /// Given the numeric <paramref name="id"/> of an item in this <see cref="BaseDataSource"/>, retrieves its corresponding <paramref name="name"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// The inverse of this function is <see cref="TryGetIdForName(string, out int)"/>
        /// </remarks>
        /// 
        /// <param name="itemId">The numeric ID of the item in the backing <see cref="BaseDataSourceRecord"/></param>
        /// <param name="itemValue">The value of the item</param>
        /// <returns>Whether a value was found or not.</returns>
        public abstract bool TryGetValue<T>(int itemId, out T itemValue);

        /// <summary>
        /// Sets the value associated with the specified <paramref name="itemName"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemName"></param>
        /// <param name="itemValue"></param>
        /// <returns>Whether the value was successfully updated.</returns>
        public abstract bool TrySetValue<T>(string itemName, T itemValue);

        /// <summary>
        /// Sets the value associated with the specified <paramref name="itemId"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemId"></param>
        /// <param name="itemValue"></param>
        /// <returns>Whether the value was successfully updated.</returns>
        public abstract bool TrySetValue<T>(int itemId, T itemValue);

        /// <summary>
        /// Retrieves the number of elements in the collection referenced by <paramref name="collectionName"/>, and sets <paramref name="count"/> to that value.
        /// </summary>
        /// <param name="collectionName">The name of the collection referencing a </param>
        /// <param name="count"></param>
        /// <returns>Whether the collection count was successfully retrieved.</returns>
        public abstract bool TryGetCollectionCount(string collectionName, out int count);

        /// <summary>
        /// Retrieves the value at a specific <paramref name="index"/> of the list associated with <paramref name="itemValue"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="index"></param>
        /// <param name="itemValue"></param>
        /// <returns>Whether the value was retrieved successfully.</returns>
        public abstract bool TryGetValueAtIndex<T>(string listName, int index, out T itemValue);

        /// <summary>
        /// Retrieves the value at a specific <paramref name="index"/> of the list associated with <paramref name="itemValue"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="index"></param>
        /// <param name="itemValue"></param>
        /// <returns>Whether the value was retrieved successfully.</returns>
        public abstract bool TryGetValueAtIndex<T>(int listId, int index, out T itemValue);

        protected abstract BaseDataSourceRecord CreateRecord(string recordPath);
        protected abstract bool TryLoadRecord(string recordPath, out BaseDataSourceRecord record);
        protected abstract string GetItemNameForCollectionItemAtIndex(string listName, int itemIndex);
    }
}

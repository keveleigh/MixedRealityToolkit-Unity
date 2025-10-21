using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Microsoft.MixedReality.Toolkit.DataBinding
{
    /// <summary>
    /// Manages <see cref="BaseDataSource"/>s that are available in the data binding system. Provides the interface for:
    /// <list type="bullet">
    ///     <item>registering and unregistering <see cref="BaseDataSource"/>s</item>
    ///     <item>subscribing and unsubscribing from particular data items in a given <see cref="BaseDataSource"/></item>
    ///     <item>setting the value of a particular data item</item>
    /// </list>
    /// 
    /// Planned functionality:
    /// <list type="bullet">
    ///     <item>controlling <see cref="BaseDataSource"/> update frequencies (e.g., only poll the backend data source for and update every <c>n</c> seconds)</item>
    /// </list>
    /// </summary>
    /// 
    /// </summary>
    public class DataSourceManager : MonoBehaviour
    {
        private class PendingSubscribe
        {
            public string ItemName { get; set; }
            public DataItemUpdate UpdateFunc { get; set; }
            public bool IsCollectionItem { get; set; } = false;
            public int CollectionIndex { get; set; } = -1;
        }
        private static ObjectPool<PendingSubscribe> pendingSubscribePool = new ObjectPool<PendingSubscribe>(CreatePendingSubscribe, defaultCapacity: 1000, maxSize: 1000);

        private static List<IDataSource> dataSources = new List<IDataSource>(100);
        private static Dictionary<string, IDataSource> dataSourceLookup = new Dictionary<string, IDataSource>(100);
        private static Dictionary<string, List<PendingSubscribe>> pendingSubscribeLookup = new Dictionary<string, List<PendingSubscribe>>(200);

        private static bool startupComplete = false;
        private static List<string> dataSourcesWaitingForStartup = new List<string>(50);
        public static DataSourceManager Instance { get; private set; }

        internal static void RegisterDataSource(IDataSource dataSource, string sourceNameOverride = "")
        {
            string dataSourceName = string.IsNullOrWhiteSpace(sourceNameOverride) ? dataSource.DataSourceName : sourceNameOverride;
            if (!dataSourceLookup.TryGetValue(dataSourceName, out IDataSource source))
            {
                dataSources.Add(dataSource);
                dataSourceLookup[dataSourceName] = dataSource;

                if (Instance != null && startupComplete)
                {
                    Instance.StartCoroutine(Instance.ApplyPendingSubscribersAfterWait(dataSourceName));
                }
                else
                {
                    dataSourcesWaitingForStartup.Add(dataSourceName);
                }
            }
            else
            {
                Debug.LogWarning($"[DataSourceManager] A data source named {dataSourceName} has already been registered!");
            }
        }

        internal static void UnregisterDataSource(IDataSource dataSource, string sourceNameOverride = "")
        {
            //Debug.Log($"[DataSourceManager] Unregistering data source {dataSource.DataSourceName}");
            if (string.IsNullOrWhiteSpace(sourceNameOverride))
            {
                dataSources.Remove(dataSource);
                dataSourceLookup.Remove(dataSource.DataSourceName);
            }
            else
            {
                dataSourceLookup.Remove(sourceNameOverride);
            }
        }

        public static bool SubscribeToDataItem(string sourceName, string itemName, DataItemUpdate onUpdate)
        {
            //Debug.Log($"[DataSourceManager] Trying to subscribe to item {itemName} from data source {sourceName}");
            bool success = false;
            if (dataSourceLookup.TryGetValue(sourceName, out IDataSource dataSource) && dataSource.IsReadyForSubscribers)
            {
                dataSource.TrySubscribeToItem(itemName, onUpdate);
                success = true;
            }
            else
            {
                PendingSubscribe subscribe = pendingSubscribePool.Get();
                subscribe.ItemName = itemName;
                subscribe.UpdateFunc = onUpdate;
                if (!pendingSubscribeLookup.TryGetValue(sourceName, out List<PendingSubscribe> pendingList))
                {
                    pendingSubscribeLookup[sourceName] = new List<PendingSubscribe>(20);
                }
                pendingSubscribeLookup[sourceName].Add(subscribe);
                success = true;
            }
            return success;
        }

        public static bool SubscribeToDataItemInCollection(string sourceName, string collectionName, int index, DataItemUpdate onUpdate)
        {
            //Debug.Log($"[DataSourceManager] Trying to subscribe to item at index {index} in {collectionName} from data source {sourceName}");
            bool success = false;
            if (dataSourceLookup.TryGetValue(sourceName, out IDataSource dataSource))
            {
                success = dataSource.TrySubscribeToItemInCollection(collectionName, index, onUpdate);
            }
            return success;
        }

        public static void UnsubscribeFromDataItem(string sourceName, string itemName, DataItemUpdate onUpdate)
        {
            if (dataSourceLookup.TryGetValue(sourceName, out IDataSource dataSource))
            {
                dataSource.UnsubscribeFromItem(itemName, onUpdate);
            }
        }

        public static bool SetDataItem<T>(string sourceName, string itemName, T itemValue)
        {
            bool success = false;

            if (dataSourceLookup.TryGetValue(sourceName, out IDataSource dataSource))
            {
                success = dataSource.TrySetValue<T>(itemName, itemValue);
            }

            return success;
        }

        public static bool TryGetDataSource(string sourceName, out IDataSource dataSource)
        {
            return dataSourceLookup.TryGetValue(sourceName, out dataSource);
        }

        public static bool TryGetNameForId(string sourceName, int itemid, out string itemName)
        {
            if (dataSourceLookup.TryGetValue(sourceName, out IDataSource dataSource))
            {
                return dataSource.TryGetNameForId(itemid, out itemName);
            }
            itemName = string.Empty;
            return false;
        }

        private static PendingSubscribe CreatePendingSubscribe()
        {
            return new PendingSubscribe();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            startupComplete = true;
            if (dataSourcesWaitingForStartup.Count > 0)
            {
                foreach (string dataSourceName in dataSourcesWaitingForStartup)
                {
                    StartCoroutine(ApplyPendingSubscribersAfterWait(dataSourceName));
                }
                dataSourcesWaitingForStartup.Clear();
            }
        }

        private IEnumerator ApplyPendingSubscribersAfterWait(string sourceName)
        {
            yield return YieldInstructionCache.WaitForNextFrame;

            if (dataSourceLookup.TryGetValue(sourceName, out IDataSource dataSource))
            {
                while (!dataSource.IsReadyForSubscribers)
                {
                    yield return YieldInstructionCache.WaitForNextFrame;
                }

                if (pendingSubscribeLookup.TryGetValue(sourceName, out List<PendingSubscribe> pendingList))
                {
                    if (pendingList != null && pendingList.Count > 0)
                    {
                        for (int i = 0; i < pendingList.Count; i++)
                        {
                            PendingSubscribe pendingSubscribe = pendingList[i];
                            if (pendingSubscribe.IsCollectionItem)
                            {
                                dataSource.TrySubscribeToItemInCollection(pendingSubscribe.ItemName, pendingSubscribe.CollectionIndex, pendingSubscribe.UpdateFunc);

                            }
                            else
                            {
                                dataSource.TrySubscribeToItem(pendingSubscribe.ItemName, pendingSubscribe.UpdateFunc);
                            }
                        }
                        pendingSubscribeLookup[dataSource.DataSourceName].Clear();
                    }
                }
            }
        }
    }
}

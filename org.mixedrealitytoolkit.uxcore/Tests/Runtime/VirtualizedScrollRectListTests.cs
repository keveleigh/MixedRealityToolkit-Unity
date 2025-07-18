// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.UX.Experimental;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for the VirtualizedScrollRectList.
    /// </summary>
    public class VirtualizedScrollRectListTests : BaseRuntimeInputTests
    {
        private const string virtualizedScrollRectListTestPrefab = "Packages/org.mixedrealitytoolkit.uxcore/Tests/Runtime/Prefabs/VirtualizedScrollRectListTest.prefab";
        private readonly string[] wordSet1 = { "one", "two", "three", "zebra", "keyboard", "rabbit", "graphite", "ruby", };
        private readonly string[] wordSet2 = { "four", "five", "six", "apple", "mouse", "tortoise", "wool", "car", };

        private VirtualizedScrollRectList virtualizedScrollRectList;

        private IEnumerator SetupVirtualizedScrollRectList()
        {
            if (virtualizedScrollRectList != null)
            {
                Object.Destroy(virtualizedScrollRectList);
                yield return null;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(virtualizedScrollRectListTestPrefab);
            GameObject obj = Object.Instantiate(prefab);
            virtualizedScrollRectList = obj.GetComponentInChildren<VirtualizedScrollRectList>();

            Assert.IsNotNull(virtualizedScrollRectList, "VirtualizedScrollRectList was not found in spawned prefab.");
        }

        /// <summary>
        /// Callback to set for the OnVisible callback for wordSet1
        /// </summary>
        private void OnVisibleCallbackForSet1(GameObject go, int i)
        {
            go.transform.name = wordSet1[i % wordSet1.Length];
        }

        /// <summary>
        /// Callback to set for the OnVisible callback for wordSet2
        /// </summary>
        private void OnVisibleCallbackForSet2(GameObject go, int i)
        {
            go.transform.name = wordSet2[i % wordSet2.Length];
        }

        public override IEnumerator TearDown()
        {
            if (virtualizedScrollRectList != null)
            {
                Object.Destroy(virtualizedScrollRectList);
                // Wait for a frame to give Unity a change to actually destroy the object
                yield return null;
                Assert.IsTrue(virtualizedScrollRectList == null);
            }
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator TestVirtualizedScrollRectList_ResetLayout()
        {
            yield return SetupVirtualizedScrollRectList();
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet1;
            yield return null; // changes happens in next frame

            GameObject item;
            GameObject refItem = null;

            int i, refI = 0, foundItems = 0;

            for (i = 0; i < wordSet1.Length; i++)
            {
                if (virtualizedScrollRectList.TryGetVisible(i, out item))
                {
                    refItem = item;
                    refI = i;
                    Assert.IsTrue(wordSet1.Contains(item.transform.name), $"Item seen doesn't belong to the items passed in (set1). Got {item.transform.name} at {i}");
                    foundItems++;
                }
            }
            Assert.IsTrue(foundItems > 0, "None of the expected items were found in the scrollable list (set1).");

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet1;
            yield return null; // changes happens in next frame

            // Setting the same value shouldn't reset the layout.
            // If foundItems > 0, refI and refItem should not be empty/null
            virtualizedScrollRectList.TryGetVisible(refI, out item);
            Assert.IsTrue(item == refItem, "Setting the same value should not trigger ResetLayout");

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet2;
            yield return null; // changes happens in next frame

            for (i = 0; i < wordSet2.Length; i++)
            {
                if (virtualizedScrollRectList.TryGetVisible(i, out item))
                {
                    Assert.IsTrue(wordSet2.Contains(item.transform.name), $"Item seen doesn't belong to the items passed in (set2). Got {item.transform.name} at {i}");
                    foundItems++;
                }
            }
            Assert.IsTrue(foundItems > 0, "None of the expected items were found in the scrollable list (set2).");
        }

        [UnityTest]
        public IEnumerator TestVirtualizedScrollRectList_ResetLayoutInactive()
        {
            yield return SetupVirtualizedScrollRectList();
            yield return null; // changes happens in next frame

            // Set the OnVisible callback while the GameObject is inactive
            virtualizedScrollRectList.gameObject.SetActive(false);
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet1;
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.gameObject.SetActive(true);
            yield return null; // changes happens in next frame

            GameObject item;
            GameObject refItem = null;

            int i, refI = 0, foundItems = 0;

            for (i = 0; i < wordSet1.Length; i++)
            {
                if (virtualizedScrollRectList.TryGetVisible(i, out item))
                {
                    refItem = item;
                    refI = i;
                    Assert.IsTrue(wordSet1.Contains(item.transform.name), $"Item seen doesn't belong to the items passed in (set1). Got {item.transform.name} at {i}");
                    foundItems++;
                }
            }
            Assert.IsTrue(foundItems > 0, "None of the expected items were found in the scrollable list (set1).");

            // Set the OnVisible callback while the GameObject is inactive
            virtualizedScrollRectList.gameObject.SetActive(false);
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet1;
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.gameObject.SetActive(true);
            yield return null; // changes happens in next frame

            // Setting the same value shouldn't reset the layout.
            // If foundItems > 0, refI and refItem should not be empty/null
            virtualizedScrollRectList.TryGetVisible(refI, out item);
            Assert.IsTrue(item == refItem, "Setting the same value should not trigger ResetLayout");

            // Set the OnVisible callback while the GameObject is inactive
            virtualizedScrollRectList.gameObject.SetActive(false);
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet2;
            yield return null; // changes happens in next frame

            // Setting OnVisible shouldn't reset the layout while the GameObject is inactive
            // If foundItems > 0, refI and refItem should not be empty/null
            virtualizedScrollRectList.TryGetVisible(refI, out item);
            Assert.IsTrue(item == refItem, "Setting the value while inactive should not trigger ResetLayout");

            virtualizedScrollRectList.gameObject.SetActive(true);
            yield return null; // changes happens in next frame

            for (i = 0; i < wordSet2.Length; i++)
            {
                if (virtualizedScrollRectList.TryGetVisible(i, out item))
                {
                    Assert.IsTrue(wordSet2.Contains(item.transform.name), $"Item seen doesn't belong to the items passed in (set2). Got {item.transform.name} at {i}");
                    foundItems++;
                }
            }
            Assert.IsTrue(foundItems > 0, "None of the expected items were found in the scrollable list (set2).");
        }
    }
}
#pragma warning restore CS1591

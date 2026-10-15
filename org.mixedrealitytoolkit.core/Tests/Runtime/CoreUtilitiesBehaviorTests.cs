// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.Core.Tests
{
    /// <summary>
    /// Explicit behavioral and regression test suite for MRTK Core utilities:
    /// VectorExtensions (Average, Median, Sort), MathUtilities (NearestPointToLinesLeastSquares, RANSAC),
    /// EnumerableExtensions (DisposeElements), and TransformExtensions (GetDepth, TryGetDepth, FindAncestorComponent).
    /// </summary>
    public class CoreUtilitiesBehaviorTests : BaseRuntimeTests
    {
        #region VectorExtensions Tests

        [Test]
        public void VectorExtensions_Average_2D_ListAndNonList()
        {
            var list = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };

            Vector2 listAvg = list.Average();
            Assert.AreEqual(new Vector2(5f, 5f), listAvg, "List average for 2D vectors must equal (5, 5).");

            var hashSet = new HashSet<Vector2>(list);
            Vector2 hashSetAvg = hashSet.Average();
            Assert.AreEqual(new Vector2(5f, 5f), hashSetAvg, "Non-list collection average for 2D vectors must equal (5, 5).");
        }

        [Test]
        public void VectorExtensions_Average_3D_ListAndNonList()
        {
            var list = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(0, 10, 0),
                new Vector3(0, 0, 10)
            };

            Vector3 listAvg = list.Average();
            Assert.AreEqual(new Vector3(2.5f, 2.5f, 2.5f), listAvg, "List average for 3D vectors must equal (2.5, 2.5, 2.5).");

            var hashSet = new HashSet<Vector3>(list);
            Vector3 hashSetAvg = hashSet.Average();
            Assert.AreEqual(new Vector3(2.5f, 2.5f, 2.5f), hashSetAvg, "Non-list collection average for 3D vectors must equal (2.5, 2.5, 2.5).");
        }

        [Test]
        public void VectorExtensions_Median_EmptyCollections_ReturnsZero()
        {
            var emptyList2D = new List<Vector2>();
            Assert.AreEqual(Vector2.zero, emptyList2D.Median());
            Assert.AreEqual(Vector2.zero, emptyList2D.Where(v => true).Median());

            var emptyList3D = new List<Vector3>();
            Assert.AreEqual(Vector3.zero, emptyList3D.Median());
            Assert.AreEqual(Vector3.zero, emptyList3D.Where(v => true).Median());
        }

        [Test]
        public void VectorExtensions_Median_OddAndEvenCount_Accuracy()
        {
            // Odd count (3 elements)
            var odd3D = new List<Vector3>
            {
                new Vector3(10, -5, 100),
                new Vector3(2, 20, -50),
                new Vector3(5, 0, 0)
            };
            // Sorted X: 2, 5, 10 -> mid is 5
            // Sorted Y: -5, 0, 20 -> mid is 0
            // Sorted Z: -50, 0, 100 -> mid is 0
            Vector3 oddMedian = odd3D.Median();
            Assert.AreEqual(new Vector3(5f, 0f, 0f), oddMedian);

            // Even count (4 elements, mid = index 4 / 2 = 2)
            var even2D = new List<Vector2>
            {
                new Vector2(10, 40),
                new Vector2(20, 30),
                new Vector2(30, 20),
                new Vector2(40, 10)
            };
            // Sorted X: 10, 20, 30, 40 -> index 2 is 30
            // Sorted Y: 10, 20, 30, 40 -> index 2 is 30
            Vector2 evenMedian = even2D.Median();
            Assert.AreEqual(new Vector2(30f, 30f), evenMedian);
        }

        [Test]
        public void VectorExtensions_Median_LargeCollection_TriggersQuickSort()
        {
            // Create collection with 101 elements (> 32) to exercise the QuickSort partition branch in Sort()
            const int count = 101;
            var list2D = new List<Vector2>(count);
            var list3D = new List<Vector3>(count);

            for (int i = 0; i < count; i++)
            {
                // Deterministic pseudo-random pattern with negative values and duplicates
                float x = ((i * 37) % 50) - 25f;
                float y = ((i * 19) % 40) - 20f;
                float z = ((i * 73) % 60) - 30f;
                list2D.Add(new Vector2(x, y));
                list3D.Add(new Vector3(x, y, z));
            }

            // Expected median from Linq sort
            float expectedX = list3D.Select(v => v.x).OrderBy(v => v).ElementAt(count / 2);
            float expectedY = list3D.Select(v => v.y).OrderBy(v => v).ElementAt(count / 2);
            float expectedZ = list3D.Select(v => v.z).OrderBy(v => v).ElementAt(count / 2);

            Vector2 median2D = list2D.Median();
            Assert.AreEqual(new Vector2(expectedX, expectedY), median2D, "2D median on >32 items must match exact sort midpoint.");

            Vector3 median3D = list3D.Median();
            Assert.AreEqual(new Vector3(expectedX, expectedY, expectedZ), median3D, "3D median on >32 items must match exact sort midpoint.");

            // Test IEnumerable overload (non-ICollection path)
            Vector3 enumerableMedian = list3D.Where(v => true).Median();
            Assert.AreEqual(new Vector3(expectedX, expectedY, expectedZ), enumerableMedian, "IEnumerable<Vector3> median must match ICollection result.");
        }

        #endregion VectorExtensions Tests

        #region MathUtilities Tests

        [Test]
        public void MathUtilities_NearestPointToLinesLeastSquares_IntersectionAndAccuracy()
        {
            // 3 lines intersecting precisely at (3, -2, 7)
            Vector3 expectedPoint = new Vector3(3f, -2f, 7f);
            var rays = new List<Ray>
            {
                new Ray(new Vector3(0f, -2f, 7f), Vector3.right),
                new Ray(new Vector3(3f, 0f, 7f), Vector3.up),
                new Ray(new Vector3(3f, -2f, 0f), Vector3.forward)
            };

            Vector3 computedPoint = MathUtilities.NearestPointToLinesLeastSquares(rays);
            Assert.AreEqual(expectedPoint.x, computedPoint.x, 0.001f);
            Assert.AreEqual(expectedPoint.y, computedPoint.y, 0.001f);
            Assert.AreEqual(expectedPoint.z, computedPoint.z, 0.001f);
        }

        [Test]
        public void MathUtilities_NearestPointToLinesLeastSquares_IList_Matches_IEnumerable()
        {
            // Skew lines that do not strictly intersect
            var rays = new List<Ray>
            {
                new Ray(new Vector3(0, 1, 0), new Vector3(1, 0, 0)),
                new Ray(new Vector3(0, 0, 1), new Vector3(0, 1, 0)),
                new Ray(new Vector3(1, 0, 0), new Vector3(0, 0, 1))
            };

            Vector3 fromList = MathUtilities.NearestPointToLinesLeastSquares(rays);
            Vector3 fromEnumerable = MathUtilities.NearestPointToLinesLeastSquares(rays.Where(r => true));

            Assert.AreEqual(fromList.x, fromEnumerable.x, 0.0001f);
            Assert.AreEqual(fromList.y, fromEnumerable.y, 0.0001f);
            Assert.AreEqual(fromList.z, fromEnumerable.z, 0.0001f);
        }

        #endregion MathUtilities Tests

        #region EnumerableExtensions Tests

        private class TestDisposable : IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;
        }

        [Test]
        public void EnumerableExtensions_DisposeElements_NullAndEmpty_DoesNotThrow()
        {
            List<TestDisposable> nullList = null;
            Assert.DoesNotThrow(() => nullList.DisposeElements());

            var emptyList = new List<TestDisposable>();
            Assert.DoesNotThrow(() => emptyList.DisposeElements());
        }

        [Test]
        public void EnumerableExtensions_DisposeElements_ListAndNonList_DisposesAll()
        {
            // Test IList path
            var itemsList = new List<TestDisposable>
            {
                new TestDisposable(),
                new TestDisposable(),
                new TestDisposable()
            };
            itemsList.DisposeElements();
            Assert.IsTrue(itemsList.All(i => i.Disposed), "All elements in List must be disposed.");

            // Test non-IList IEnumerable path
            var linkedList = new LinkedList<TestDisposable>();
            var item1 = new TestDisposable();
            var item2 = new TestDisposable();
            linkedList.AddLast(item1);
            linkedList.AddLast(item2);
            linkedList.DisposeElements();
            Assert.IsTrue(item1.Disposed && item2.Disposed, "All elements in non-list IEnumerable must be disposed.");

            // Test list with null entry
            var listWithNull = new List<TestDisposable>
            {
                new TestDisposable(),
                null,
                new TestDisposable()
            };
            Assert.DoesNotThrow(() => listWithNull.DisposeElements());
            Assert.IsTrue(listWithNull[0].Disposed);
            Assert.IsTrue(listWithNull[2].Disposed);
        }

        #endregion EnumerableExtensions Tests

        #region TransformExtensions Tests

        [UnityTest]
        public IEnumerator TransformExtensions_FindAncestorComponent_IncludeSelf()
        {
            GameObject parent = new GameObject("Parent");
            BoxCollider parentBox = parent.AddComponent<BoxCollider>();

            GameObject child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            SphereCollider childSphere = child.AddComponent<SphereCollider>();

            yield return null;

            // includeSelf = true should find component on self first
            Collider foundOnSelf = child.transform.FindAncestorComponent<Collider>(includeSelf: true);
            Assert.AreSame(childSphere, foundOnSelf, "FindAncestorComponent(includeSelf: true) must return component on self.");

            // includeSelf = false should skip self and find parent component
            Collider foundOnParent = child.transform.FindAncestorComponent<Collider>(includeSelf: false);
            Assert.AreSame(parentBox, foundOnParent, "FindAncestorComponent(includeSelf: false) must return parent component.");

            // Looking for non-existent component should return null without throwing
            AudioSource nonExistent = child.transform.FindAncestorComponent<AudioSource>(includeSelf: true);
            Assert.IsNull(nonExistent);

            UnityEngine.Object.Destroy(parent);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TransformExtensions_GetDepth_RootAndUnrelated()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            GameObject grandchild = new GameObject("Grandchild");
            grandchild.transform.SetParent(child.transform);

            GameObject unrelated = new GameObject("Unrelated");

            yield return null;

            // Root transform has no parent; depth must be -1
            Assert.AreEqual(-1, root.transform.GetDepth(), "Root transform depth must be -1.");
            Assert.AreEqual(-1, unrelated.transform.GetDepth(), "Unparented transform depth must be -1.");

            // Child depth is 0, Grandchild depth is 1
            Assert.AreEqual(0, child.transform.GetDepth(), "Child of root depth must be 0.");
            Assert.AreEqual(1, grandchild.transform.GetDepth(), "Grandchild of root depth must be 1.");

            // TryGetDepth on target == parent returns false
            int depth = 0;
            Assert.IsFalse(TransformExtensions.TryGetDepth(child.transform, child.transform, ref depth));

            // TryGetDepth on unrelated transform returns false
            depth = 0;
            Assert.IsFalse(TransformExtensions.TryGetDepth(child.transform, unrelated.transform, ref depth));
            Assert.AreEqual(0, depth, "Unrelated TryGetDepth should not modify depth.");

            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(unrelated);
            yield return null;
        }

        #endregion TransformExtensions Tests
    }
}

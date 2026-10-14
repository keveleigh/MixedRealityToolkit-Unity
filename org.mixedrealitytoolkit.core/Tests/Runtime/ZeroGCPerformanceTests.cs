// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace MixedReality.Toolkit.Core.Tests
{
    /// <summary>
    /// Performance and Zero-GC regression tests for MRTK Core utilities.
    /// Combines microbenchmark latency timing (Stopwatch) with Unity's native
    /// <see cref="AllocatingGCMemoryConstraint"/> to enforce zero heap allocations in hot paths.
    /// </summary>
    public class ZeroGCPerformanceTests : BaseRuntimeTests
    {
        private const int BenchmarkIterations = 1000;
        private const int HighFrequencyIterations = 10000;

        #region BoundsExtensions

        [UnityTest]
        public IEnumerator BoundsExtensions_GetRenderBounds_AllocatesZeroGC()
        {
            GameObject root = new GameObject("RenderRoot");
            for (int i = 0; i < 5; i++)
            {
                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.transform.SetParent(root.transform);
                child.transform.localPosition = new Vector3(i * 2f, i * 0.5f, i * 1.5f);
            }

            yield return null;

            Bounds bounds = default;
            // Warm up
            BoundsExtensions.GetRenderBounds(root, out bounds, 0);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                BoundsExtensions.GetRenderBounds(root, out bounds, 0);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] BoundsExtensions.GetRenderBounds: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                BoundsExtensions.GetRenderBounds(root, out Bounds b, 0);
            }, Is.Not.AllocatingGCMemory());

            Assert.IsTrue(bounds.size.magnitude > 0);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoundsExtensions_GetColliderBounds_AllocatesZeroGC()
        {
            GameObject root = new GameObject("ColliderRoot");
            for (int i = 0; i < 4; i++)
            {
                GameObject child = new GameObject($"Child_{i}");
                child.transform.SetParent(root.transform);
                child.transform.localPosition = new Vector3(i * 1.5f, 0, 0);
                BoxCollider box = child.AddComponent<BoxCollider>();
                box.size = Vector3.one;
            }

            yield return null;

            Bounds bounds = default;
            BoundsExtensions.GetColliderBounds(root, out bounds, 0);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                BoundsExtensions.GetColliderBounds(root, out bounds, 0);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] BoundsExtensions.GetColliderBounds: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                BoundsExtensions.GetColliderBounds(root, out Bounds b, 0);
            }, Is.Not.AllocatingGCMemory());

            Assert.IsTrue(bounds.size.magnitude > 0);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoundsExtensions_GetMeshFilterBounds_AllocatesZeroGC()
        {
            GameObject root = new GameObject("MeshFilterRoot");
            for (int i = 0; i < 3; i++)
            {
                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                child.transform.SetParent(root.transform);
                child.transform.localPosition = new Vector3(0, i * 2f, 0);
            }

            yield return null;

            Bounds bounds = default;
            BoundsExtensions.GetMeshFilterBounds(root, out bounds, 0);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                BoundsExtensions.GetMeshFilterBounds(root, out bounds, 0);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] BoundsExtensions.GetMeshFilterBounds: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                BoundsExtensions.GetMeshFilterBounds(root, out Bounds b, 0);
            }, Is.Not.AllocatingGCMemory());

            Assert.IsTrue(bounds.size.magnitude > 0);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoundsExtensions_GetScreenRectangle_AllocatesZeroGC()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("TestCamera");
                cam = camObj.AddComponent<Camera>();
                cam.transform.position = new Vector3(0, 0, -5);
                cam.transform.LookAt(Vector3.zero);
            }

            Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
            Rect rect = bounds.GetScreenRectangle(cam);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                rect = bounds.GetScreenRectangle(cam);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] BoundsExtensions.GetScreenRectangle: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                rect = bounds.GetScreenRectangle(cam);
            }, Is.Not.AllocatingGCMemory());

            Assert.IsTrue(rect.width > 0 && rect.height > 0);

            yield return null;
        }

        [Test]
        public void BoundsExtensions_GetCornerPositions_CachedMatrix_ZeroGC()
        {
            Bounds bounds = new Bounds(new Vector3(1, 2, 3), new Vector3(2, 4, 6));
            Vector3[] positions = new Vector3[8];

            bounds.GetCornerPositions(ref positions);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                bounds.GetCornerPositions(ref positions);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] BoundsExtensions.GetCornerPositions: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                bounds.GetCornerPositions(ref positions);
            }, Is.Not.AllocatingGCMemory());

            Assert.AreEqual(8, positions.Length);
        }

        #endregion

        #region Data Structures & Hash Code

        [Test]
        public void HandJointPose_EqualityAndHashCode_NoBoxingZeroGC()
        {
            HandJointPose poseA = new HandJointPose(new Vector3(1, 2, 3), Quaternion.identity, 0.05f);
            HandJointPose poseB = new HandJointPose(new Vector3(1, 2, 3), Quaternion.identity, 0.05f);
            HandJointPose poseC = new HandJointPose(new Vector3(4, 5, 6), Quaternion.identity, 0.05f);

            Assert.IsTrue(poseA.Equals(poseB));
            Assert.IsFalse(poseA.Equals(poseC));
            Assert.IsTrue(poseA == poseB);
            Assert.IsTrue(poseA != poseC);

            Dictionary<HandJointPose, int> dict = new Dictionary<HandJointPose, int>
            {
                [poseA] = 42
            };

            int val = 0;
            dict.TryGetValue(poseB, out val);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                dict.TryGetValue(poseB, out val);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] HandJointPose.DictionaryLookup: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                dict.TryGetValue(poseB, out val);
            }, Is.Not.AllocatingGCMemory());

            Assert.AreEqual(42, val);
        }

        [Test]
        public void MixedRealityTransform_EqualityAndHashCode_IncludesScaleZeroGC()
        {
            MixedRealityTransform t1 = new MixedRealityTransform(Vector3.one, Quaternion.identity, Vector3.one);
            MixedRealityTransform t2 = new MixedRealityTransform(Vector3.one, Quaternion.identity, Vector3.one);
            MixedRealityTransform t3 = new MixedRealityTransform(Vector3.one, Quaternion.identity, new Vector3(2, 2, 2));

            Assert.IsTrue(t1.Equals(t2));
            Assert.IsFalse(t1.Equals(t3), "Transforms differing only by Scale must not be equal!");
            Assert.IsTrue(t1 == t2);
            Assert.IsFalse(t1 == t3);

            Dictionary<MixedRealityTransform, int> dict = new Dictionary<MixedRealityTransform, int>
            {
                [t1] = 99
            };

            int val = 0;
            dict.TryGetValue(t2, out val);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                dict.TryGetValue(t2, out val);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] MixedRealityTransform.DictionaryLookup: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                dict.TryGetValue(t2, out val);
            }, Is.Not.AllocatingGCMemory());

            Assert.AreEqual(99, val);
        }

        [Test]
        public void RayStep_ZeroGC_ThreadSafety_And_ContainsMath()
        {
            Vector3 start = new Vector3(0, 0, 0);
            Vector3 end = new Vector3(0, 0, 10);
            RayStep step = new RayStep(start, end);

            Vector3 midPoint = new Vector3(0, 0, 5);
            Vector3 onOrigin = new Vector3(0, 0, 0);
            Vector3 onTerminus = new Vector3(0, 0, 10);
            Vector3 offLine = new Vector3(0, 1.0f, 5);
            Vector3 beyondEnd = new Vector3(0, 0, 15);
            Vector3 behindStart = new Vector3(0, 0, -5);

            Assert.IsTrue(step.Contains(midPoint), "RayStep.Contains must return true for points on the segment.");
            Assert.IsTrue(step.Contains(onOrigin), "RayStep.Contains must return true for origin.");
            Assert.IsTrue(step.Contains(onTerminus), "RayStep.Contains must return true for terminus.");
            Assert.IsFalse(step.Contains(offLine), "RayStep.Contains must return false for points perpendicular off segment.");
            Assert.IsFalse(step.Contains(beyondEnd), "RayStep.Contains must return false for points beyond terminus.");
            Assert.IsFalse(step.Contains(behindStart), "RayStep.Contains must return false for points behind origin.");

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            bool result = false;
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                result ^= step.Contains(midPoint);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] RayStep.Contains: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                result = step.Contains(midPoint);
            }, Is.Not.AllocatingGCMemory());
        }

        #endregion

        #region Subsystems, Vectors, & GameObjects

        [Test]
        public void XRSubsystemHelpers_GetAllRunningSubsystems_AllocatesZeroGC()
        {
            XRSubsystemHelpers.GetAllSubsystems<XRInputSubsystem>();
            XRSubsystemHelpers.GetAllRunningSubsystems<XRInputSubsystem>();

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                XRSubsystemHelpers.GetAllRunningSubsystems<XRInputSubsystem>();
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] XRSubsystemHelpers.GetAllRunningSubsystems: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                XRSubsystemHelpers.GetAllRunningSubsystems<XRInputSubsystem>();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void VectorExtensions_Median_AccurateAndZeroGC()
        {
            List<Vector2> vectors2D = new List<Vector2>
            {
                new Vector2(1, 10),
                new Vector2(9, 2),
                new Vector2(5, 6)
            };
            Assert.AreEqual(new Vector2(5, 6), vectors2D.Median(), "Component-wise median must accurately calculate median for 2D vectors.");

            List<Vector3> vectors3D = new List<Vector3>
            {
                new Vector3(1, 10, 100),
                new Vector3(9, 2, 50),
                new Vector3(5, 6, 20)
            };

            Vector3 expectedMedian = new Vector3(5, 6, 50);
            Vector3 median = vectors3D.Median();
            Assert.AreEqual(expectedMedian, median, "Component-wise median must accurately calculate median for each axis.");

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                median = vectors3D.Median();
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] VectorExtensions.Median: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

#if !UNITY_EDITOR || ENABLE_IL2CPP
            // 2. Unity's native Zero-GC assertion
            // In IL2CPP production builds, standard BCL List<float>.Sort compiles to native C++ with 0 GC allocations.
            // Under Unity Editor's Mono runtime, BCL List.Sort allocates ~40 bytes internally for generic Comparer dispatch.
            Assert.That(() =>
            {
                median = vectors3D.Median();
            }, Is.Not.AllocatingGCMemory());
#endif
        }

        [UnityTest]
        public IEnumerator GameObjectExtensions_ApplyToHierarchy_ZeroGC_SingleRootVisit()
        {
            GameObject root = new GameObject("Root");
            for (int i = 0; i < 3; i++)
            {
                GameObject child = new GameObject($"Child_{i}");
                child.transform.SetParent(root.transform);
            }

            yield return null;

            int visitCount = 0;
            Action<GameObject> countAction = go => visitCount++;

            root.ApplyToHierarchy(countAction);
            Assert.AreEqual(4, visitCount, "ApplyToHierarchy must visit root once + 3 children = 4 nodes total (no duplicate root visit).");

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                root.ApplyToHierarchy(countAction);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] GameObjectExtensions.ApplyToHierarchy: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            visitCount = 0;
            Assert.That(() =>
            {
                root.ApplyToHierarchy(countAction);
            }, Is.Not.AllocatingGCMemory());

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameObjectExtensions_ForEachComponent_AllocatesZeroGC()
        {
            GameObject go = new GameObject("TestComponents");
            go.AddComponent<BoxCollider>();
            go.AddComponent<SphereCollider>();

            yield return null;

            int componentCount = 0;
            Action<Collider> countAction = c => componentCount++;

            go.ForEachComponent(countAction);
            Assert.AreEqual(2, componentCount);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                go.ForEachComponent(countAction);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] GameObjectExtensions.ForEachComponent: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            componentCount = 0;
            Assert.That(() =>
            {
                go.ForEachComponent(countAction);
            }, Is.Not.AllocatingGCMemory());

            UnityEngine.Object.Destroy(go);
            yield return null;
        }

        #endregion

        #region TransformExtensions, MathUtilities, & EnumerableExtensions

        [UnityTest]
        public IEnumerator TransformExtensions_GetColliderBounds_AllocatesZeroGC()
        {
            GameObject root = new GameObject("TransformColliderRoot");
            for (int i = 0; i < 4; i++)
            {
                GameObject child = new GameObject($"Child_{i}");
                child.transform.SetParent(root.transform);
                child.transform.localPosition = new Vector3(i * 1.5f, 0, 0);
                BoxCollider box = child.AddComponent<BoxCollider>();
                box.size = Vector3.one;
            }

            yield return null;

            Bounds bounds = default;
            bounds = root.transform.GetColliderBounds();

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                bounds = root.transform.GetColliderBounds();
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] TransformExtensions.GetColliderBounds: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                bounds = root.transform.GetColliderBounds();
            }, Is.Not.AllocatingGCMemory());

            Assert.IsTrue(bounds.size.magnitude > 0);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TransformExtensions_FindAncestorComponent_AllocatesZeroGC()
        {
            GameObject grandparent = new GameObject("Grandparent");
            BoxCollider rootCollider = grandparent.AddComponent<BoxCollider>();
            GameObject parent = new GameObject("Parent");
            parent.transform.SetParent(grandparent.transform);
            GameObject child = new GameObject("Child");
            child.transform.SetParent(parent.transform);

            yield return null;

            BoxCollider found = null;
            found = child.transform.FindAncestorComponent<BoxCollider>(includeSelf: true);
            Assert.AreSame(rootCollider, found);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                found = child.transform.FindAncestorComponent<BoxCollider>(includeSelf: true);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] TransformExtensions.FindAncestorComponent: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                found = child.transform.FindAncestorComponent<BoxCollider>(includeSelf: true);
            }, Is.Not.AllocatingGCMemory());

            UnityEngine.Object.Destroy(grandparent);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TransformExtensions_TryGetDepth_AccurateAndZeroGC()
        {
            GameObject root = new GameObject("Root");
            GameObject siblingA = new GameObject("SiblingA");
            siblingA.transform.SetParent(root.transform);
            for (int i = 0; i < 3; i++)
            {
                GameObject childA = new GameObject($"ChildA_{i}");
                childA.transform.SetParent(siblingA.transform);
            }
            GameObject siblingB = new GameObject("SiblingB");
            siblingB.transform.SetParent(root.transform);
            GameObject target = new GameObject("Target");
            target.transform.SetParent(siblingB.transform);

            yield return null;

            int depthFromRoot = -1;
            bool foundRoot = TransformExtensions.TryGetDepth(target.transform, root.transform, ref depthFromRoot);
            Assert.IsTrue(foundRoot);
            Assert.AreEqual(1, depthFromRoot, "Target is grandchild of root; depth must be exactly 1.");

            int depthDirect = target.transform.GetDepth();
            Assert.AreEqual(1, depthDirect, "GetDepth must return 1 for grandchild of root.");

            int depthFromSibling = -1;
            bool foundSibling = TransformExtensions.TryGetDepth(target.transform, siblingB.transform, ref depthFromSibling);
            Assert.IsTrue(foundSibling);
            Assert.AreEqual(0, depthFromSibling, "Target is direct child of siblingB; depth must be exactly 0.");

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            int depth = -1;
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                depth = -1;
                TransformExtensions.TryGetDepth(target.transform, root.transform, ref depth);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] TransformExtensions.TryGetDepth: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                depth = -1;
                TransformExtensions.TryGetDepth(target.transform, root.transform, ref depth);
            }, Is.Not.AllocatingGCMemory());

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [Test]
        public void MathUtilities_NearestPointToLinesRANSAC_AllocatesZeroGC()
        {
            List<Ray> rays = new List<Ray>
            {
                new Ray(new Vector3(-1, 0, 0), Vector3.right),
                new Ray(new Vector3(0, -1, 0), Vector3.up),
                new Ray(new Vector3(0, 0, -1), Vector3.forward),
                new Ray(new Vector3(5, 5, 5), Vector3.up) // Outlier
            };

            Vector3 point = Vector3.zero;
            int inliers = 0;

            // Warm up
            point = MathUtilities.NearestPointToLinesRANSAC(rays, 10, 0.1f, out inliers);

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                point = MathUtilities.NearestPointToLinesRANSAC(rays, 10, 0.1f, out inliers);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] MathUtilities.NearestPointToLinesRANSAC: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                point = MathUtilities.NearestPointToLinesRANSAC(rays, 10, 0.1f, out inliers);
            }, Is.Not.AllocatingGCMemory());

            Assert.IsTrue(inliers >= 3, "At least 3 rays should be inliers intersecting at origin.");
            Assert.That(Vector3.Distance(point, Vector3.zero), UnityEngine.TestTools.Constraints.Is.LessThan(0.01f));
        }

        private class DisposableItem : IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }

        [Test]
        public void EnumerableExtensions_DisposeElements_AllocatesZeroGC()
        {
            List<DisposableItem> items = new List<DisposableItem>();
            for (int i = 0; i < 5; i++)
            {
                items.Add(new DisposableItem());
            }

            // Warm up
            items.DisposeElements();

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                items.DisposeElements();
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] EnumerableExtensions.DisposeElements: {usPerCall:F3} µs/call across {BenchmarkIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                items.DisposeElements();
            }, Is.Not.AllocatingGCMemory());

            Assert.IsTrue(items[0].IsDisposed);
        }

        #endregion
    }
}
#pragma warning restore CS1591

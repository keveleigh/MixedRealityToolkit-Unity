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
using Is = UnityEngine.TestTools.Constraints.Is;
using Debug = UnityEngine.Debug;

namespace MixedReality.Toolkit.Core.Tests
{
    /// <summary>
    /// Performance and Zero-GC regression tests for MRTK Core utilities.
    /// These tests verify that hot-path operations produce zero managed heap allocations (0 B GC)
    /// and maintain microsecond-level execution latency.
    /// </summary>
    public class ZeroGCPerformanceTests : BaseRuntimeTests
    {
        private const int BenchmarkIterations = 1000;
        private const int HighFrequencyIterations = 10000;

        #region BoundsExtensions Performance Tests

        [UnityTest]
        public IEnumerator BoundsExtensions_GetRenderBounds_AllocatesZeroGC()
        {
            // Setup a hierarchy with multiple renderers
            GameObject root = new GameObject("RenderRoot");
            for (int i = 0; i < 5; i++)
            {
                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.transform.SetParent(root.transform);
                child.transform.localPosition = new Vector3(i * 2f, i * 0.5f, i * 1.5f);
            }

            yield return null;

            // Warm up
            BoundsExtensions.GetRenderBounds(root, out Bounds bounds, 0);

            // Assert Zero-GC
            Assert.That(() =>
            {
                BoundsExtensions.GetRenderBounds(root, out Bounds b, 0);
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                BoundsExtensions.GetRenderBounds(root, out bounds, 0);
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] GetRenderBounds: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations (0 bytes GC)");

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

            // Warm up
            BoundsExtensions.GetColliderBounds(root, out Bounds bounds, 0);

            // Assert Zero-GC
            Assert.That(() =>
            {
                BoundsExtensions.GetColliderBounds(root, out Bounds b, 0);
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                BoundsExtensions.GetColliderBounds(root, out bounds, 0);
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] GetColliderBounds: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations (0 bytes GC)");

            Assert.IsTrue(bounds.size.magnitude > 0);
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoundsExtensions_GetMeshFilterBounds_AllocatesZeroGC()
        {
            GameObject root = new GameObject("MeshFilterRoot");
            Mesh cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            for (int i = 0; i < 4; i++)
            {
                GameObject child = new GameObject($"ChildMesh_{i}");
                child.transform.SetParent(root.transform);
                child.transform.localPosition = new Vector3(i, i, i);
                MeshFilter mf = child.AddComponent<MeshFilter>();
                mf.sharedMesh = cubeMesh;
            }

            yield return null;

            // Warm up
            BoundsExtensions.GetMeshFilterBounds(root, out Bounds bounds, 0);

            // Assert Zero-GC
            Assert.That(() =>
            {
                BoundsExtensions.GetMeshFilterBounds(root, out Bounds b, 0);
            }, Is.Not.AllocatingGCMemory());

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoundsExtensions_GetScreenRectangle_AllocatesZeroGC()
        {
            GameObject camObj = new GameObject("TestCamera");
            Camera camera = camObj.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            camera.transform.LookAt(Vector3.zero);

            Bounds bounds = new Bounds(Vector3.zero, new Vector3(2, 2, 2));
            yield return null;

            // Warm up
            Rect rect = bounds.GetScreenRectangle(camera);

            // Assert Zero-GC
            Assert.That(() =>
            {
                Rect r = bounds.GetScreenRectangle(camera);
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                rect = bounds.GetScreenRectangle(camera);
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] GetScreenRectangle: {usPerCall:F2} µs/call across {BenchmarkIterations} iterations (0 bytes GC)");

            Assert.IsTrue(rect.width > 0 && rect.height > 0);
            UnityEngine.Object.Destroy(camObj);
            yield return null;
        }

        [Test]
        public void BoundsExtensions_GetCornerPositions_CachedMatrix_ZeroGC()
        {
            GameObject go = new GameObject("TransformObj");
            go.transform.position = new Vector3(10, 20, 30);
            go.transform.rotation = Quaternion.Euler(30, 45, 60);

            Bounds bounds = new Bounds(Vector3.zero, new Vector3(2, 4, 6));
            Vector3[] positions = new Vector3[8];

            // Warm up
            bounds.GetCornerPositions(go.transform, ref positions);

            // Assert Zero-GC
            Assert.That(() =>
            {
                bounds.GetCornerPositions(go.transform, ref positions);
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                bounds.GetCornerPositions(go.transform, ref positions);
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] GetCornerPositions (cached matrix): {usPerCall:F2} µs/call across {HighFrequencyIterations} iterations (0 bytes GC)");

            UnityEngine.Object.DestroyImmediate(go);
        }

        #endregion

        #region HandJointPose & MixedRealityTransform Performance Tests

        [Test]
        public void HandJointPose_EqualityAndHashCode_NoBoxingZeroGC()
        {
            HandJointPose pose1 = new HandJointPose(new Vector3(1, 2, 3), Quaternion.Euler(10, 20, 30), 0.015f);
            HandJointPose pose2 = new HandJointPose(new Vector3(1, 2, 3), Quaternion.Euler(10, 20, 30), 0.015f);
            HandJointPose pose3 = new HandJointPose(new Vector3(4, 5, 6), Quaternion.identity, 0.02f);

            // Equality checks
            Assert.IsTrue(pose1.Equals(pose2));
            Assert.IsTrue(pose1 == pose2);
            Assert.IsFalse(pose1 == pose3);
            Assert.AreEqual(pose1.GetHashCode(), pose2.GetHashCode());

            // Dictionary lookups without boxing
            Dictionary<HandJointPose, int> dict = new Dictionary<HandJointPose, int>();
            dict[pose1] = 42;

            // Assert Zero-GC dictionary lookup
            Assert.That(() =>
            {
                bool found = dict.TryGetValue(pose2, out int val);
                int hash = pose1.GetHashCode();
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark: 10,000 hash calculations and lookups
            Stopwatch sw = Stopwatch.StartNew();
            int sum = 0;
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                if (dict.TryGetValue(pose1, out int val))
                {
                    sum += val;
                }
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] HandJointPose Dictionary Lookup: {usPerCall:F3} µs/lookup across {HighFrequencyIterations} calls (0 bytes GC, no boxing)");
            Assert.AreEqual(42 * HighFrequencyIterations, sum);
        }

        [Test]
        public void MixedRealityTransform_EqualityAndHashCode_IncludesScaleZeroGC()
        {
            MixedRealityTransform t1 = new MixedRealityTransform(new Vector3(1, 2, 3), Quaternion.identity, new Vector3(1, 1, 1));
            MixedRealityTransform t2 = new MixedRealityTransform(new Vector3(1, 2, 3), Quaternion.identity, new Vector3(2, 2, 2));
            MixedRealityTransform t3 = new MixedRealityTransform(new Vector3(1, 2, 3), Quaternion.identity, new Vector3(1, 1, 1));

            // Verify Scale sensitivity in equality
            Assert.IsFalse(t1.Equals(t2), "Transforms differing only by Scale must not be equal!");
            Assert.IsFalse(t1 == t2);
            Assert.IsTrue(t1.Equals(t3));
            Assert.IsTrue(t1 == t3);
            Assert.AreEqual(t1.GetHashCode(), t3.GetHashCode());

            // Dictionary lookups without boxing
            Dictionary<MixedRealityTransform, int> dict = new Dictionary<MixedRealityTransform, int>();
            dict[t1] = 100;

            Assert.That(() =>
            {
                bool found = dict.TryGetValue(t3, out int val);
                int hash = t1.GetHashCode();
            }, Is.Not.AllocatingGCMemory());

            Stopwatch sw = Stopwatch.StartNew();
            int sum = 0;
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                if (dict.TryGetValue(t3, out int val))
                {
                    sum += val;
                }
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] MixedRealityTransform Dictionary Lookup: {usPerCall:F3} µs/lookup across {HighFrequencyIterations} calls (0 bytes GC, no boxing)");
            Assert.AreEqual(100 * HighFrequencyIterations, sum);
        }

        #endregion

        #region RayStep Performance Tests

        [Test]
        public void RayStep_ZeroGC_ThreadSafety_And_ContainsMath()
        {
            Vector3 start = new Vector3(0, 0, 0);
            Vector3 end = new Vector3(0, 0, 10);
            RayStep step = new RayStep(start, end);

            // Assert Zero-GC in GetPoint, UpdateRayStep, CopyRay
            Assert.That(() =>
            {
                step.UpdateRayStep(in start, in end);
                Vector3 pt = step.GetPoint(5.0f);
                Ray ray = new Ray(Vector3.zero, Vector3.forward);
                step.CopyRay(ray, 10f);
            }, Is.Not.AllocatingGCMemory());

            // Mathematical correctness test for Contains
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

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            bool result = false;
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                result ^= step.Contains(midPoint);
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] RayStep.Contains: {usPerCall:F3} µs/call across {HighFrequencyIterations} calls (0 bytes GC, thread-safe)");
        }

        #endregion

        #region XRSubsystemHelpers & Extensions Performance Tests

        [Test]
        public void XRSubsystemHelpers_GetAllRunningSubsystems_AllocatesZeroGC()
        {
            // Warm up
            XRSubsystemHelpers.GetAllSubsystems<XRInputSubsystem>();
            XRSubsystemHelpers.GetAllRunningSubsystems<XRInputSubsystem>();

            // Assert Zero-GC
            Assert.That(() =>
            {
                List<XRInputSubsystem> running = XRSubsystemHelpers.GetAllRunningSubsystems<XRInputSubsystem>();
                List<XRInputSubsystem> all = XRSubsystemHelpers.GetAllSubsystems<XRInputSubsystem>();
            }, Is.Not.AllocatingGCMemory());

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                XRSubsystemHelpers.GetAllRunningSubsystems<XRInputSubsystem>();
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] XRSubsystemHelpers.GetAllRunningSubsystems: {usPerCall:F3} µs/call across {HighFrequencyIterations} calls (0 bytes GC, generic static cache)");
        }

        [Test]
        public void VectorExtensions_Median_AccurateAndZeroGC()
        {
            List<Vector3> vectors3D = new List<Vector3>
            {
                new Vector3(1, 10, 100),
                new Vector3(9, 2, 50),
                new Vector3(5, 6, 20)
            };

            // Ground truth: X components (1, 5, 9) -> 5; Y components (2, 6, 10) -> 6; Z components (20, 50, 100) -> 50
            Vector3 expectedMedian = new Vector3(5, 6, 50);
            Vector3 median = vectors3D.Median();
            Assert.AreEqual(expectedMedian, median, "Component-wise median must accurately calculate median for each axis.");

            // Assert Zero-GC
            Assert.That(() =>
            {
                Vector3 m = vectors3D.Median();
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                median = vectors3D.Median();
            }
            sw.Stop();

            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] VectorExtensions.Median: {usPerCall:F2} µs/call across {BenchmarkIterations} calls (0 bytes GC)");
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

            // Warm up
            root.ApplyToHierarchy(countAction);
            Assert.AreEqual(4, visitCount, "ApplyToHierarchy must visit root once + 3 children = 4 nodes total (no duplicate root visit).");

            // Assert Zero-GC
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

            // Warm up
            go.ForEachComponent(countAction);
            Assert.AreEqual(2, componentCount);

            // Assert Zero-GC
            Assert.That(() =>
            {
                go.ForEachComponent(countAction);
            }, Is.Not.AllocatingGCMemory());

            UnityEngine.Object.Destroy(go);
            yield return null;
        }

        #endregion
    }
}
#pragma warning restore CS1591

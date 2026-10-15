// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace MixedReality.Toolkit.SpatialManipulation.Runtime.Tests
{
    /// <summary>
    /// Comprehensive test suite verifying performance, Zero-GC behavior, null Camera safety,
    /// and behavioral correctness of Bounds Control, SqueezableBoxVisuals, Constraints, and Manipulation Logics.
    /// </summary>
    public class BoundsControlAndManipulationHotPathTests : BaseRuntimeInputTests
    {
        protected override InputTestUtilities.RigVersion RigVersion => InputTestUtilities.RigVersion.Version1;

        private const int HighFrequencyIterations = 1000;
        private const int MicrobenchmarkIterations = 20000;

        private GameObject testRoot;
        private List<GameObject> objectsToClean;

        public override IEnumerator Setup()
        {
            yield return base.Setup();
            testRoot = new GameObject("HotPathTestRoot");
            testRoot.transform.position = InputTestUtilities.InFrontOfUser(1.0f);
            objectsToClean = new List<GameObject> { testRoot };
        }

        public override IEnumerator TearDown()
        {
            if (objectsToClean != null)
            {
                for (int i = objectsToClean.Count - 1; i >= 0; i--)
                {
                    if (objectsToClean[i] != null)
                    {
                        Object.Destroy(objectsToClean[i]);
                    }
                }
                objectsToClean.Clear();
            }

            yield return base.TearDown();
        }

        private GameObject CreateCleanObject(string name, Transform parent = null)
        {
            GameObject go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent);
            }
            objectsToClean.Add(go);
            return go;
        }

        #region ScaleHandleInteractable Tests

        [UnityTest]
        public IEnumerator ScaleHandleInteractable_ProjectToScreen_ZeroGCAndPerf()
        {
            GameObject handleObj = CreateCleanObject("ScaleHandle", testRoot.transform);
            handleObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
            var handle = handleObj.AddComponent<ScaleHandleInteractable>();
            handle.IsOccluded = false;
            handle.IsFlattened = false;

            yield return null;

            MethodInfo lateUpdateMethod = typeof(ScaleHandleInteractable).GetMethod("LateUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(lateUpdateMethod, "ScaleHandleInteractable should have LateUpdate method.");
            Action lateUpdateAction = (Action)Delegate.CreateDelegate(typeof(Action), handle, lateUpdateMethod);

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                lateUpdateAction();
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                lateUpdateAction();
            }
            sw.Stop();
            Debug.Log($"[PERF_BENCHMARK] ScaleHandleInteractable.LateUpdate x{HighFrequencyIterations}: {sw.Elapsed.TotalMilliseconds:F3} ms ({(sw.Elapsed.TotalMilliseconds / HighFrequencyIterations) * 1000.0:F2} µs/call)");

            // Assert Zero GC
            Assert.That(() =>
            {
                lateUpdateAction();
            }, Is.Not.AllocatingGCMemory(), "ScaleHandleInteractable.LateUpdate must not allocate GC memory during execution.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ScaleHandleInteractable_NullCamera_DoesNotThrow()
        {
            GameObject handleObj = CreateCleanObject("ScaleHandleNullCam", testRoot.transform);
            handleObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
            var handle = handleObj.AddComponent<ScaleHandleInteractable>();
            handle.IsOccluded = false;
            handle.IsFlattened = false;

            yield return null;

            MethodInfo lateUpdateMethod = typeof(ScaleHandleInteractable).GetMethod("LateUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Action lateUpdateAction = (Action)Delegate.CreateDelegate(typeof(Action), handle, lateUpdateMethod);

            Camera cam = Camera.main;
            string prevTag = null;
            try
            {
                if (cam != null)
                {
                    prevTag = cam.tag;
                    cam.tag = "Untagged";
                }

                Assert.IsNull(Camera.main, "Camera.main must simulate null.");

                Assert.DoesNotThrow(() =>
                {
                    lateUpdateAction();
                }, "ScaleHandleInteractable.LateUpdate must safely handle null Camera.main without throwing NullReferenceException.");
            }
            finally
            {
                if (cam != null && prevTag != null)
                {
                    cam.tag = prevTag;
                }
            }

            yield return null;
        }

        #endregion

        #region SqueezableBoxVisuals Tests

        [UnityTest]
        public IEnumerator SqueezableBoxVisuals_ConvexHullAndFlatten_NullCamera_DoesNotThrow()
        {
            GameObject visualsObj = CreateCleanObject("SqueezableBoxVisuals", testRoot.transform);
            var visuals = visualsObj.AddComponent<SqueezableBoxVisuals>();

            // Add child handles
            GameObject handleObj1 = CreateCleanObject("ChildHandle1", visualsObj.transform);
            handleObj1.AddComponent<ScaleHandleInteractable>();
            GameObject handleObj2 = CreateCleanObject("ChildHandle2", visualsObj.transform);
            handleObj2.AddComponent<ScaleHandleInteractable>();

            yield return null;

            MethodInfo convexHullMethod = typeof(SqueezableBoxVisuals).GetMethod("ConvexHull",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(convexHullMethod, "SqueezableBoxVisuals should have ConvexHull method.");

            MethodInfo hideFlattenMethod = typeof(SqueezableBoxVisuals).GetMethod("HideFlattenedHandles",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(hideFlattenMethod, "SqueezableBoxVisuals should have HideFlattenedHandles method.");

            Camera cam = Camera.main;
            string prevTag = null;
            try
            {
                if (cam != null)
                {
                    prevTag = cam.tag;
                    cam.tag = "Untagged";
                }

                Assert.IsNull(Camera.main, "Camera.main must simulate null.");

                Assert.DoesNotThrow(() =>
                {
                    convexHullMethod.Invoke(visuals, null);
                }, "SqueezableBoxVisuals.ConvexHull must safely handle null Camera.main without throwing NullReferenceException.");

                Assert.DoesNotThrow(() =>
                {
                    hideFlattenMethod.Invoke(visuals, null);
                }, "SqueezableBoxVisuals.HideFlattenedHandles must safely handle null Camera.main without throwing NullReferenceException.");
            }
            finally
            {
                if (cam != null && prevTag != null)
                {
                    cam.tag = prevTag;
                }
            }

            yield return null;
        }

        #endregion

        #region Constraints Tests

        [UnityTest]
        public IEnumerator Constraints_NullCamera_DoNotThrow()
        {
            GameObject faceObj = CreateCleanObject("FaceUserObj", testRoot.transform);
            var faceUser = faceObj.AddComponent<FaceUserConstraint>();

            GameObject distObj = CreateCleanObject("FixedDistObj", testRoot.transform);
            var fixedDist = distObj.AddComponent<FixedDistanceConstraint>();

            GameObject rotObj = CreateCleanObject("FixedRotObj", testRoot.transform);
            var fixedRot = rotObj.AddComponent<FixedRotationToUserConstraint>();

            GameObject sizeObj = CreateCleanObject("MaintainSizeObj", testRoot.transform);
            var maintainSize = sizeObj.AddComponent<MaintainApparentSizeConstraint>();

            yield return null;

            Camera cam = Camera.main;
            string prevTag = null;
            try
            {
                if (cam != null)
                {
                    prevTag = cam.tag;
                    cam.tag = "Untagged";
                }

                Assert.IsNull(Camera.main, "Camera.main must simulate null.");

                var pose = new MixedRealityTransform(Vector3.forward * 2f, Quaternion.identity, Vector3.one);

                Assert.DoesNotThrow(() =>
                {
                    faceUser.ApplyConstraint(ref pose);
                }, "FaceUserConstraint.ApplyConstraint must safely handle null Camera.main without throwing NullReferenceException.");

                Assert.DoesNotThrow(() =>
                {
                    fixedDist.OnManipulationStarted(pose);
                    fixedDist.ApplyConstraint(ref pose);
                }, "FixedDistanceConstraint must safely handle null Camera.main without throwing NullReferenceException.");

                Assert.DoesNotThrow(() =>
                {
                    fixedRot.OnManipulationStarted(pose);
                    fixedRot.ApplyConstraint(ref pose);
                }, "FixedRotationToUserConstraint must safely handle null Camera.main without throwing NullReferenceException.");

                Assert.DoesNotThrow(() =>
                {
                    maintainSize.OnManipulationStarted(pose);
                    maintainSize.ApplyConstraint(ref pose);
                }, "MaintainApparentSizeConstraint must safely handle null Camera.main without throwing NullReferenceException.");
            }
            finally
            {
                if (cam != null && prevTag != null)
                {
                    cam.tag = prevTag;
                }
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator MaintainApparentSizeConstraint_ZeroDistance_DoesNotNaN()
        {
            GameObject sizeObj = CreateCleanObject("MaintainSizeZeroDist", testRoot.transform);
            var maintainSize = sizeObj.AddComponent<MaintainApparentSizeConstraint>();

            Camera cam = Camera.main;
            Assert.IsNotNull(cam);

            // Object coincident with camera -> distance is 0
            var pose = new MixedRealityTransform(cam.transform.position, Quaternion.identity, Vector3.one);
            maintainSize.OnManipulationStarted(pose);

            // Move slightly
            pose.Position = cam.transform.position + Vector3.forward * 0.5f;
            maintainSize.ApplyConstraint(ref pose);

            Assert.IsFalse(float.IsNaN(pose.Scale.x) || float.IsInfinity(pose.Scale.x), "Scale.x must not be NaN or Infinity.");
            Assert.IsFalse(float.IsNaN(pose.Scale.y) || float.IsInfinity(pose.Scale.y), "Scale.y must not be NaN or Infinity.");
            Assert.IsFalse(float.IsNaN(pose.Scale.z) || float.IsInfinity(pose.Scale.z), "Scale.z must not be NaN or Infinity.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Constraints_BehaviorAndZeroGC()
        {
            Camera cam = Camera.main;
            Assert.IsNotNull(cam);

            GameObject faceObj = CreateCleanObject("FaceUserObj", testRoot.transform);
            var faceUser = faceObj.AddComponent<FaceUserConstraint>();

            GameObject distObj = CreateCleanObject("FixedDistObj", testRoot.transform);
            var fixedDist = distObj.AddComponent<FixedDistanceConstraint>();
            fixedDist.ConstraintTransform = cam.transform;

            var startPose = new MixedRealityTransform(cam.transform.position + Vector3.forward * 2f, Quaternion.identity, Vector3.one);
            fixedDist.OnManipulationStarted(startPose);

            // Verify FaceUser constraint aligns forward vector to camera
            var pose = startPose;
            faceUser.ApplyConstraint(ref pose);
            Vector3 toCamera = (cam.transform.position - pose.Position).normalized;
            Assert.Less(Vector3.Angle(pose.Rotation * Vector3.forward, toCamera), 1.0f, "FaceUserConstraint should rotate forward towards camera.");

            // Verify FixedDistance constraint preserves distance
            pose.Position = cam.transform.position + Vector3.forward * 5f;
            fixedDist.ApplyConstraint(ref pose);
            float dist = Vector3.Distance(pose.Position, cam.transform.position);
            Assert.AreEqual(2f, dist, 0.001f, "FixedDistanceConstraint should restore original 2m distance.");

            // Zero-GC verification
            Assert.That(() =>
            {
                var p = startPose;
                faceUser.ApplyConstraint(ref p);
            }, Is.Not.AllocatingGCMemory(), "FaceUserConstraint.ApplyConstraint must be Zero-GC.");

            Assert.That(() =>
            {
                var p = startPose;
                fixedDist.ApplyConstraint(ref p);
            }, Is.Not.AllocatingGCMemory(), "FixedDistanceConstraint.ApplyConstraint must be Zero-GC.");

            yield return null;
        }

        #endregion

        #region MoveLogic, RotateLogic, ScaleLogic Tests

        [UnityTest]
        public IEnumerator RotateLogic_TwoHanded_ZeroGCAndAccuracy()
        {
            GameObject i1Obj = CreateCleanObject("Interactor1", testRoot.transform);
            i1Obj.transform.position = new Vector3(-0.2f, 0, 1);
            var i1 = i1Obj.AddComponent<XRDirectInteractor>();

            GameObject i2Obj = CreateCleanObject("Interactor2", testRoot.transform);
            i2Obj.transform.position = new Vector3(0.2f, 0, 1);
            var i2 = i2Obj.AddComponent<XRDirectInteractor>();

            GameObject targetObj = CreateCleanObject("Target", testRoot.transform);
            var interactable = targetObj.AddComponent<StatefulInteractable>();

            yield return null;

            var interactors = new List<IXRSelectInteractor> { i1, i2 };
            var currentTarget = new MixedRealityTransform(new Vector3(0, 0, 1), Quaternion.identity, Vector3.one);

            var rotateLogic = new RotateLogic();
            rotateLogic.Setup(interactors, interactable, currentTarget);

            // Rotate hands by 45 degrees
            i1Obj.transform.position = new Vector3(-0.1414f, 0.1414f, 1);
            i2Obj.transform.position = new Vector3(0.1414f, -0.1414f, 1);

            Quaternion updatedRot = rotateLogic.Update(interactors, interactable, currentTarget, false);
            Assert.Greater(Quaternion.Angle(currentTarget.Rotation, updatedRot), 20f, "Two-handed rotation should respond to interactor movement.");

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                _ = rotateLogic.Update(interactors, interactable, currentTarget, false);
            }

            // Benchmark
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                _ = rotateLogic.Update(interactors, interactable, currentTarget, false);
            }
            sw.Stop();
            Debug.Log($"[PERF_BENCHMARK] RotateLogic.Update (Two-Handed) x{HighFrequencyIterations}: {sw.Elapsed.TotalMilliseconds:F3} ms ({(sw.Elapsed.TotalMilliseconds / HighFrequencyIterations) * 1000.0:F2} µs/call)");

            // Assert Zero-GC
            Assert.That(() =>
            {
                _ = rotateLogic.Update(interactors, interactable, currentTarget, false);
            }, Is.Not.AllocatingGCMemory(), "RotateLogic.Update must not allocate GC memory in 2-handed mode.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator MoveLogic_SingleAndDualInteractor_AccuracyAndZeroGC()
        {
            GameObject i1Obj = CreateCleanObject("MoveInteractor1", testRoot.transform);
            i1Obj.transform.position = new Vector3(-0.2f, 0, 1);
            var i1 = i1Obj.AddComponent<XRDirectInteractor>();

            GameObject i2Obj = CreateCleanObject("MoveInteractor2", testRoot.transform);
            i2Obj.transform.position = new Vector3(0.2f, 0, 1);
            var i2 = i2Obj.AddComponent<XRDirectInteractor>();

            GameObject targetObj = CreateCleanObject("MoveTarget", testRoot.transform);
            var interactable = targetObj.AddComponent<StatefulInteractable>();

            yield return null;

            var singleList = new List<IXRSelectInteractor> { i1 };
            var dualList = new List<IXRSelectInteractor> { i1, i2 };
            var currentTarget = new MixedRealityTransform(new Vector3(0, 0, 1), Quaternion.identity, Vector3.one);

            // Single interactor test
            var moveLogicSingle = new MoveLogic();
            moveLogicSingle.Setup(singleList, interactable, currentTarget);
            Vector3 singleUpdated = moveLogicSingle.Update(singleList, interactable, currentTarget, true);
            Assert.AreEqual(currentTarget.Position, singleUpdated, "Single interactor centeredAnchor should preserve initial target position at setup.");

            // Dual interactor test
            var moveLogicDual = new MoveLogic();
            moveLogicDual.Setup(dualList, interactable, currentTarget);
            Vector3 dualUpdated = moveLogicDual.Update(dualList, interactable, currentTarget, true);
            Assert.AreEqual(currentTarget.Position, dualUpdated, "Dual interactor centeredAnchor should preserve initial target position at setup.");

            // Warm up & Benchmark Single
            for (int i = 0; i < 50; i++)
            {
                _ = moveLogicSingle.Update(singleList, interactable, currentTarget, false);
            }
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                _ = moveLogicSingle.Update(singleList, interactable, currentTarget, false);
            }
            sw.Stop();
            Debug.Log($"[PERF_BENCHMARK] MoveLogic.Update (Single-Handed) x{HighFrequencyIterations}: {sw.Elapsed.TotalMilliseconds:F3} ms ({(sw.Elapsed.TotalMilliseconds / HighFrequencyIterations) * 1000.0:F2} µs/call)");

            // Warm up & Benchmark Dual
            for (int i = 0; i < 50; i++)
            {
                _ = moveLogicDual.Update(dualList, interactable, currentTarget, false);
            }
            sw.Restart();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                _ = moveLogicDual.Update(dualList, interactable, currentTarget, false);
            }
            sw.Stop();
            Debug.Log($"[PERF_BENCHMARK] MoveLogic.Update (Dual-Handed) x{HighFrequencyIterations}: {sw.Elapsed.TotalMilliseconds:F3} ms ({(sw.Elapsed.TotalMilliseconds / HighFrequencyIterations) * 1000.0:F2} µs/call)");

            // Assert Zero GC
            Assert.That(() =>
            {
                _ = moveLogicSingle.Update(singleList, interactable, currentTarget, false);
            }, Is.Not.AllocatingGCMemory(), "MoveLogic.Update must not allocate GC memory for single interactor.");

            Assert.That(() =>
            {
                _ = moveLogicDual.Update(dualList, interactable, currentTarget, false);
            }, Is.Not.AllocatingGCMemory(), "MoveLogic.Update must not allocate GC memory for dual interactors.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ScaleLogic_SingleAndDualInteractor_AccuracyAndZeroGC()
        {
            GameObject i1Obj = CreateCleanObject("ScaleInteractor1", testRoot.transform);
            i1Obj.transform.position = new Vector3(-0.2f, 0, 1);
            var i1 = i1Obj.AddComponent<XRDirectInteractor>();

            GameObject i2Obj = CreateCleanObject("ScaleInteractor2", testRoot.transform);
            i2Obj.transform.position = new Vector3(0.2f, 0, 1);
            var i2 = i2Obj.AddComponent<XRDirectInteractor>();

            GameObject targetObj = CreateCleanObject("ScaleTarget", testRoot.transform);
            var interactable = targetObj.AddComponent<StatefulInteractable>();

            yield return null;

            var singleList = new List<IXRSelectInteractor> { i1 };
            var dualList = new List<IXRSelectInteractor> { i1, i2 };
            var currentTarget = new MixedRealityTransform(new Vector3(0, 0, 1), Quaternion.identity, Vector3.one);

            // Single interactor: should preserve 1.0f scale
            var scaleLogicSingle = new ScaleLogic();
            scaleLogicSingle.Setup(singleList, interactable, currentTarget);
            Vector3 singleUpdated = scaleLogicSingle.Update(singleList, interactable, currentTarget, false);
            Assert.AreEqual(Vector3.one, singleUpdated, "Single interactor should retain original scale without attach scale change.");

            // Dual interactor: double distance -> double scale
            var scaleLogicDual = new ScaleLogic();
            scaleLogicDual.Setup(dualList, interactable, currentTarget);
            i1Obj.transform.position = new Vector3(-0.4f, 0, 1);
            i2Obj.transform.position = new Vector3(0.4f, 0, 1);
            Vector3 dualUpdated = scaleLogicDual.Update(dualList, interactable, currentTarget, false);
            Assert.AreEqual(2.0f, dualUpdated.x, 0.01f, "Doubled interactor distance should result in doubled scale.");

            // Warm up & Benchmark Single
            for (int i = 0; i < 50; i++)
            {
                _ = scaleLogicSingle.Update(singleList, interactable, currentTarget, false);
            }
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                _ = scaleLogicSingle.Update(singleList, interactable, currentTarget, false);
            }
            sw.Stop();
            Debug.Log($"[PERF_BENCHMARK] ScaleLogic.Update (Single-Handed) x{HighFrequencyIterations}: {sw.Elapsed.TotalMilliseconds:F3} ms ({(sw.Elapsed.TotalMilliseconds / HighFrequencyIterations) * 1000.0:F2} µs/call)");

            // Warm up & Benchmark Dual
            for (int i = 0; i < 50; i++)
            {
                _ = scaleLogicDual.Update(dualList, interactable, currentTarget, false);
            }
            sw.Restart();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                _ = scaleLogicDual.Update(dualList, interactable, currentTarget, false);
            }
            sw.Stop();
            Debug.Log($"[PERF_BENCHMARK] ScaleLogic.Update (Dual-Handed) x{HighFrequencyIterations}: {sw.Elapsed.TotalMilliseconds:F3} ms ({(sw.Elapsed.TotalMilliseconds / HighFrequencyIterations) * 1000.0:F2} µs/call)");

            // Assert Zero GC
            Assert.That(() =>
            {
                _ = scaleLogicSingle.Update(singleList, interactable, currentTarget, false);
            }, Is.Not.AllocatingGCMemory(), "ScaleLogic.Update must not allocate GC memory for single interactor.");

            Assert.That(() =>
            {
                _ = scaleLogicDual.Update(dualList, interactable, currentTarget, false);
            }, Is.Not.AllocatingGCMemory(), "ScaleLogic.Update must not allocate GC memory for dual interactors.");

            yield return null;
        }

        #endregion

        #region Microbenchmarks

        [UnityTest]
        public IEnumerator DirectMicrobenchmark_ComprehensiveComparison()
        {
            Camera cam = Camera.main;
            Assert.IsNotNull(cam);

            // 1. ScaleHandle LateUpdate microbenchmark
            GameObject handleObj = CreateCleanObject("BenchmarkScaleHandle", testRoot.transform);
            handleObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
            var handle = handleObj.AddComponent<ScaleHandleInteractable>();
            handle.IsOccluded = false;
            handle.IsFlattened = false;

            yield return null;

            MethodInfo lateUpdateMethod = typeof(ScaleHandleInteractable).GetMethod("LateUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Action lateUpdateAction = (Action)Delegate.CreateDelegate(typeof(Action), handle, lateUpdateMethod);

            // 2. Constraints microbenchmarks
            GameObject faceObj = CreateCleanObject("BenchmarkFaceUser", testRoot.transform);
            var faceUser = faceObj.AddComponent<FaceUserConstraint>();

            GameObject distObj = CreateCleanObject("BenchmarkFixedDist", testRoot.transform);
            var fixedDist = distObj.AddComponent<FixedDistanceConstraint>();
            fixedDist.ConstraintTransform = cam.transform;

            GameObject rotObj = CreateCleanObject("BenchmarkFixedRot", testRoot.transform);
            var fixedRot = rotObj.AddComponent<FixedRotationToUserConstraint>();

            GameObject sizeObj = CreateCleanObject("BenchmarkMaintainSize", testRoot.transform);
            var maintainSize = sizeObj.AddComponent<MaintainApparentSizeConstraint>();

            var pose = new MixedRealityTransform(cam.transform.position + Vector3.forward * 2f, Quaternion.identity, Vector3.one);
            fixedDist.OnManipulationStarted(pose);
            fixedRot.OnManipulationStarted(pose);
            maintainSize.OnManipulationStarted(pose);

            // 3. Move, Rotate, Scale Logics microbenchmarks
            GameObject i1Obj = CreateCleanObject("BenchI1", testRoot.transform);
            i1Obj.transform.position = new Vector3(-0.2f, 0, 1);
            var i1 = i1Obj.AddComponent<XRDirectInteractor>();

            GameObject i2Obj = CreateCleanObject("BenchI2", testRoot.transform);
            i2Obj.transform.position = new Vector3(0.2f, 0, 1);
            var i2 = i2Obj.AddComponent<XRDirectInteractor>();

            GameObject targetObj = CreateCleanObject("BenchTarget", testRoot.transform);
            var interactable = targetObj.AddComponent<StatefulInteractable>();

            yield return null;

            var singleList = new List<IXRSelectInteractor> { i1 };
            var dualList = new List<IXRSelectInteractor> { i1, i2 };
            var currentTarget = new MixedRealityTransform(new Vector3(0, 0, 1), Quaternion.identity, Vector3.one);

            var moveLogicSingle = new MoveLogic();
            moveLogicSingle.Setup(singleList, interactable, currentTarget);

            var moveLogicDual = new MoveLogic();
            moveLogicDual.Setup(dualList, interactable, currentTarget);

            var rotateLogicDual = new RotateLogic();
            rotateLogicDual.Setup(dualList, interactable, currentTarget);

            var scaleLogicDual = new ScaleLogic();
            scaleLogicDual.Setup(dualList, interactable, currentTarget);

            // Warm up all
            for (int i = 0; i < 100; i++)
            {
                lateUpdateAction();
                var p = pose;
                faceUser.ApplyConstraint(ref p);
                fixedDist.ApplyConstraint(ref p);
                fixedRot.ApplyConstraint(ref p);
                maintainSize.ApplyConstraint(ref p);
                _ = moveLogicSingle.Update(singleList, interactable, currentTarget, false);
                _ = moveLogicDual.Update(dualList, interactable, currentTarget, false);
                _ = rotateLogicDual.Update(dualList, interactable, currentTarget, false);
                _ = scaleLogicDual.Update(dualList, interactable, currentTarget, false);
            }

            // Benchmark ScaleHandleInteractable.LateUpdate
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < MicrobenchmarkIterations; i++)
            {
                lateUpdateAction();
            }
            sw.Stop();
            double scaleHandleMs = sw.Elapsed.TotalMilliseconds;

            // Benchmark FaceUserConstraint.ApplyConstraint
            sw.Restart();
            for (int i = 0; i < MicrobenchmarkIterations; i++)
            {
                var p = pose;
                faceUser.ApplyConstraint(ref p);
            }
            sw.Stop();
            double faceUserMs = sw.Elapsed.TotalMilliseconds;

            // Benchmark FixedDistanceConstraint.ApplyConstraint
            sw.Restart();
            for (int i = 0; i < MicrobenchmarkIterations; i++)
            {
                var p = pose;
                fixedDist.ApplyConstraint(ref p);
            }
            sw.Stop();
            double fixedDistMs = sw.Elapsed.TotalMilliseconds;

            // Benchmark MoveLogic (Single-Handed)
            sw.Restart();
            for (int i = 0; i < MicrobenchmarkIterations; i++)
            {
                _ = moveLogicSingle.Update(singleList, interactable, currentTarget, false);
            }
            sw.Stop();
            double moveSingleMs = sw.Elapsed.TotalMilliseconds;

            // Benchmark MoveLogic (Dual-Handed)
            sw.Restart();
            for (int i = 0; i < MicrobenchmarkIterations; i++)
            {
                _ = moveLogicDual.Update(dualList, interactable, currentTarget, false);
            }
            sw.Stop();
            double moveDualMs = sw.Elapsed.TotalMilliseconds;

            // Benchmark RotateLogic (Dual-Handed)
            sw.Restart();
            for (int i = 0; i < MicrobenchmarkIterations; i++)
            {
                _ = rotateLogicDual.Update(dualList, interactable, currentTarget, false);
            }
            sw.Stop();
            double rotateDualMs = sw.Elapsed.TotalMilliseconds;

            // Benchmark ScaleLogic (Dual-Handed)
            sw.Restart();
            for (int i = 0; i < MicrobenchmarkIterations; i++)
            {
                _ = scaleLogicDual.Update(dualList, interactable, currentTarget, false);
            }
            sw.Stop();
            double scaleDualMs = sw.Elapsed.TotalMilliseconds;

            Debug.Log($"[MICROBENCHMARK_SUMMARY] Iterations={MicrobenchmarkIterations} | " +
                      $"ScaleHandle={scaleHandleMs:F2}ms ({(scaleHandleMs / MicrobenchmarkIterations) * 1000.0:F2}µs/call) | " +
                      $"FaceUser={faceUserMs:F2}ms ({(faceUserMs / MicrobenchmarkIterations) * 1000.0:F2}µs/call) | " +
                      $"FixedDist={fixedDistMs:F2}ms ({(fixedDistMs / MicrobenchmarkIterations) * 1000.0:F2}µs/call) | " +
                      $"MoveSingle={moveSingleMs:F2}ms ({(moveSingleMs / MicrobenchmarkIterations) * 1000.0:F2}µs/call) | " +
                      $"MoveDual={moveDualMs:F2}ms ({(moveDualMs / MicrobenchmarkIterations) * 1000.0:F2}µs/call) | " +
                      $"RotateDual={rotateDualMs:F2}ms ({(rotateDualMs / MicrobenchmarkIterations) * 1000.0:F2}µs/call) | " +
                      $"ScaleDual={scaleDualMs:F2}ms ({(scaleDualMs / MicrobenchmarkIterations) * 1000.0:F2}µs/call)");

            yield return null;
        }

        #endregion
    }
}

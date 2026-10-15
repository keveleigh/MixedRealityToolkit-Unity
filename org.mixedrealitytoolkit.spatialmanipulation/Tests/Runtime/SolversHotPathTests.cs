// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace MixedReality.Toolkit.SpatialManipulation.Runtime.Tests
{
    /// <summary>
    /// Comprehensive test suite verifying accuracy, performance, lifecycle, null camera safety,
    /// and Zero-GC behavior of MRTK Solvers (SolverHandler, Follow, RadialView, Orbital, InBetween, and Solver).
    /// </summary>
    public class SolversHotPathTests : BaseRuntimeInputTests
    {
        protected override InputTestUtilities.RigVersion RigVersion => InputTestUtilities.RigVersion.Version1;

        private const int HighFrequencyIterations = 1000;

        private GameObject solverHost;
        private SolverHandler solverHandler;

        public override IEnumerator Setup()
        {
            yield return base.Setup();

            solverHost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            solverHost.name = "SolverHost";
            solverHost.transform.position = InputTestUtilities.InFrontOfUser(1.0f);
            solverHost.transform.localScale = Vector3.one * 0.2f;

            solverHandler = solverHost.AddComponent<SolverHandler>();
            solverHandler.TrackedTargetType = TrackedObjectType.Head;

            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        public override IEnumerator TearDown()
        {
            if (solverHost != null)
            {
                Object.Destroy(solverHost);
            }

            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator SolverHandler_LifecycleAndTracking()
        {
            Assert.IsNotNull(solverHandler);
            Assert.IsNotNull(solverHandler.TransformTarget, "SolverHandler should resolve a valid head target.");

            // Switch tracked target type and verify refresh
            solverHandler.TrackedTargetType = TrackedObjectType.CustomOverride;
            GameObject customTarget = new GameObject("CustomTarget");
            customTarget.transform.position = Vector3.up * 2f;
            solverHandler.TransformOverride = customTarget.transform;

            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(customTarget.transform.position, solverHandler.TransformTarget.position);

            Object.Destroy(customTarget);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Solver_SmoothTo_ZeroGCAndPerf()
        {
            Vector3 v1 = Vector3.zero;
            Vector3 v2 = Vector3.one * 10f;
            Quaternion q1 = Quaternion.identity;
            Quaternion q2 = Quaternion.Euler(45f, 90f, 180f);

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                _ = Solver.SmoothTo(v1, v2, 0.016f, 0.1f);
                _ = Solver.SmoothTo(q1, q2, 0.016f, 0.1f);
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                _ = Solver.SmoothTo(v1, v2, 0.016f, 0.1f);
                _ = Solver.SmoothTo(q1, q2, 0.016f, 0.1f);
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] Solver.SmoothTo: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    _ = Solver.SmoothTo(v1, v2, 0.016f, 0.1f);
                    _ = Solver.SmoothTo(q1, q2, 0.016f, 0.1f);
                }
            }, Is.Not.AllocatingGCMemory());

            yield return null;
        }

        [UnityTest]
        public IEnumerator Follow_SolverUpdate_ZeroGCAndPerf()
        {
            var follow = solverHost.AddComponent<Follow>();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                follow.SolverUpdateEntry();
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                follow.SolverUpdateEntry();
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] Follow.SolverUpdateEntry: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    follow.SolverUpdateEntry();
                }
            }, Is.Not.AllocatingGCMemory());

            Object.Destroy(follow);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RadialView_SolverUpdate_ZeroGCAndPerf()
        {
            var radialView = solverHost.AddComponent<RadialView>();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                radialView.SolverUpdateEntry();
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                radialView.SolverUpdateEntry();
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] RadialView.SolverUpdateEntry: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    radialView.SolverUpdateEntry();
                }
            }, Is.Not.AllocatingGCMemory());

            Object.Destroy(radialView);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Orbital_SolverUpdate_ZeroGCAndPerf()
        {
            var orbital = solverHost.AddComponent<Orbital>();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                orbital.SolverUpdateEntry();
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                orbital.SolverUpdateEntry();
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] Orbital.SolverUpdateEntry: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    orbital.SolverUpdateEntry();
                }
            }, Is.Not.AllocatingGCMemory());

            Object.Destroy(orbital);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InBetween_SolverUpdate_ZeroGCAndPerf()
        {
            var inBetween = solverHost.AddComponent<InBetween>();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                inBetween.SolverUpdateEntry();
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                inBetween.SolverUpdateEntry();
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] InBetween.SolverUpdateEntry: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    inBetween.SolverUpdateEntry();
                }
            }, Is.Not.AllocatingGCMemory());

            Object.Destroy(inBetween);
            yield return null;
        }

        #region Behavioral / Regression Tests

        [UnityTest]
        public IEnumerator Follow_DistanceClamping_Behavior()
        {
            var follow = solverHost.AddComponent<Follow>();
            follow.MinDistance = 1.0f;
            follow.MaxDistance = 2.0f;
            follow.DefaultDistance = 1.5f;
            yield return RuntimeTestUtilities.WaitForUpdates();

            // First update centers the object at DefaultDistance in front of target
            follow.SolverUpdateEntry();
            Assert.AreEqual(1.5f, Vector3.Distance(solverHost.transform.position, solverHandler.TransformTarget.position), 0.05f);

            // Move solver far beyond MaxDistance (e.g. 5.0m away)
            solverHost.transform.position = solverHandler.TransformTarget.position + solverHandler.TransformTarget.forward * 5.0f;
            follow.SolverUpdateEntry();
            float distFar = Vector3.Distance(solverHandler.GoalPosition, solverHandler.TransformTarget.position);
            Assert.AreEqual(2.0f, distFar, 0.05f, "GoalPosition should clamp to MaxDistance (2.0m)");

            // Move solver closer than MinDistance (e.g. 0.2m away)
            solverHost.transform.position = solverHandler.TransformTarget.position + solverHandler.TransformTarget.forward * 0.2f;
            follow.SolverUpdateEntry();
            float distNear = Vector3.Distance(solverHandler.GoalPosition, solverHandler.TransformTarget.position);
            Assert.AreEqual(1.0f, distNear, 0.05f, "GoalPosition should clamp to MinDistance (1.0m)");

            Object.Destroy(follow);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RadialView_ViewConeClamping_Behavior()
        {
            var radialView = solverHost.AddComponent<RadialView>();
            radialView.MinDistance = 1.0f;
            radialView.MaxDistance = 2.0f;
            radialView.MinViewDegrees = 0f;
            radialView.MaxViewDegrees = 20f;
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Place solver directly ahead within cone
            solverHost.transform.position = solverHandler.TransformTarget.position + solverHandler.TransformTarget.forward * 1.5f;
            radialView.SolverUpdateEntry();
            Vector3 toGoal = (solverHandler.GoalPosition - solverHandler.TransformTarget.position).normalized;
            float angleCenter = Vector3.Angle(solverHandler.TransformTarget.forward, toGoal);
            Assert.LessOrEqual(angleCenter, 20.1f, "Directly ahead target should remain within view cone.");

            // Place solver at 60 degrees off to the right (outside the 20-degree cone)
            Vector3 offsetDir = Quaternion.Euler(0, 60f, 0) * solverHandler.TransformTarget.forward;
            solverHost.transform.position = solverHandler.TransformTarget.position + offsetDir * 1.5f;
            radialView.SolverUpdateEntry();
            toGoal = (solverHandler.GoalPosition - solverHandler.TransformTarget.position).normalized;
            float angleClamped = Vector3.Angle(solverHandler.TransformTarget.forward, toGoal);
            Assert.AreEqual(20f, angleClamped, 0.5f, "GoalPosition direction should be clamped to MaxViewDegrees (20 deg)");

            Object.Destroy(radialView);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Orbital_LocalOffsetAndStepping_Behavior()
        {
            var orbital = solverHost.AddComponent<Orbital>();
            orbital.LocalOffset = new Vector3(0.5f, 0f, 1.0f);
            orbital.WorldOffset = Vector3.zero;
            orbital.UseAngleStepping = false;
            yield return RuntimeTestUtilities.WaitForUpdates();

            orbital.SolverUpdateEntry();
            Vector3 targetPos = solverHandler.TransformTarget.position;
            Quaternion targetRot = solverHandler.TransformTarget.rotation;
            Vector3 expectedPos = targetPos + (targetRot * new Vector3(0.5f, 0f, 1.0f));
            Assert.AreEqual(expectedPos.x, solverHandler.GoalPosition.x, 0.01f);
            Assert.AreEqual(expectedPos.y, solverHandler.GoalPosition.y, 0.01f);
            Assert.AreEqual(expectedPos.z, solverHandler.GoalPosition.z, 0.01f);

            // Test angle stepping
            orbital.UseAngleStepping = true;
            orbital.TetherAngleSteps = 4; // 90 degree increments
            orbital.SolverUpdateEntry();
            Assert.IsFalse(float.IsNaN(solverHandler.GoalPosition.x));
            Assert.IsFalse(float.IsNaN(solverHandler.GoalRotation.x));

            Object.Destroy(orbital);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InBetween_Interpolation_Behavior()
        {
            var inBetween = solverHost.AddComponent<InBetween>();
            GameObject target1 = new GameObject("Target1");
            GameObject target2 = new GameObject("Target2");
            target1.transform.position = new Vector3(0, 0, 0);
            target2.transform.position = new Vector3(0, 0, 10);

            solverHandler.TrackedTargetType = TrackedObjectType.CustomOverride;
            solverHandler.TransformOverride = target1.transform;

            inBetween.SecondTrackedObjectType = TrackedObjectType.CustomOverride;
            inBetween.SecondTransformOverride = target2.transform;
            inBetween.PartwayOffset = 0.5f;

            yield return RuntimeTestUtilities.WaitForUpdates();
            inBetween.SolverUpdateEntry();

            // Centerline is target1 (0,0,0) - target2 (0,0,10) = (0,0,-10)
            // GoalPosition = secondPos + centerline * partwayOffset = (0,0,10) + (0,0,-10)*0.5 = (0,0,5)
            Assert.AreEqual(5.0f, solverHandler.GoalPosition.z, 0.01f, "At 0.5 offset, GoalPosition should be midpoint (5.0)");

            inBetween.PartwayOffset = 0.2f;
            inBetween.SolverUpdateEntry();
            // (0,0,10) + (0,0,-10)*0.2 = (0,0,8)
            Assert.AreEqual(8.0f, solverHandler.GoalPosition.z, 0.01f, "At 0.2 offset from second, GoalPosition should be at 8.0");

            inBetween.PartwayOffset = 0.8f;
            inBetween.SolverUpdateEntry();
            // (0,0,10) + (0,0,-10)*0.8 = (0,0,2)
            Assert.AreEqual(2.0f, solverHandler.GoalPosition.z, 0.01f, "At 0.8 offset from second, GoalPosition should be at 2.0");

            Object.Destroy(target1);
            Object.Destroy(target2);
            Object.Destroy(inBetween);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Solvers_NullCamera_DoesNotThrow()
        {
            GameObject testObj = new GameObject("NullCameraTestObj");
            var handler = testObj.AddComponent<SolverHandler>();
            var follow = testObj.AddComponent<Follow>();
            var orbital = testObj.AddComponent<Orbital>();

            follow.OrientationType = SolverOrientationType.CameraAligned;
            orbital.OrientationType = SolverOrientationType.CameraAligned;

            string prevTag = null;
            Camera cam = Camera.main;
            try
            {
                if (cam != null)
                {
                    prevTag = cam.tag;
                    cam.tag = "Untagged";
                }

                Assert.IsNull(Camera.main, "Camera.main should be simulated as null for this test.");

                Assert.DoesNotThrow(() =>
                {
                    handler.TrackedTargetType = TrackedObjectType.Head;
                    follow.SolverUpdateEntry();
                    orbital.SolverUpdateEntry();
                }, "Solvers should safely handle null Camera.main without throwing NullReferenceException.");
            }
            finally
            {
                if (cam != null && prevTag != null)
                {
                    cam.tag = prevTag;
                }
                Object.Destroy(testObj);
            }

            yield return null;
        }

        #endregion
    }
}

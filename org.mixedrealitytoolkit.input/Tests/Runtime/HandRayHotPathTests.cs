// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Subsystems;
using NUnit.Framework;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace MixedReality.Toolkit.Input.Tests
{
    using AggregatorDescriptor = MRTKSubsystemDescriptor<HandsAggregatorSubsystem, HandsAggregatorSubsystem.Provider>;

    /// <summary>
    /// Comprehensive test suite verifying functional accuracy, tolerances, camera null-safety,
    /// performance, and Zero-GC invariants of <see cref="HandRay"/>, <see cref="LOSAngularOffsetHandRayPoseSource"/>,
    /// and <see cref="PolyfillHandRayPoseSource"/>.
    /// </summary>
    public class HandRayHotPathTests : BaseRuntimeInputTests
    {
        protected override InputTestUtilities.RigVersion RigVersion => InputTestUtilities.RigVersion.Version1;

        private const int HighFrequencyIterations = 1000;
        private MRTKHandsAggregatorSubsystem aggregator;

        public override IEnumerator Setup()
        {
            yield return base.Setup();

            aggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<MRTKHandsAggregatorSubsystem>();
            if (aggregator == null)
            {
                aggregator = SubsystemTestUtilities.CreateAndEnsureExists<MRTKHandsAggregatorSubsystem, AggregatorDescriptor>();
                aggregator.Start();
            }

            Assert.IsNotNull(aggregator, "MRTKHandsAggregatorSubsystem must exist and be accessible.");
            Assert.IsTrue(aggregator.running, "MRTKHandsAggregatorSubsystem must be running.");
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        #region HandRay Pure Unit & Behavioral Tests

        [UnityTest]
        public IEnumerator HandRay_PivotCalculation_RightAndLeftHand_BehavioralConsistency()
        {
            GameObject headGo = new GameObject("TestHead");
            try
            {
                headGo.transform.position = new Vector3(0f, 1.6f, 0f);
                headGo.transform.rotation = Quaternion.identity;

                HandRay rightHandRay = new HandRay();
                Vector3 rightHandPos = new Vector3(0.2f, 1.4f, 0.4f);
                rightHandRay.Update(rightHandPos, Vector3.forward, headGo.transform, Handedness.Right);

                Ray rightRay = rightHandRay.Ray;
                Assert.AreEqual(rightHandPos.x, rightRay.origin.x, 0.001f);
                Assert.AreEqual(rightHandPos.y, rightRay.origin.y, 0.001f);
                Assert.AreEqual(rightHandPos.z, rightRay.origin.z, 0.001f);
                Assert.AreEqual(1.0f, rightRay.direction.magnitude, 0.001f);

                // For right hand at (0.2, 1.4, 0.4):
                // handHeadSpace = (0.2, -0.2, 0.4)
                // relativePivotY = DynamicPivotBaseY (-0.1) + min(0.65 * -0.2, 0) = -0.23, clamped to [-0.6, -0.2] => -0.23
                // relativePivotX = DynamicPivotBaseX (0.03) + 0.65 * 0.2 = 0.16, clamped to [0.08, 0.15] => 0.15
                // relativePivotZ = 0.08
                // pivot = (0, 1.6, 0) + (0.15, -0.23, 0.08) = (0.15, 1.37, 0.08)
                // measuredDirection = rightHandPos - pivot = (0.2 - 0.15, 1.4 - 1.37, 0.4 - 0.08) = (0.05, 0.03, 0.32)
                Vector3 expectedRightPivot = new Vector3(0.15f, 1.37f, 0.08f);
                Vector3 expectedRightDir = (rightHandPos - expectedRightPivot).normalized;
                Assert.AreEqual(expectedRightDir.x, rightRay.direction.x, 0.01f);
                Assert.AreEqual(expectedRightDir.y, rightRay.direction.y, 0.01f);
                Assert.AreEqual(expectedRightDir.z, rightRay.direction.z, 0.01f);

                // Symmetrical test for Left Hand
                HandRay leftHandRay = new HandRay();
                Vector3 leftHandPos = new Vector3(-0.2f, 1.4f, 0.4f);
                leftHandRay.Update(leftHandPos, Vector3.forward, headGo.transform, Handedness.Left);

                Ray leftRay = leftHandRay.Ray;
                // For left hand at (-0.2, 1.4, 0.4):
                // handHeadSpace = (-0.2, -0.2, 0.4)
                // relativePivotX = -0.03 + 0.65 * (-0.2) = -0.16, clamped to [-0.15, -0.08] => -0.15
                // pivot = (0, 1.6, 0) + (-0.15, -0.23, 0.08) = (-0.15, 1.37, 0.08)
                Vector3 expectedLeftPivot = new Vector3(-0.15f, 1.37f, 0.08f);
                Vector3 expectedLeftDir = (leftHandPos - expectedLeftPivot).normalized;
                Assert.AreEqual(expectedLeftDir.x, leftRay.direction.x, 0.01f);
                Assert.AreEqual(expectedLeftDir.y, leftRay.direction.y, 0.01f);
                Assert.AreEqual(expectedLeftDir.z, leftRay.direction.z, 0.01f);

                // Test with rotated head:
                headGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                rightHandRay.Update(headGo.transform.TransformPoint(new Vector3(0.2f, -0.2f, 0.4f)), headGo.transform.forward, headGo.transform, Handedness.Right);
                Ray rotatedRay = rightHandRay.Ray;
                Assert.AreEqual(1.0f, rotatedRay.direction.magnitude, 0.001f);
            }
            finally
            {
                Object.Destroy(headGo);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HandRay_ShouldShowRay_Tolerances()
        {
            GameObject headGo = new GameObject("TestHead");
            try
            {
                headGo.transform.position = Vector3.up * 1.6f;
                headGo.transform.rotation = Quaternion.identity;

                HandRay handRay = new HandRay();

                // Before any update, headForward is Vector3.zero -> ShouldShowRay must be false
                Assert.IsFalse(handRay.ShouldShowRay, "Uninitialized HandRay must not show ray.");

                Vector3 handPos = new Vector3(0.2f, 1.4f, 0.4f);

                // Palm facing forward (away from camera)
                handRay.Update(handPos, Vector3.forward, headGo.transform, Handedness.Right);
                Assert.IsTrue(handRay.ShouldShowRay, "Palm facing away from camera must show ray.");

                // Palm facing backward towards camera (-headForward)
                handRay.Update(handPos, -headGo.transform.forward, headGo.transform, Handedness.Right);
                Assert.IsFalse(handRay.ShouldShowRay, "Palm facing camera must hide ray.");

                // Palm facing straight up (exceeds CursorBeamUpTolerance 0.8)
                handRay.Update(handPos, Vector3.up, headGo.transform, Handedness.Right);
                Assert.IsFalse(handRay.ShouldShowRay, "Palm facing straight up must hide ray.");

                // Palm angled forward-up (dot with up ~0.447 <= 0.8)
                Vector3 angledPalm = (Vector3.forward + Vector3.up * 0.5f).normalized;
                handRay.Update(handPos, angledPalm, headGo.transform, Handedness.Right);
                Assert.IsTrue(handRay.ShouldShowRay, "Comfortably angled palm must show ray.");
            }
            finally
            {
                Object.Destroy(headGo);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HandRay_NullHeadTransform_DoesNotThrow()
        {
            HandRay handRay = new HandRay();
            Assert.DoesNotThrow(() =>
            {
                handRay.Update(Vector3.zero, Vector3.forward, null, Handedness.Right);
            }, "HandRay.Update must safely handle null headTransform without throwing NullReferenceException.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator HandRay_UpdateAndQuery_ZeroGCAndPerf()
        {
            GameObject headGo = new GameObject("TestHead");
            try
            {
                headGo.transform.position = Vector3.up * 1.6f;
                headGo.transform.rotation = Quaternion.identity;

                HandRay handRay = new HandRay();
                Vector3 handPos = new Vector3(0.2f, 1.4f, 0.4f);
                Vector3 palmNormal = Vector3.forward;

                // Warmup
                for (int i = 0; i < 100; i++)
                {
                    handRay.Update(handPos, palmNormal, headGo.transform, Handedness.Right);
                    _ = handRay.Ray;
                    _ = handRay.ShouldShowRay;
                }

                // Zero-GC Assertion
                Assert.That(() =>
                {
                    for (int i = 0; i < HighFrequencyIterations; i++)
                    {
                        handRay.Update(handPos, palmNormal, headGo.transform, Handedness.Right);
                        _ = handRay.Ray;
                        _ = handRay.ShouldShowRay;
                    }
                }, Is.Not.AllocatingGCMemory());

                // Microbenchmark timing
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    handRay.Update(handPos, palmNormal, headGo.transform, Handedness.Right);
                    _ = handRay.Ray;
                    _ = handRay.ShouldShowRay;
                }
                sw.Stop();

                Debug.Log($"[PERF] HandRay.Update + Queries ({HighFrequencyIterations} iters): {sw.Elapsed.TotalMilliseconds:F3} ms (avg: {sw.Elapsed.TotalMilliseconds / HighFrequencyIterations * 1000.0:F3} us/op)");
            }
            finally
            {
                Object.Destroy(headGo);
            }

            yield return null;
        }

        #endregion

        #region PoseSources Behavioral & Null Camera Safety Tests

        [UnityTest]
        public IEnumerator LOSAngularOffsetHandRayPoseSource_TryGetPose_TracksKnuckle()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            LOSAngularOffsetHandRayPoseSource losSource = new LOSAngularOffsetHandRayPoseSource
            {
                Hand = Handedness.Right
            };

            bool success = losSource.TryGetPose(out Pose pose);
            Assert.IsTrue(success, "LOSAngularOffsetHandRayPoseSource should successfully retrieve pose when hand is tracked.");
            Assert.AreNotEqual(Vector3.zero, pose.position, "Retrieved pose position should not be zero.");
            Assert.AreNotEqual(Quaternion.identity, pose.rotation, "Retrieved pose rotation should not be identity.");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator PolyfillHandRayPoseSource_TryGetPose_TracksKnuckle()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            PolyfillHandRayPoseSource polyfillSource = new PolyfillHandRayPoseSource
            {
                Hand = Handedness.Right
            };

            bool success = polyfillSource.TryGetPose(out Pose pose);
            Assert.IsTrue(success, "PolyfillHandRayPoseSource should successfully retrieve pose when hand is tracked.");
            Assert.AreNotEqual(Vector3.zero, pose.position, "Retrieved pose position should not be zero.");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator LOSAngularOffsetHandRayPoseSource_NullCamera_DoesNotThrow()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            LOSAngularOffsetHandRayPoseSource losSource = new LOSAngularOffsetHandRayPoseSource
            {
                Hand = Handedness.Right
            };

            Camera mainCam = Camera.main;
            string originalTag = mainCam != null ? mainCam.tag : null;
            if (mainCam != null)
            {
                mainCam.tag = "Untagged";
            }

            try
            {
                Assert.IsNull(Camera.main, "Camera.main must be null for this test.");

                bool success = false;
                Pose pose = Pose.identity;

                Assert.DoesNotThrow(() =>
                {
                    success = losSource.TryGetPose(out pose);
                }, "LOSAngularOffsetHandRayPoseSource must handle null Camera.main safely without throwing NullReferenceException.");

                Assert.IsFalse(success, "TryGetPose must return false when Camera.main is null.");
                Assert.AreEqual(Pose.identity, pose, "Pose must be Pose.identity when Camera.main is null.");
            }
            finally
            {
                if (mainCam != null && originalTag != null)
                {
                    mainCam.tag = originalTag;
                }
            }

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator PolyfillHandRayPoseSource_NullCamera_DoesNotThrow()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            PolyfillHandRayPoseSource polyfillSource = new PolyfillHandRayPoseSource
            {
                Hand = Handedness.Right
            };

            Camera mainCam = Camera.main;
            string originalTag = mainCam != null ? mainCam.tag : null;
            if (mainCam != null)
            {
                mainCam.tag = "Untagged";
            }

            try
            {
                Assert.IsNull(Camera.main, "Camera.main must be null for this test.");

                bool success = false;
                Pose pose = Pose.identity;

                Assert.DoesNotThrow(() =>
                {
                    success = polyfillSource.TryGetPose(out pose);
                }, "PolyfillHandRayPoseSource must handle null Camera.main safely without throwing NullReferenceException.");

                Assert.IsFalse(success, "TryGetPose must return false when Camera.main is null.");
                Assert.AreEqual(Pose.identity, pose, "Pose must be Pose.identity when Camera.main is null.");
            }
            finally
            {
                if (mainCam != null && originalTag != null)
                {
                    mainCam.tag = originalTag;
                }
            }

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator LOSAngularOffsetHandRayPoseSource_ZeroGCAndPerf()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            LOSAngularOffsetHandRayPoseSource losSource = new LOSAngularOffsetHandRayPoseSource
            {
                Hand = Handedness.Right
            };

            // Warmup
            for (int i = 0; i < 100; i++)
            {
                losSource.TryGetPose(out _);
            }

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    losSource.TryGetPose(out _);
                }
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                losSource.TryGetPose(out _);
            }
            sw.Stop();

            Debug.Log($"[PERF] LOSAngularOffsetHandRayPoseSource.TryGetPose ({HighFrequencyIterations} iters): {sw.Elapsed.TotalMilliseconds:F3} ms (avg: {sw.Elapsed.TotalMilliseconds / HighFrequencyIterations * 1000.0:F3} us/op)");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator PolyfillHandRayPoseSource_ZeroGCAndPerf()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            PolyfillHandRayPoseSource polyfillSource = new PolyfillHandRayPoseSource
            {
                Hand = Handedness.Right
            };

            // Warmup
            for (int i = 0; i < 100; i++)
            {
                polyfillSource.TryGetPose(out _);
            }

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    polyfillSource.TryGetPose(out _);
                }
            }, Is.Not.AllocatingGCMemory());

            // Microbenchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                polyfillSource.TryGetPose(out _);
            }
            sw.Stop();

            Debug.Log($"[PERF] PolyfillHandRayPoseSource.TryGetPose ({HighFrequencyIterations} iters): {sw.Elapsed.TotalMilliseconds:F3} ms (avg: {sw.Elapsed.TotalMilliseconds / HighFrequencyIterations * 1000.0:F3} us/op)");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        #endregion
    }
}
#pragma warning restore CS1591

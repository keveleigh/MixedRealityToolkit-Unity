// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Subsystems;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using UnityEngine.XR;
using HandshapeId = MixedReality.Toolkit.Input.HandshapeTypes.HandshapeId;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace MixedReality.Toolkit.Input.Tests
{
    using AggregatorDescriptor = MRTKSubsystemDescriptor<HandsAggregatorSubsystem, HandsAggregatorSubsystem.Provider>;

    /// <summary>
    /// Comprehensive test suite verifying accuracy, performance, lifecycle, and Zero-GC
    /// behavior of the HandsAggregatorSubsystem and MRTKHandsAggregatorSubsystem.
    /// </summary>
    public class HandsAggregatorSubsystemTests : BaseRuntimeInputTests
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

        [UnityTest]
        public IEnumerator HandsAggregator_LifecycleAndRunningState()
        {
            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.running);

            // Verify subsystem stop and start
            aggregator.Stop();
            Assert.IsFalse(aggregator.running);

            aggregator.Start();
            Assert.IsTrue(aggregator.running);

            yield return null;
        }

        [UnityTest]
        public IEnumerator HandsAggregator_TryGetJoint_AccuracyAndTracking()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Left hand is not showing yet; should return false
            bool leftSuccess = aggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.LeftHand, out HandJointPose leftPose);
            Assert.IsFalse(leftSuccess, "Hidden left hand joint query should return false.");

            // Right hand is showing; query all finger tips
            TrackedHandJoint[] testJoints = new[]
            {
                TrackedHandJoint.Palm,
                TrackedHandJoint.Wrist,
                TrackedHandJoint.ThumbTip,
                TrackedHandJoint.IndexTip,
                TrackedHandJoint.MiddleTip,
                TrackedHandJoint.RingTip,
                TrackedHandJoint.LittleTip
            };

            foreach (var joint in testJoints)
            {
                bool rightSuccess = aggregator.TryGetJoint(joint, XRNode.RightHand, out HandJointPose jointPose);
                Assert.IsTrue(rightSuccess, $"Right hand query for {joint} should succeed.");
                Assert.IsTrue(jointPose.Position != Vector3.zero, $"Joint {joint} position should not be zero.");
            }

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator HandsAggregator_TryGetEntireHand_Accuracy()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            bool success = aggregator.TryGetEntireHand(XRNode.RightHand, out IReadOnlyList<HandJointPose> joints);
            Assert.IsTrue(success, "TryGetEntireHand on active hand should succeed.");
            Assert.IsNotNull(joints, "Returned joint list must not be null.");
            Assert.AreEqual((int)TrackedHandJoint.TotalJoints, joints.Count, "Entire hand must contain TotalJoints (26) poses.");

            // Verify joint from full-hand query matches single-joint query
            aggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.RightHand, out HandJointPose singlePose);
            int indexTipIndex = (int)TrackedHandJoint.IndexTip;
            Assert.AreEqual(singlePose.Position, joints[indexTipIndex].Position, "TryGetEntireHand index joint must match TryGetJoint.");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator HandsAggregator_PinchPointsAndProgress()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Test interaction points
            bool nearPointSuccess = aggregator.TryGetNearInteractionPoint(XRNode.RightHand, out HandJointPose nearPose);
            Assert.IsTrue(nearPointSuccess, "TryGetNearInteractionPoint should succeed.");

            bool pinchPointSuccess = aggregator.TryGetPinchingPoint(XRNode.RightHand, out HandJointPose pinchPose);
            Assert.IsTrue(pinchPointSuccess, "TryGetPinchingPoint should succeed.");

            // Test Open handshape
            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            bool progressSuccess = aggregator.TryGetPinchProgress(XRNode.RightHand, out bool isReady, out bool isPinching, out float pinchAmount);
            Assert.IsTrue(progressSuccess, "TryGetPinchProgress should succeed on tracked hand.");
            Assert.IsFalse(isPinching, "Open hand must not be reported as pinching.");
            Assert.Less(pinchAmount, 0.5f, "Open hand pinch amount must be less than 0.5.");

            // Test Pinch handshape
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            progressSuccess = aggregator.TryGetPinchProgress(XRNode.RightHand, out isReady, out isPinching, out pinchAmount);
            Assert.IsTrue(progressSuccess, "TryGetPinchProgress should succeed on pinched hand.");
            Assert.IsTrue(isPinching, "Pinch handshape must be reported as pinching.");
            Assert.Greater(pinchAmount, 0.5f, "Pinch handshape pinch amount must be greater than 0.5.");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator HandsAggregator_TryGetJoint_PerfAndZeroGC()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            HandJointPose pose = default;

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                aggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.RightHand, out pose);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] HandsAggregator.TryGetJoint: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                aggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.RightHand, out pose);
            }, Is.Not.AllocatingGCMemory());

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator HandsAggregator_TryGetEntireHand_PerfAndZeroGC()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            IReadOnlyList<HandJointPose> joints = null;

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                aggregator.TryGetEntireHand(XRNode.RightHand, out joints);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] HandsAggregator.TryGetEntireHand: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                aggregator.TryGetEntireHand(XRNode.RightHand, out joints);
            }, Is.Not.AllocatingGCMemory());

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator HandsAggregator_TryGetPinchProgress_PerfAndZeroGC()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            bool isReady = false;
            bool isPinching = false;
            float pinchAmount = 0f;

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                aggregator.TryGetPinchProgress(XRNode.RightHand, out isReady, out isPinching, out pinchAmount);
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[Perf] HandsAggregator.TryGetPinchProgress: {usPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // 2. Unity's native Zero-GC assertion
            Assert.That(() =>
            {
                aggregator.TryGetPinchProgress(XRNode.RightHand, out isReady, out isPinching, out pinchAmount);
            }, Is.Not.AllocatingGCMemory());

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        [UnityTest]
        public IEnumerator HandsAggregator_NullCamera_DoesNotThrow()
        {
            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Temporarily untag the main camera
            Camera mainCam = Camera.main;
            string originalTag = mainCam != null ? mainCam.tag : null;
            if (mainCam != null)
            {
                mainCam.tag = "Untagged";
            }

            try
            {
                Assert.IsNull(Camera.main, "Camera.main must be null for this test.");

                // Must not throw NullReferenceException
                Assert.DoesNotThrow(() =>
                {
                    aggregator.TryGetPinchProgress(XRNode.RightHand, out bool isReady, out bool isPinching, out float pinchAmount);
                    aggregator.TryGetPalmFacingAway(XRNode.RightHand, out bool isAway);
                }, "HandsAggregator must handle null Camera.main safely without throwing NullReferenceException.");
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
    }
}
#pragma warning restore CS1591

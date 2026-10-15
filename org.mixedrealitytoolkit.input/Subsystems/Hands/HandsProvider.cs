// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Subsystems;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;
using UnityEngine.XR;

using System.Runtime.CompilerServices;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Provides a generic, base hands provider for subsystem use.
    /// </summary>
    /// <remarks>
    /// Extends the <see cref="MixedReality.Toolkit.Subsystems.HandsSubsystem.Provider">Provider</see> class and
    /// obtains hand joint poses from a <see cref="MixedReality.Toolkit.Input.HandDataContainer">HandDataContainer</see> class.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of <see cref="MixedReality.Toolkit.Input.HandDataContainer">HandDataContainer</see> to query hand data from.
    /// </typeparam>
    [Preserve]
    internal class HandsProvider<T> : HandsSubsystem.Provider where T : HandDataContainer
    {
        private T leftHand = null;
        private T rightHand = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetHand(XRNode handNode)
        {
            if (handNode == XRNode.LeftHand) { return leftHand; }
            if (handNode == XRNode.RightHand) { return rightHand; }
            return null;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            base.Start();

            leftHand ??= Activator.CreateInstance(typeof(T), XRNode.LeftHand) as T;
            rightHand ??= Activator.CreateInstance(typeof(T), XRNode.RightHand) as T;

            InputSystem.onBeforeUpdate += ResetHands;
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            ResetHands();
            InputSystem.onBeforeUpdate -= ResetHands;
            base.Stop();
        }

        private void ResetHands()
        {
            leftHand?.Reset();
            rightHand?.Reset();
        }

        #region IHandsSubsystem implementation

        /// <inheritdoc/>
        public override bool TryGetEntireHand(XRNode handNode, out IReadOnlyList<HandJointPose> jointPoses)
        {
            Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand, "Non-hand XRNode used in TryGetEntireHand query.");

            if (!MRTKFocusFeature.XrSessionFocused.Value)
            {
                jointPoses = Array.Empty<HandJointPose>();
                return false;
            }

            T hand = GetHand(handNode);
            if (hand == null)
            {
                jointPoses = Array.Empty<HandJointPose>();
                return false;
            }

            return hand.TryGetEntireHand(out jointPoses);
        }

        /// <inheritdoc/>
        public override bool TryGetJoint(TrackedHandJoint joint, XRNode handNode, out HandJointPose jointPose)
        {
            Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand, "Non-hand XRNode used in TryGetJoint query.");

            if (!MRTKFocusFeature.XrSessionFocused.Value)
            {
                jointPose = default;
                return false;
            }

            T hand = GetHand(handNode);
            if (hand == null)
            {
                jointPose = default;
                return false;
            }

            return hand.TryGetJoint(joint, out jointPose);
        }

        #endregion IHandsSubsystem implementation
    }
}

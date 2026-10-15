// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Subsystems;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A pose source which represents a hand ray. This hand ray is constructed by deriving it from the
    /// palm and knuckle positions
    /// </summary>
    [Serializable]
    public class PolyfillHandRayPoseSource : HandBasedPoseSource
    {
        // The Hand Ray used to calculate the polyfill.
        private HandRay handRay = new HandRay();

        /// <summary>
        /// A cache of the knuckle joint pose returned by the hands aggregator.
        /// </summary>
        private HandJointPose knuckle;

        /// <summary>
        /// A cache of the knuckle joint pose returned by the hands aggregator.
        /// </summary>
        private HandJointPose palm;

        private const string InvalidHandErrorMessage = "The PolyfillHandRayPoseSource does not have a valid hand assigned.";

        /// <summary>
        /// Tries to get the pose of the hand ray in world space by deriving it from the
        /// palm and knuckle positions
        /// </summary>
        public override bool TryGetPose(out Pose pose)
        {
            Debug.Assert(Hand == Handedness.Left || Hand == Handedness.Right, InvalidHandErrorMessage);

            XRNode? handNode = Hand.ToXRNode();

            if (!handNode.HasValue)
            {
                pose = Pose.identity;
                return false;
            }

            Camera mainCamera = Camera.main;
            bool poseRetrieved = mainCamera != null;
            poseRetrieved &= XRSubsystemHelpers.HandsAggregator?.TryGetJoint(TrackedHandJoint.IndexProximal, handNode.Value, out knuckle) ?? false;
            poseRetrieved &= XRSubsystemHelpers.HandsAggregator?.TryGetJoint(TrackedHandJoint.Palm, handNode.Value, out palm) ?? false;

            // Tick the hand ray generator function. Uses index knuckle for position.
            if (poseRetrieved)
            {
                handRay.Update(knuckle.Position, -palm.Up, mainCamera.transform, Hand);

                pose = new Pose(
                    handRay.Ray.origin,
                    Quaternion.LookRotation(handRay.Ray.direction, palm.Up));
            }
            else
            {
                pose = Pose.identity;
            }

            return poseRetrieved;
        }
    }
}

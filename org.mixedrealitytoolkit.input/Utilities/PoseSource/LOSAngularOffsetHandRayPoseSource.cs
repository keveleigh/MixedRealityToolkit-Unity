// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A pose source which represents a hand ray. This hand ray is constructed by deriving it from a
    /// line of sight (LOS) head to knuckle vector with angular offset applied.
    /// </summary>
    [Serializable]
    public class LOSAngularOffsetHandRayPoseSource : HandBasedPoseSource
    {
        [SerializeField]
        [Tooltip("The half life used for the position of the StabilizedRay.  A value of 0 means no stabilization/smoothing.")]
        private float stabilizedPositionHalfLife = .01f;

        [SerializeField]
        [Tooltip("The half life used for the direction of the StabilizedRay.  A value of 0 means no stabilization/smoothing.")]
        private float stabilizedDirectionHalfLife = .05f;

        /// <summary>
        /// The StabilizedRay used in hand ray stabilization calculations.
        /// </summary>
        protected Lazy<StabilizedRay> StabilizedHandRay { get; private set; }

        /// <summary>
        /// A cache of the knuckle joint pose returned by the hands aggregator.
        /// </summary>
        private HandJointPose knuckle;

        private const float HeadCenterOffset = .09f;
        private readonly Vector2 MinMaxYawAngleOffset = new Vector2(-12f, -35f);
        private readonly Vector2 MinMaxPitchAngleOffset = new Vector2(-24f, -85f);
        private const float MinMaxAngleAdjustHandProximity = .5f;

        private const string InvalidHandErrorMessage = "The LOSAngularOffsetHandRayPoseSource does not have a valid hand assigned.";

        public LOSAngularOffsetHandRayPoseSource()
        {
            StabilizedHandRay = new Lazy<StabilizedRay>(() =>
            {
                return new StabilizedRay(stabilizedPositionHalfLife, stabilizedDirectionHalfLife);
            });
        }

        /// <summary>
        /// Tries to get the pose of the hand ray in world space by deriving it from a
        /// line of sight head to knuckle vector with angular offset applied.
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

            if (poseRetrieved)
            {
                pose = CalculateHandRay(knuckle.Position, mainCamera.transform, Hand);
            }
            else
            {
                pose = Pose.identity;
            }

            return poseRetrieved;
        }

        private Pose CalculateHandRay(Vector3 handJointPosition, Transform headTransform, Handedness hand)
        {
            Vector3 headPosition = headTransform.position;
            Vector3 headForward = headTransform.forward;
            Vector3 headUp = headTransform.up;

            // Approximate head center to reduce the head rotation wobble effect on the hand ray.
            Vector3 headCenter = headPosition - headForward * HeadCenterOffset;
            Vector3 headToJoint = handJointPosition - headCenter;

            // Adjust yaw and pitch angle offsets to more closely approximate the original hand ray orientation behavior affected by hand proximity.
            float t = Mathf.InverseLerp(MinMaxAngleAdjustHandProximity, 0, headToJoint.magnitude);
            float yawOffset = Mathf.Lerp(MinMaxYawAngleOffset.x, MinMaxYawAngleOffset.y, t) * (hand == Handedness.Right ? 1.0f : -1.0f);
            float pitchOffset = Mathf.Lerp(MinMaxPitchAngleOffset.x, MinMaxPitchAngleOffset.y, t);

            Quaternion rayRotation = Quaternion.LookRotation(headToJoint, headUp) * Quaternion.Euler(pitchOffset, yawOffset, 0);

            if (stabilizedPositionHalfLife > 0 || stabilizedDirectionHalfLife > 0)
            {
                StabilizedHandRay.Value.AddSample(new Ray(handJointPosition, rayRotation * Vector3.forward));

                return new Pose(StabilizedHandRay.Value.StabilizedPosition, Quaternion.LookRotation(StabilizedHandRay.Value.StabilizedDirection, headUp));
            }
            else
            {
                return new Pose(handJointPosition, rayRotation);
            }
        }
    }
}

// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using UnityEngine;

using HandshapeId = MixedReality.Toolkit.Input.HandshapeTypes.HandshapeId;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Convenience wrapper around the underlying <see cref="InputTestUtilities"/> functions
    /// for manipulating test hands.
    /// </summary>
    public class TestHand : TestController
    {
        private HandshapeId handshapeId = HandshapeId.Open;

        public TestHand(Handedness handedness) : base(handedness) { }

        /// <inheritdoc />
        public override IEnumerator Show(Vector3 position, bool waitForFixedUpdate = true)
        {
            yield return InputTestUtilities.SetHandTrackingState(handedness, true);
            yield return MoveTo(position, 2);
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <inheritdoc />
        public override IEnumerator Show(bool waitForFixedUpdate = true)
        {
            yield return InputTestUtilities.SetHandTrackingState(handedness, true);
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <inheritdoc />
        public override IEnumerator Hide(bool waitForFixedUpdate = true)
        {
            yield return InputTestUtilities.SetHandTrackingState(handedness, false);
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <inheritdoc />
        public override IEnumerator MoveTo(Vector3 newPosition, int numSteps = InputTestUtilities.ControllerMoveStepsSentinelValue, bool waitForFixedUpdate = true)
        {
            yield return InputTestUtilities.MoveHandTo(newPosition, handshapeId, handedness, numSteps);
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <inheritdoc />
        public override IEnumerator Move(Vector3 delta, int numSteps = InputTestUtilities.ControllerMoveStepsSentinelValue, bool waitForFixedUpdate = true)
        {
            Vector3 currentPosition = InputTestUtilities.GetHandPose(handedness).position;
            yield return MoveTo(currentPosition + delta, InputTestUtilities.CalculateNumSteps(numSteps));
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <inheritdoc />
        public override IEnumerator RotateTo(Quaternion newRotation, int numSteps = InputTestUtilities.ControllerMoveStepsSentinelValue, bool waitForFixedUpdate = true)
        {
            yield return InputTestUtilities.RotateHandTo(newRotation, handshapeId, handedness, numSteps);
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <inheritdoc />
        public override IEnumerator AimAt(Vector3 target, int numSteps = InputTestUtilities.ControllerMoveStepsSentinelValue, bool waitForFixedUpdate = true)
        {
            InputTestUtilities.SetHandAnchorPoint(handedness, Simulation.ControllerAnchorPoint.Device);
            Vector3 currentPosition = InputTestUtilities.GetHandPose(handedness).position;
            yield return InputTestUtilities.RotateHandTo(Quaternion.LookRotation(target - currentPosition, Vector3.up), handshapeId, handedness);
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Changes the hand's pose to the given handshape. Does not animate the hand between the current pose and new pose.
        /// </summary>
        /// <param name="newHandshapeId">The new hand pose.</param>
        /// <param name="numSteps">The number of steps to take to reach the new hand pose.</param>
        /// <param name="waitForFixedUpdate">If true, waits for a fixed update after moving to the new pose.</param>
        public IEnumerator SetHandshape(HandshapeId newHandshapeId, int numSteps = InputTestUtilities.ControllerMoveStepsSentinelValue, bool waitForFixedUpdate = true)
        {
            handshapeId = newHandshapeId;
            yield return InputTestUtilities.SetHandshape(handshapeId, handedness, numSteps);
            if (waitForFixedUpdate)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Combined sequence of pinching and unpinching.
        /// </summary>
        public override IEnumerator Click()
        {
            yield return SetHandshape(HandshapeId.Pinch);
            yield return null;
            yield return SetHandshape(HandshapeId.Open);
            yield return null;
        }
    }
}
#pragma warning restore CS1591

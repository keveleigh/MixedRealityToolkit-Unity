// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.UX.Experimental;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for the <see cref="Scrollable"/> class.
    /// </summary>
    public class ScrollableTests : BaseRuntimeInputTests
    {
        private const string VerticalAndHorizontalScrollTestPrefab = "5fded42d542378f4c9d2991767eb039b";
        private const string VerticalScrollTestPrefab = "fc9628599d466c44aa5453aba06c93ef";
        private const string HorizontalScrollTestPrefab = "7eb6a6dd3c4be864da6b5a84c7976573";
        GameObject scrollObject;
        Scrollable scrollable;
        PressableButton firstPressableButton;
        TestHand hand;
        bool firstPressableButtonClicked;
        Vector2 startScrollPosition;

        public override IEnumerator Setup()
        {
            yield return base.Setup();
            firstPressableButtonClicked = false;
            startScrollPosition = Vector2.zero;
            hand = new TestHand(Handedness.Right);
        }

        public override IEnumerator TearDown()
        {
            if (scrollObject != null)
            {
                Object.Destroy(scrollObject);
                // Wait for a frame to give Unity a change to actually destroy the object
                yield return null;
                Assert.IsTrue(scrollObject == null);
            }
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator TestCreationOfVerticalAndHorizontalScrollTestPrefab()
        {
            yield return InitializeScrollObject(VerticalAndHorizontalScrollTestPrefab);
            ValidateTestComponents();
        }

        [UnityTest]
        public IEnumerator TestCreationOfHorizontalScrollTestPrefab()
        {
            yield return InitializeScrollObject(HorizontalScrollTestPrefab);
            ValidateTestComponents();
        }

        [UnityTest]
        public IEnumerator TestCreationOfVerticalScrollTestPrefab()
        {
            yield return InitializeScrollObject(VerticalScrollTestPrefab);
            ValidateTestComponents();
        }

        [UnityTest]
        public IEnumerator TestSmallHandRayMovementsDoNotCancelButtonSelection()
        {
            yield return InitializeScrollObject(VerticalAndHorizontalScrollTestPrefab);
            Vector3 smallMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)).normalized * -(scrollable.CancelSelectDistance);

            yield return ShowHand();
            yield return hand.AimAt(firstPressableButton.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(firstPressableButtonClicked, "The button should not have been clicked yet.");

            yield return hand.AimAt(firstPressableButton.transform.position + smallMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(firstPressableButtonClicked, "The button should have been clicked.");
        }

        [UnityTest]
        public IEnumerator TestHandRayMovementsCancelButtonSelection()
        {
            yield return InitializeScrollObject(VerticalAndHorizontalScrollTestPrefab);
            Vector3 smallMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)).normalized * -(scrollable.CancelSelectDistance + 0.02f);

            yield return ShowHand();
            yield return hand.AimAt(firstPressableButton.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(firstPressableButtonClicked, "The button should not have been clicked yet.");

            yield return hand.AimAt(firstPressableButton.transform.position + smallMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(firstPressableButtonClicked, "The button should not have been clicked since 'Scrollable' moved more than the 'CancelSelectDistance'.");
        }

        [UnityTest]
        public IEnumerator TestSmallHandRayMovementsDoNotScroll()
        {
            yield return InitializeScrollObject(VerticalAndHorizontalScrollTestPrefab);
            Vector3 smallMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)).normalized * -(scrollable.DeadZone);

            yield return ShowHand();
            yield return hand.AimAt(firstPressableButton.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(0.0f, GetScrollDistance(), "The scroller shouldn't have moved yet.");

            yield return hand.AimAt(firstPressableButton.transform.position + smallMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(0.0f, GetScrollDistance(), "The scroller shouldn't have moved still.");
        }

        [UnityTest]
        public IEnumerator TestHandRayMovementsScrolls()
        {
            yield return InitializeScrollObject(VerticalAndHorizontalScrollTestPrefab);
            Vector3 smallMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)).normalized * -(scrollable.DeadZone + 0.02f);

            yield return ShowHand();
            yield return hand.AimAt(firstPressableButton.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(0.0f, GetScrollDistance(), "The scroller shouldn't have moved yet.");

            yield return hand.AimAt(firstPressableButton.transform.position + smallMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.Less(0.0f, GetScrollDistance(), "The scroller should have moved.");
        }

        [UnityTest]
        public IEnumerator TestHandRayMovementsScrollsWithDisabledButtons()
        {
            yield return InitializeScrollObject(VerticalAndHorizontalScrollTestPrefab);

            // Disable the buttons, so we can test that the scroll still works.
            SetEnableOnPressableButtons(scrollObject, false);

            // Disabbling buttons should disable their colliders.
            VerifyButtonCollidersAreDisabled(scrollObject);

            SetEnableOnPressableButtons(scrollObject, false);
            Vector3 smallMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)).normalized * -(scrollable.DeadZone + 0.02f);

            yield return ShowHand();
            yield return hand.AimAt(firstPressableButton.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(0.0f, GetScrollDistance(), "The scroller shouldn't have moved yet.");

            yield return hand.AimAt(firstPressableButton.transform.position + smallMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.Less(0.0f, GetScrollDistance(), "The scroller should have moved.");

            // Verify that the buttons are still disabled.
            VerifyButtonCollidersAreDisabled(scrollObject);
        }

        [UnityTest]
        public IEnumerator TestHandRayMovementsScrollsWithDisabledButtonsLargeMovement()
        {
            yield return InitializeScrollObject(VerticalAndHorizontalScrollTestPrefab);

            // Disable the buttons, so we can test that the scroll still works.
            SetEnableOnPressableButtons(scrollObject, false);

            // Disabbling buttons should disable their colliders.
            VerifyButtonCollidersAreDisabled(scrollObject);

            Vector3 largeMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)).normalized * -20f;

            var startAimAt = firstPressableButton.transform.position;

            yield return ShowHand();
            yield return hand.AimAt(startAimAt);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(0.0f, GetScrollDistance(), "The scroller shouldn't have moved yet.");

            yield return hand.AimAt(startAimAt + largeMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.Less(0.0f, GetScrollDistance(), "The scroller should have moved.");

            // Move scroll view the other way
            yield return hand.AimAt(startAimAt);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.AimAt(startAimAt - largeMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Verify that the buttons are still disabled.
            VerifyButtonCollidersAreDisabled(scrollObject);
        }

        [UnityTest]
        public IEnumerator TestVerticalScrolling()
        {
            yield return InitializeScrollObject(VerticalScrollTestPrefab);
            Vector3 smallMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(0.0f, 1.0f, 0.0f)).normalized * (scrollable.DeadZone * 2.0f);

            yield return ShowHand();
            yield return hand.AimAt(firstPressableButton.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(0.0f, GetScrollDistance(), "The scroller shouldn't have moved yet.");

            yield return hand.AimAt(firstPressableButton.transform.position + smallMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.Less(0.0f, GetScrollDistance(), "The scroller should have moved.");
            Assert.IsFalse(firstPressableButtonClicked, "The button should not have been clicked since 'Scrollable' moved more than the 'CancelSelectDistance'.");
        }

        [UnityTest]
        public IEnumerator TestHorizontalScrolling()
        {
            yield return InitializeScrollObject(HorizontalScrollTestPrefab);
            Vector3 smallMovement = scrollable.ScrollRect.transform.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)).normalized * -(scrollable.DeadZone * 2.0f);

            yield return ShowHand();
            yield return hand.AimAt(firstPressableButton.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(0.0f, GetScrollDistance(), "The scroller shouldn't have moved yet.");

            yield return hand.AimAt(firstPressableButton.transform.position + smallMovement);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return hand.SetHandshape(Input.HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.Less(0.0f, GetScrollDistance(), "The scroller should have moved.");
            Assert.IsFalse(firstPressableButtonClicked, "The button should not have been clicked since 'Scrollable' moved more than the 'CancelSelectDistance'.");
        }

        private float GetScrollDistance()
        {
            if (scrollable == null || scrollable.ScrollRect == null)
            {
                return 0f;
            }

            return scrollable.ScrollRect.transform.TransformVector(
                startScrollPosition - scrollable.ScrollRect.normalizedPosition).magnitude;
        }

        private IEnumerator ShowHand()
        {
            Vector3 initialHandPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, -0.05f, 0.3f));
            yield return hand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        private void ValidateTestComponents()
        {
            Assert.IsNotNull(scrollObject, "Scroll prefab was not created.");
            Assert.IsNotNull(scrollable, "Scrollable was not found.");
            Assert.IsNotNull(scrollable.ScrollRect, "Scrollable scroll rect was not specified.");
            Assert.IsNotNull(firstPressableButton, "Pressable button was not found.");
        }

        private void SetEnableOnPressableButtons(GameObject container, bool enable)
        {
            if (container != null)
            {
                PressableButton[] pressableButtons = container.GetComponentsInChildren<PressableButton>();
                foreach (PressableButton pressableButton in pressableButtons)
                {
                    pressableButton.enabled = enable;
                }
            }
        }

        private void VerifyButtonCollidersAreDisabled(GameObject container)
        {
            if (container != null)
            {
                PressableButton[] pressableButtons = container.GetComponentsInChildren<PressableButton>();
                foreach (PressableButton pressableButton in pressableButtons)
                {
                    Assert.IsFalse(pressableButton.enabled, "Pressable button should be disabled.");
                }
            }
        }

        private IEnumerator InitializeScrollObject(string prefabGuid)
        {
            if (scrollObject != null)
            {
                Object.Destroy(scrollObject);
                yield return null;
            }

            scrollObject = InstantiatePrefab(prefabGuid);
            yield return null;

            scrollable = scrollObject.GetComponentInChildren<Scrollable>();
            firstPressableButton = scrollObject.GetComponentInChildren<PressableButton>();
            scrollObject.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.0f, 0.0f, 0.75f));

            if (scrollable != null &&
                scrollable.ScrollRect != null)
            {
                startScrollPosition = scrollable.ScrollRect.normalizedPosition;
            }

            if (firstPressableButton != null)
            {
                firstPressableButton.OnClicked.AddListener(() =>
                {
                    firstPressableButtonClicked = true;
                });
            }
        }

        private GameObject InstantiatePrefab(string prefabGuid)
        {
            Object pressableButtonPrefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(prefabGuid), typeof(Object));
            GameObject testGO = Object.Instantiate(pressableButtonPrefab) as GameObject;
            return testGO;
        }
    }
}
#pragma warning restore CS1591

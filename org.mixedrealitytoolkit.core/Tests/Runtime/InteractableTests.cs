// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace MixedReality.Toolkit.Core.Tests
{
    #region Mock Interactors

    internal class MockGazeInteractor : XRRayInteractor, IGazeInteractor { }

    internal class MockGazePinchInteractor : XRRayInteractor, IGazePinchInteractor
    {
        public float SelectProgress { get; set; } = 0f;
    }

    internal class MockPokeInteractor : XRRayInteractor, IPokeInteractor
    {
        public float PokeRadius => 0.005f;
        public IPokeInteractor.PokePath PokeTrajectory => default;
    }

    internal class MockGrabInteractor : XRRayInteractor, IGrabInteractor { }

    internal class MockRayInteractor : XRRayInteractor, IRayInteractor { }

    internal class MockVariableSelectInteractor : XRRayInteractor, IVariableSelectInteractor
    {
        public float SelectProgress { get; set; } = 0.5f;
    }

    #endregion Mock Interactors

    /// <summary>
    /// Comprehensive test suite for <see cref="MRTKBaseInteractable"/> and <see cref="StatefulInteractable"/>.
    /// Validates collection semantics, hover/select lifecycles, interactor filtering,
    /// toggle modes, variable selection progress, and hot-path zero-GC allocations.
    /// </summary>
    public class InteractableTests : BaseRuntimeTests
    {
        private const int BenchmarkIterations = 10000;

        private GameObject rootObject;
        private GameObject managerObject;
        private XRInteractionManager interactionManager;
        private GameObject interactableObject;
        private GameObject interactorContainer;

        public override IEnumerator Setup()
        {
            yield return base.Setup();

            rootObject = new GameObject("InteractableTestRoot");
            managerObject = new GameObject("InteractionManager");
            managerObject.transform.SetParent(rootObject.transform, false);
            interactionManager = managerObject.AddComponent<XRInteractionManager>();

            interactableObject = new GameObject("InteractableObject");
            interactableObject.transform.SetParent(rootObject.transform, false);
            interactableObject.AddComponent<BoxCollider>();

            interactorContainer = new GameObject("InteractorContainer");
            interactorContainer.transform.SetParent(rootObject.transform, false);

            yield return null;
        }

        public override IEnumerator TearDown()
        {
            if (rootObject != null)
            {
                Object.Destroy(rootObject);
                rootObject = null;
            }

            yield return base.TearDown();
        }

        private T CreateInteractor<T>(string name = null) where T : XRBaseInteractor
        {
            var go = new GameObject(name ?? typeof(T).Name);
            go.transform.SetParent(interactorContainer.transform, false);
            return go.AddComponent<T>();
        }

        private void HoverEnter(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            interactionManager.HoverEnter(interactor, interactable);
        }

        private void HoverExit(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            interactionManager.HoverExit(interactor, interactable);
        }

        private void SelectEnter(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            interactionManager.SelectEnter(interactor, interactable);
        }

        private void SelectExit(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            interactionManager.SelectExit(interactor, interactable);
        }

        #region Initialization Tests

        [UnityTest]
        public IEnumerator InitialState_MRTKBaseInteractable_ListsAreEmptyAndSafe()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            yield return null;

            // Validate that all interactor lists are non-null and empty
            Assert.IsNotNull(interactable.HoveringGazeInteractors, "HoveringGazeInteractors must not be null.");
            Assert.AreEqual(0, interactable.HoveringGazeInteractors.Count, "HoveringGazeInteractors should be empty initially.");

            Assert.IsNotNull(interactable.HoveringGazePinchInteractors, "HoveringGazePinchInteractors must not be null.");
            Assert.AreEqual(0, interactable.HoveringGazePinchInteractors.Count, "HoveringGazePinchInteractors should be empty initially.");

            Assert.IsNotNull(interactable.SelectingGazePinchInteractors, "SelectingGazePinchInteractors must not be null.");
            Assert.AreEqual(0, interactable.SelectingGazePinchInteractors.Count, "SelectingGazePinchInteractors should be empty initially.");

            Assert.IsNotNull(interactable.HoveringPokeInteractors, "HoveringPokeInteractors must not be null.");
            Assert.AreEqual(0, interactable.HoveringPokeInteractors.Count, "HoveringPokeInteractors should be empty initially.");

            Assert.IsNotNull(interactable.HoveringGrabInteractors, "HoveringGrabInteractors must not be null.");
            Assert.AreEqual(0, interactable.HoveringGrabInteractors.Count, "HoveringGrabInteractors should be empty initially.");

            Assert.IsNotNull(interactable.SelectingGrabInteractors, "SelectingGrabInteractors must not be null.");
            Assert.AreEqual(0, interactable.SelectingGrabInteractors.Count, "SelectingGrabInteractors should be empty initially.");

            Assert.IsNotNull(interactable.HoveringRayInteractors, "HoveringRayInteractors must not be null.");
            Assert.AreEqual(0, interactable.HoveringRayInteractors.Count, "HoveringRayInteractors should be empty initially.");

            // Validate all TimedFlags are initialized and inactive
            Assert.IsFalse(interactable.IsGazeHovered.Active, "IsGazeHovered should be inactive initially.");
            Assert.IsFalse(interactable.IsGazePinchHovered.Active, "IsGazePinchHovered should be inactive initially.");
            Assert.IsFalse(interactable.IsRayHovered.Active, "IsRayHovered should be inactive initially.");
            Assert.IsFalse(interactable.IsGrabHovered.Active, "IsGrabHovered should be inactive initially.");
            Assert.IsFalse(interactable.IsPokeHovered.Active, "IsPokeHovered should be inactive initially.");
            Assert.IsFalse(interactable.IsActiveHovered.Active, "IsActiveHovered should be inactive initially.");

            Assert.IsFalse(interactable.IsGazePinchSelected.Active, "IsGazePinchSelected should be inactive initially.");
            Assert.IsFalse(interactable.IsRaySelected.Active, "IsRaySelected should be inactive initially.");
            Assert.IsFalse(interactable.IsPokeSelected.Active, "IsPokeSelected should be inactive initially.");
            Assert.IsFalse(interactable.IsGrabSelected.Active, "IsGrabSelected should be inactive initially.");
        }

        [UnityTest]
        public IEnumerator InitialState_StatefulInteractable_DefaultsAreValid()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            yield return null;

            Assert.AreEqual(StatefulInteractable.ToggleType.Button, interactable.ToggleMode);
            Assert.AreEqual(0.9f, interactable.SelectThreshold);
            Assert.AreEqual(0.1f, interactable.DeselectThreshold);
            Assert.IsTrue(interactable.TriggerOnRelease);
            Assert.IsFalse(interactable.UseGazeDwell);
            Assert.IsFalse(interactable.UseFarDwell);
            Assert.IsFalse(interactable.IsToggled.Active);
            Assert.AreEqual(0f, interactable.GetSelectionProgress());
        }

        #endregion Initialization Tests

        #region Hover Lifecycle Tests

        [UnityTest]
        public IEnumerator HoverLifecycle_GazeInteractor()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var gaze = CreateInteractor<MockGazeInteractor>();
            yield return null;

            HoverEnter(gaze, interactable);
            Assert.IsTrue(interactable.IsGazeHovered.Active, "IsGazeHovered should be true when gaze enters.");
            Assert.IsFalse(interactable.IsActiveHovered.Active, "Passive gaze should NOT activate IsActiveHovered.");
            Assert.AreEqual(1, interactable.HoveringGazeInteractors.Count);
            Assert.AreSame(gaze, interactable.HoveringGazeInteractors[0]);

            HoverExit(gaze, interactable);
            Assert.IsFalse(interactable.IsGazeHovered.Active, "IsGazeHovered should be false when gaze exits.");
            Assert.AreEqual(0, interactable.HoveringGazeInteractors.Count);
        }

        [UnityTest]
        public IEnumerator HoverLifecycle_GazePinchInteractor()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var gazePinch = CreateInteractor<MockGazePinchInteractor>();
            yield return null;

            HoverEnter(gazePinch, interactable);
            Assert.IsTrue(interactable.IsGazePinchHovered.Active, "IsGazePinchHovered should be active.");
            Assert.IsTrue(interactable.IsActiveHovered.Active, "GazePinch should activate IsActiveHovered.");
            Assert.AreEqual(1, interactable.HoveringGazePinchInteractors.Count);
            Assert.AreSame(gazePinch, interactable.HoveringGazePinchInteractors[0]);

            HoverExit(gazePinch, interactable);
            Assert.IsFalse(interactable.IsGazePinchHovered.Active);
            Assert.IsFalse(interactable.IsActiveHovered.Active);
            Assert.AreEqual(0, interactable.HoveringGazePinchInteractors.Count);
        }

        [UnityTest]
        public IEnumerator HoverLifecycle_PokeInteractor()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var poke = CreateInteractor<MockPokeInteractor>();
            yield return null;

            HoverEnter(poke, interactable);
            Assert.IsTrue(interactable.IsPokeHovered.Active, "IsPokeHovered should be active.");
            Assert.IsTrue(interactable.IsActiveHovered.Active, "Poke should activate IsActiveHovered.");
            Assert.AreEqual(1, interactable.HoveringPokeInteractors.Count);

            HoverExit(poke, interactable);
            Assert.IsFalse(interactable.IsPokeHovered.Active);
            Assert.IsFalse(interactable.IsActiveHovered.Active);
            Assert.AreEqual(0, interactable.HoveringPokeInteractors.Count);
        }

        [UnityTest]
        public IEnumerator HoverLifecycle_GrabInteractor()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var grab = CreateInteractor<MockGrabInteractor>();
            yield return null;

            HoverEnter(grab, interactable);
            Assert.IsTrue(interactable.IsGrabHovered.Active, "IsGrabHovered should be active.");
            Assert.IsTrue(interactable.IsActiveHovered.Active, "Grab should activate IsActiveHovered.");
            Assert.AreEqual(1, interactable.HoveringGrabInteractors.Count);

            HoverExit(grab, interactable);
            Assert.IsFalse(interactable.IsGrabHovered.Active);
            Assert.IsFalse(interactable.IsActiveHovered.Active);
            Assert.AreEqual(0, interactable.HoveringGrabInteractors.Count);
        }

        [UnityTest]
        public IEnumerator HoverLifecycle_RayInteractor()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var ray = CreateInteractor<MockRayInteractor>();
            yield return null;

            HoverEnter(ray, interactable);
            Assert.IsTrue(interactable.IsRayHovered.Active, "IsRayHovered should be active.");
            Assert.IsTrue(interactable.IsActiveHovered.Active, "Ray should activate IsActiveHovered.");
            Assert.AreEqual(1, interactable.HoveringRayInteractors.Count);

            HoverExit(ray, interactable);
            Assert.IsFalse(interactable.IsRayHovered.Active);
            Assert.IsFalse(interactable.IsActiveHovered.Active);
            Assert.AreEqual(0, interactable.HoveringRayInteractors.Count);
        }

        [UnityTest]
        public IEnumerator HoverLifecycle_MultipleInteractorsConcurrent()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var ray = CreateInteractor<MockRayInteractor>("Ray1");
            var grab = CreateInteractor<MockGrabInteractor>("Grab1");
            yield return null;

            HoverEnter(ray, interactable);
            HoverEnter(grab, interactable);

            Assert.IsTrue(interactable.IsRayHovered.Active);
            Assert.IsTrue(interactable.IsGrabHovered.Active);
            Assert.IsTrue(interactable.IsActiveHovered.Active);
            Assert.AreEqual(1, interactable.HoveringRayInteractors.Count);
            Assert.AreEqual(1, interactable.HoveringGrabInteractors.Count);

            // Exit ray: grab is still hovering, so IsActiveHovered must remain true
            HoverExit(ray, interactable);
            Assert.IsFalse(interactable.IsRayHovered.Active);
            Assert.IsTrue(interactable.IsGrabHovered.Active);
            Assert.IsTrue(interactable.IsActiveHovered.Active);

            // Exit grab: all exited, IsActiveHovered must become false
            HoverExit(grab, interactable);
            Assert.IsFalse(interactable.IsGrabHovered.Active);
            Assert.IsFalse(interactable.IsActiveHovered.Active);
        }

        #endregion Hover Lifecycle Tests

        #region Select Lifecycle Tests

        [UnityTest]
        public IEnumerator SelectLifecycle_GazePinchInteractor()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var gazePinch = CreateInteractor<MockGazePinchInteractor>();
            yield return null;

            SelectEnter(gazePinch, interactable);
            Assert.IsTrue(interactable.IsGazePinchSelected.Active);
            Assert.AreEqual(1, interactable.SelectingGazePinchInteractors.Count);
            Assert.AreSame(gazePinch, interactable.SelectingGazePinchInteractors[0]);

            SelectExit(gazePinch, interactable);
            Assert.IsFalse(interactable.IsGazePinchSelected.Active);
            Assert.AreEqual(0, interactable.SelectingGazePinchInteractors.Count);
        }

        [UnityTest]
        public IEnumerator SelectLifecycle_VariousInteractors()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var ray = CreateInteractor<MockRayInteractor>();
            var poke = CreateInteractor<MockPokeInteractor>();
            var grab = CreateInteractor<MockGrabInteractor>();
            yield return null;

            // Ray select
            SelectEnter(ray, interactable);
            Assert.IsTrue(interactable.IsRaySelected.Active);
            SelectExit(ray, interactable);
            Assert.IsFalse(interactable.IsRaySelected.Active);

            // Poke select
            SelectEnter(poke, interactable);
            Assert.IsTrue(interactable.IsPokeSelected.Active);
            SelectExit(poke, interactable);
            Assert.IsFalse(interactable.IsPokeSelected.Active);

            // Grab select
            SelectEnter(grab, interactable);
            Assert.IsTrue(interactable.IsGrabSelected.Active);
            SelectExit(grab, interactable);
            Assert.IsFalse(interactable.IsGrabSelected.Active);
        }

        #endregion Select Lifecycle Tests

        #region Interactor Filtering Tests

        [UnityTest]
        public IEnumerator InteractorFiltering_DisableAndEnableType()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var ray = CreateInteractor<MockRayInteractor>();
            var poke = CreateInteractor<MockPokeInteractor>();
            yield return null;

            // Initially valid
            Assert.IsTrue(interactable.IsInteractorTypeValid(ray));
            Assert.IsTrue(interactable.IsInteractorTypeValid(poke));
            Assert.IsTrue(interactable.IsHoverableBy((IXRHoverInteractor)ray));
            Assert.IsTrue(interactable.IsSelectableBy((IXRSelectInteractor)ray));

            // Disable MockRayInteractor
            var rayType = new SystemInterfaceType(typeof(MockRayInteractor));
            interactable.DisableInteractorType(rayType);

            Assert.IsFalse(interactable.IsInteractorTypeValid(ray), "Ray should be disabled.");
            Assert.IsTrue(interactable.IsInteractorTypeValid(poke), "Poke should remain valid.");
            Assert.IsFalse(interactable.IsHoverableBy((IXRHoverInteractor)ray), "Ray should not be hoverable.");
            Assert.IsFalse(interactable.IsSelectableBy((IXRSelectInteractor)ray), "Ray should not be selectable.");
            Assert.IsTrue(interactable.IsHoverableBy((IXRHoverInteractor)poke));

            // Re-enable MockRayInteractor
            interactable.EnableInteractorType(rayType);
            Assert.IsTrue(interactable.IsInteractorTypeValid(ray), "Ray should be valid after re-enabling.");
            Assert.IsTrue(interactable.IsHoverableBy((IXRHoverInteractor)ray));
        }

        [UnityTest]
        public IEnumerator InteractorFiltering_BaseInterfaceDisabling()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var ray = CreateInteractor<MockRayInteractor>();
            var grab = CreateInteractor<MockGrabInteractor>();
            yield return null;

            // Disable IXRInteractor interface -> affects all interactors implementing it
            var interactorInterfaceType = new SystemInterfaceType(typeof(IXRInteractor));
            interactable.DisableInteractorType(interactorInterfaceType);

            Assert.IsFalse(interactable.IsInteractorTypeValid(ray));
            Assert.IsFalse(interactable.IsInteractorTypeValid(grab));

            interactable.EnableInteractorType(interactorInterfaceType);
            Assert.IsTrue(interactable.IsInteractorTypeValid(ray));
            Assert.IsTrue(interactable.IsInteractorTypeValid(grab));
        }

        #endregion Interactor Filtering Tests

        #region StatefulInteractable Tests

        [UnityTest]
        public IEnumerator StatefulInteractable_ToggleMode_ButtonDoesNotToggle()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            interactable.ToggleMode = StatefulInteractable.ToggleType.Button;
            var ray = CreateInteractor<MockRayInteractor>();
            yield return null;

            int clickCount = 0;
            interactable.OnClicked.AddListener(() => clickCount++);

            // Simulate click (SelectEnter + SelectExit with TriggerOnRelease = true)
            SelectEnter(ray, interactable);
            Assert.AreEqual(0, clickCount);
            SelectExit(ray, interactable);

            Assert.AreEqual(1, clickCount, "OnClicked should fire on select release.");
            Assert.IsFalse(interactable.IsToggled.Active, "Button mode should not alter IsToggled.");
        }

        [UnityTest]
        public IEnumerator StatefulInteractable_ToggleMode_ToggleInverts()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            interactable.ToggleMode = StatefulInteractable.ToggleType.Toggle;
            var ray = CreateInteractor<MockRayInteractor>();
            yield return null;

            int clickCount = 0;
            interactable.OnClicked.AddListener(() => clickCount++);

            // Click 1 -> Toggled on
            SelectEnter(ray, interactable);
            SelectExit(ray, interactable);
            Assert.AreEqual(1, clickCount);
            Assert.IsTrue(interactable.IsToggled.Active, "Interactable should be toggled on.");

            // Click 2 -> Toggled off
            SelectEnter(ray, interactable);
            SelectExit(ray, interactable);
            Assert.AreEqual(2, clickCount);
            Assert.IsFalse(interactable.IsToggled.Active, "Interactable should be toggled off.");
        }

        [UnityTest]
        public IEnumerator StatefulInteractable_ToggleMode_OneWayToggleStaysTrue()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            interactable.ToggleMode = StatefulInteractable.ToggleType.OneWayToggle;
            var ray = CreateInteractor<MockRayInteractor>();
            yield return null;

            // Click 1 -> Toggled on
            SelectEnter(ray, interactable);
            SelectExit(ray, interactable);
            Assert.IsTrue(interactable.IsToggled.Active);

            // Click 2 -> Remains on
            SelectEnter(ray, interactable);
            SelectExit(ray, interactable);
            Assert.IsTrue(interactable.IsToggled.Active, "OneWayToggle should remain true on subsequent clicks.");
        }

        [UnityTest]
        public IEnumerator StatefulInteractable_ForceSetToggled_FiresEventsConditionally()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            yield return null;

            int enterCount = 0;
            interactable.IsToggled.OnEntered.AddListener((_) => enterCount++);

            // ForceSetToggled(true, fireEvents: false) -> state changes, no event
            interactable.ForceSetToggled(true, fireEvents: false);
            Assert.IsTrue(interactable.IsToggled.Active);
            Assert.AreEqual(0, enterCount, "fireEvents=false should not raise OnEntered.");

            // Reset
            interactable.ForceSetToggled(false, fireEvents: false);
            Assert.IsFalse(interactable.IsToggled.Active);
            Assert.AreEqual(0, enterCount);

            // ForceSetToggled(true, fireEvents: true) -> state changes, event fires
            interactable.ForceSetToggled(true, fireEvents: true);
            Assert.IsTrue(interactable.IsToggled.Active);
            Assert.AreEqual(1, enterCount, "fireEvents=true should raise OnEntered.");
        }

        [UnityTest]
        public IEnumerator StatefulInteractable_GetSelectionProgress_VariableSelect()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            var var1 = CreateInteractor<MockVariableSelectInteractor>("Var1");
            var var2 = CreateInteractor<MockVariableSelectInteractor>("Var2");
            var ray = CreateInteractor<MockRayInteractor>("Ray");
            yield return null;

            Assert.AreEqual(0f, interactable.GetSelectionProgress(), "Initially selection progress should be 0.");

            var1.SelectProgress = 0.4f;
            HoverEnter(var1, interactable);
            Assert.AreEqual(0.4f, interactable.GetSelectionProgress(), 1e-4f);

            var2.SelectProgress = 0.75f;
            HoverEnter(var2, interactable);
            Assert.AreEqual(0.75f, interactable.GetSelectionProgress(), 1e-4f, "Should return max progress among hovering interactors.");

            // When a non-variable interactor selects, progress becomes 1.0f
            SelectEnter(ray, interactable);
            Assert.AreEqual(1.0f, interactable.GetSelectionProgress(), 1e-4f, "Non-variable selecting interactor yields 1.0f progress.");

            SelectExit(ray, interactable);
            HoverExit(var2, interactable);
            Assert.AreEqual(0.4f, interactable.GetSelectionProgress(), 1e-4f);

            HoverExit(var1, interactable);
            Assert.AreEqual(0f, interactable.GetSelectionProgress(), 1e-4f);
        }

        [UnityTest]
        public IEnumerator StatefulInteractable_TriggerOnRelease_Timing()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            interactable.TriggerOnRelease = false;
            var ray = CreateInteractor<MockRayInteractor>();
            yield return null;

            int clickCount = 0;
            interactable.OnClicked.AddListener(() => clickCount++);

            // TriggerOnRelease = false -> OnClicked fires immediately upon SelectEnter
            SelectEnter(ray, interactable);
            Assert.AreEqual(1, clickCount, "TriggerOnRelease=false should fire OnClicked on enter.");
            SelectExit(ray, interactable);
            Assert.AreEqual(1, clickCount, "No additional click on exit.");
        }

        #endregion StatefulInteractable Tests

        #region Performance & Zero-GC Tests

        [UnityTest]
        public IEnumerator Performance_IsInteractorTypeValid_ZeroGC()
        {
            var interactable = interactableObject.AddComponent<MRTKBaseInteractable>();
            var ray = CreateInteractor<MockRayInteractor>();
            yield return null;

            // Warm up
            interactable.IsInteractorTypeValid(ray);

            // 1. Time measurement without disabled types
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                interactable.IsInteractorTypeValid(ray);
            }
            sw.Stop();
            double nsPerCall = (sw.Elapsed.TotalMilliseconds * 1000000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] MRTKBaseInteractable.IsInteractorTypeValid (empty disabled list): {nsPerCall:F1} ns/op across {BenchmarkIterations} iterations");

            // 2. Zero-GC Assertion without disabled types
            Assert.That(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    interactable.IsInteractorTypeValid(ray);
                }
            }, Is.Not.AllocatingGCMemory());

            // Add a disabled type and test again
            interactable.DisableInteractorType(new SystemInterfaceType(typeof(MockPokeInteractor)));

            // Warm up
            interactable.IsInteractorTypeValid(ray);

            // 1. Time measurement with disabled types
            sw.Restart();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                interactable.IsInteractorTypeValid(ray);
            }
            sw.Stop();
            nsPerCall = (sw.Elapsed.TotalMilliseconds * 1000000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] MRTKBaseInteractable.IsInteractorTypeValid (with disabled types): {nsPerCall:F1} ns/op across {BenchmarkIterations} iterations");

            // 2. Zero-GC Assertion with disabled types
            Assert.That(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    interactable.IsInteractorTypeValid(ray);
                }
            }, Is.Not.AllocatingGCMemory());
        }

        [UnityTest]
        public IEnumerator Performance_GetSelectionProgress_ZeroGC()
        {
            var interactable = interactableObject.AddComponent<StatefulInteractable>();
            var var1 = CreateInteractor<MockVariableSelectInteractor>("Var1");
            var var2 = CreateInteractor<MockVariableSelectInteractor>("Var2");
            yield return null;

            HoverEnter(var1, interactable);
            HoverEnter(var2, interactable);

            // Warm up
            interactable.GetSelectionProgress();

            // 1. Time measurement
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < BenchmarkIterations; i++)
            {
                interactable.GetSelectionProgress();
            }
            sw.Stop();
            double nsPerCall = (sw.Elapsed.TotalMilliseconds * 1000000.0) / BenchmarkIterations;
            Debug.Log($"[Perf] StatefulInteractable.GetSelectionProgress: {nsPerCall:F1} ns/op across {BenchmarkIterations} iterations");

            // 2. Zero-GC Assertion
            Assert.That(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    interactable.GetSelectionProgress();
                }
            }, Is.Not.AllocatingGCMemory());

            HoverExit(var2, interactable);
            HoverExit(var1, interactable);
        }

        #endregion Performance & Zero-GC Tests
    }
}
#pragma warning restore CS1591

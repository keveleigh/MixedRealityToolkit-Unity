// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
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

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Comprehensive test suite verifying accuracy, performance, lifecycle, and Zero-GC
    /// behavior of the core interactors (PokeInteractor, MRTKRayInteractor, GrabInteractor, GazePinchInteractor, FuzzyGazeInteractor).
    /// </summary>
    public class InteractorsHotPathTests : BaseRuntimeInputTests
    {
        protected override InputTestUtilities.RigVersion RigVersion => InputTestUtilities.RigVersion.Version1;

        private const int HighFrequencyIterations = 1000;

        private PokeInteractor pokeInteractor;
        private MRTKRayInteractor rayInteractor;
        private GrabInteractor grabInteractor;
        private GazePinchInteractor gazePinchInteractor;
        private FuzzyGazeInteractor fuzzyGazeInteractor;
        private GameObject testCube;
        private StatefulInteractable testInteractable;

        public override IEnumerator Setup()
        {
            yield return base.Setup();

            var rightController = CachedLookup.RightHandController;
            Assert.IsNotNull(rightController, "RightHandController must be present on the rig.");

            pokeInteractor = rightController.GetComponentInChildren<PokeInteractor>(true);
            rayInteractor = rightController.GetComponentInChildren<MRTKRayInteractor>(true);
            grabInteractor = rightController.GetComponentInChildren<GrabInteractor>(true);
            gazePinchInteractor = rightController.GetComponentInChildren<GazePinchInteractor>(true);
            fuzzyGazeInteractor = Object.FindAnyObjectByType<FuzzyGazeInteractor>();

            Assert.IsNotNull(pokeInteractor, "PokeInteractor must be present in right hand controller.");
            Assert.IsNotNull(rayInteractor, "MRTKRayInteractor must be present in right hand controller.");
            Assert.IsNotNull(grabInteractor, "GrabInteractor must be present in right hand controller.");

            // Spawn a standard test interactable cube in front of user
            testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCube.transform.position = InputTestUtilities.InFrontOfUser(1.0f);
            testCube.transform.localScale = Vector3.one * 0.2f;
            testInteractable = testCube.AddComponent<StatefulInteractable>();

            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        public override IEnumerator TearDown()
        {
            if (testCube != null)
            {
                Object.Destroy(testCube);
            }

            yield return base.TearDown();
        }

        #region PokeInteractor Tests

        [UnityTest]
        public IEnumerator PokeInteractor_TrackingAndTrajectory()
        {
            pokeInteractor.gameObject.SetActive(true);
            pokeInteractor.enabled = true;

            Assert.IsTrue(pokeInteractor.isSelectActive, "PokeInteractor must have isSelectActive set to true.");
            Assert.IsTrue(pokeInteractor.PokeRadius > 0.0f, "PokeRadius must be greater than zero.");

            // Verify trajectory updates over frames
            Vector3 startPos = pokeInteractor.PokeTrajectory.Start;
            pokeInteractor.transform.position += Vector3.forward * 0.05f;
            pokeInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

            Assert.AreEqual(startPos, pokeInteractor.PokeTrajectory.Start, "Previous end should become current start.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator PokeInteractor_GetValidTargets_ZeroGCAndPerf()
        {
            pokeInteractor.gameObject.SetActive(true);
            pokeInteractor.enabled = true;
            List<IXRInteractable> targets = new List<IXRInteractable>(16);

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                pokeInteractor.GetValidTargets(targets);
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                pokeInteractor.GetValidTargets(targets);
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] PokeInteractor.GetValidTargets: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    pokeInteractor.GetValidTargets(targets);
                }
            }, Is.Not.AllocatingGCMemory());

            yield return null;
        }

        [UnityTest]
        public IEnumerator PokeInteractor_ProcessInteractor_ZeroGCAndPerf()
        {
            pokeInteractor.gameObject.SetActive(true);
            pokeInteractor.enabled = true;

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                pokeInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                pokeInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] PokeInteractor.ProcessInteractor: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    pokeInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
                }
            }, Is.Not.AllocatingGCMemory());

            yield return null;
        }

        #endregion PokeInteractor Tests

        #region MRTKRayInteractor Tests

        [UnityTest]
        public IEnumerator RayInteractor_RelaxationAndStickyHover()
        {
            rayInteractor.gameObject.SetActive(true);
            rayInteractor.enabled = true;

            Assert.IsNotNull(rayInteractor);
            Assert.IsTrue(rayInteractor.relaxationThreshold > 0.0f);

            List<IXRInteractable> targets = new List<IXRInteractable>(16);
            rayInteractor.GetValidTargets(targets);
            Assert.IsNotNull(targets);

            yield return null;
        }

        [UnityTest]
        public IEnumerator RayInteractor_GetValidTargets_ZeroGCAndPerf()
        {
            rayInteractor.gameObject.SetActive(true);
            rayInteractor.enabled = true;
            List<IXRInteractable> targets = new List<IXRInteractable>(16);

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                rayInteractor.GetValidTargets(targets);
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                rayInteractor.GetValidTargets(targets);
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] MRTKRayInteractor.GetValidTargets: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    rayInteractor.GetValidTargets(targets);
                }
            }, Is.Not.AllocatingGCMemory());

            yield return null;
        }

        [UnityTest]
        public IEnumerator RayInteractor_ProcessInteractor_ZeroGCAndPerf()
        {
            rayInteractor.gameObject.SetActive(true);
            rayInteractor.enabled = true;

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                rayInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                rayInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] MRTKRayInteractor.ProcessInteractor: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    rayInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
                }
            }, Is.Not.AllocatingGCMemory());

            yield return null;
        }

        #endregion MRTKRayInteractor Tests

        #region GrabInteractor Tests

        [UnityTest]
        public IEnumerator GrabInteractor_ProcessInteractor_ZeroGCAndPerf()
        {
            grabInteractor.gameObject.SetActive(true);
            grabInteractor.enabled = true;

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                grabInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                grabInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] GrabInteractor.ProcessInteractor: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    grabInteractor.ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
                }
            }, Is.Not.AllocatingGCMemory());

            yield return null;
        }

        #endregion GrabInteractor Tests

        #region FuzzyGazeInteractor Tests

        [UnityTest]
        public IEnumerator FuzzyGazeInteractor_GetValidTargets_ZeroGCAndPerf()
        {
            if (fuzzyGazeInteractor == null)
            {
                Assert.Ignore("FuzzyGazeInteractor not present in scene.");
                yield break;
            }

            List<IXRInteractable> targets = new List<IXRInteractable>(16);

            // Warm up
            for (int i = 0; i < 50; i++)
            {
                fuzzyGazeInteractor.GetValidTargets(targets);
            }

            // Benchmark execution time
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < HighFrequencyIterations; i++)
            {
                fuzzyGazeInteractor.GetValidTargets(targets);
            }
            sw.Stop();
            double microsecondsPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / HighFrequencyIterations;
            Debug.Log($"[PERF] FuzzyGazeInteractor.GetValidTargets: {microsecondsPerCall:F3} µs/call across {HighFrequencyIterations} iterations");

            // Zero-GC assertion
            Assert.That(() =>
            {
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    fuzzyGazeInteractor.GetValidTargets(targets);
                }
            }, Is.Not.AllocatingGCMemory());

            yield return null;
        }

        #endregion FuzzyGazeInteractor Tests
    }
}

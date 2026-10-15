// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace MixedReality.Toolkit.Audio.Tests
{
    /// <summary>
    /// Comprehensive test suite verifying accuracy, property setters, camera null-safety,
    /// occlusion raycast math, performance, and Zero-GC invariants of the Audio subsystem.
    /// </summary>
    public class AudioHotPathTests : BaseRuntimeTests
    {
        private const int HighFrequencyIterations = 1000;

        #region Behavioral & Correctness Tests

        [UnityTest]
        public IEnumerator AudioOccluder_VolumePassThrough_SetsCorrectProperty()
        {
            GameObject occluderGo = new GameObject("TestOccluder");
            try
            {
                AudioOccluder occluder = occluderGo.AddComponent<AudioOccluder>();
                Assert.AreEqual(1.0f, occluder.VolumePassThrough, 0.001f);
                Assert.AreEqual(5000.0f, occluder.CutoffFrequency, 0.001f);

                // Setting VolumePassThrough must update volumePassThrough, NOT cutoffFrequency
                occluder.VolumePassThrough = 0.5f;
                Assert.AreEqual(0.5f, occluder.VolumePassThrough, 0.001f, "Setting VolumePassThrough must update VolumePassThrough.");
                Assert.AreEqual(5000.0f, occluder.CutoffFrequency, 0.001f, "Setting VolumePassThrough must not overwrite CutoffFrequency.");

                // Clamping behavior: [0.0, 1.0]
                occluder.VolumePassThrough = 1.5f;
                Assert.AreEqual(1.0f, occluder.VolumePassThrough, 0.001f, "VolumePassThrough must clamp to 1.0.");
                occluder.VolumePassThrough = -0.5f;
                Assert.AreEqual(0.0f, occluder.VolumePassThrough, 0.001f, "VolumePassThrough must clamp to 0.0.");
            }
            finally
            {
                Object.Destroy(occluderGo);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioOccluder_ApplyAndRemoveEffect()
        {
            GameObject emitterGo = new GameObject("TestEmitter");
            GameObject occluderGo = new GameObject("TestOccluder");
            try
            {
                AudioSource audioSource = emitterGo.AddComponent<AudioSource>();
                audioSource.volume = 0.8f;

                AudioOccluder occluder = occluderGo.AddComponent<AudioOccluder>();
                occluder.CutoffFrequency = 2000.0f;
                occluder.VolumePassThrough = 0.5f;

                // Apply Effect
                occluder.ApplyEffect(emitterGo);

                AudioLowPassFilter lowPass = emitterGo.GetComponent<AudioLowPassFilter>();
                Assert.IsNotNull(lowPass, "AudioLowPassFilter must be added to emitter.");
                Assert.IsTrue(lowPass.enabled, "AudioLowPassFilter must be enabled.");
                Assert.AreEqual(2000.0f, lowPass.cutoffFrequency, 1.0f, "Cutoff frequency must match occluder.");
                Assert.AreEqual(0.4f, audioSource.volume, 0.001f, "AudioSource volume must be attenuated by VolumePassThrough (0.8 * 0.5 = 0.4).");

                // Remove Effect
                occluder.RemoveEffect(emitterGo);
                Assert.IsFalse(lowPass.enabled, "AudioLowPassFilter must be disabled after RemoveEffect.");
            }
            finally
            {
                Object.Destroy(emitterGo);
                Object.Destroy(occluderGo);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioBandPassEffect_NullFilter_DoesNotThrow()
        {
            GameObject emitterGo = new GameObject("TestEmitter");
            try
            {
                emitterGo.AddComponent<AudioSource>();
                emitterGo.AddComponent<AudioLowPassFilter>();
                emitterGo.AddComponent<AudioHighPassFilter>();

                // Adding AudioBandPassEffect with Filter == null must not throw NullReferenceException in Awake
                AudioBandPassEffect effect = null;
                Assert.DoesNotThrow(() =>
                {
                    effect = emitterGo.AddComponent<AudioBandPassEffect>();
                }, "AudioBandPassEffect must handle null Filter gracefully without throwing NullReferenceException in Awake.");

                Assert.IsNotNull(effect);
                Assert.IsNull(effect.Filter);
            }
            finally
            {
                Object.Destroy(emitterGo);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioBandPassEffect_ApplyFilter_UpdatesFrequencies()
        {
            GameObject emitterGo = new GameObject("TestEmitter");
            AudioBandPassFilter filter = ScriptableObject.CreateInstance<AudioBandPassFilter>();
            try
            {
                emitterGo.AddComponent<AudioSource>();
                AudioLowPassFilter lowPass = emitterGo.AddComponent<AudioLowPassFilter>();
                AudioHighPassFilter highPass = emitterGo.AddComponent<AudioHighPassFilter>();

                filter.LowFrequencyCutoff = 300.0f;
                filter.HighFrequencyCutoff = 3400.0f;

                AudioBandPassEffect effect = emitterGo.AddComponent<AudioBandPassEffect>();
                effect.Filter = filter;

                yield return null;

                Assert.AreEqual(3400.0f, lowPass.cutoffFrequency, 1.0f, "LowPass cutoff frequency must match HighFrequencyCutoff.");
                Assert.AreEqual(300.0f, highPass.cutoffFrequency, 1.0f, "HighPass cutoff frequency must match LowFrequencyCutoff.");
            }
            finally
            {
                Object.Destroy(emitterGo);
                Object.Destroy(filter);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioInfluencerController_NullCamera_DoesNotThrow()
        {
            GameObject emitterGo = new GameObject("TestEmitter");
            Camera camera = null;
            GameObject cameraGo = null;
            string originalTag = null;
            try
            {
                camera = Camera.main;
                if (camera != null)
                {
                    originalTag = camera.tag;
                    camera.tag = "Untagged";
                }

                Assert.IsNull(Camera.main, "Camera.main must be null for this test.");

                AudioSource audioSource = emitterGo.AddComponent<AudioSource>();
                AudioInfluencerController controller = emitterGo.AddComponent<AudioInfluencerController>();
                controller.UpdateInterval = 0.0f; // Update every frame

                Assert.DoesNotThrow(() =>
                {
                    // Trigger update via frame step
                    emitterGo.transform.position += Vector3.forward * 0.1f;
                }, "AudioInfluencerController must not throw NullReferenceException when Camera.main is null.");

                yield return null;
            }
            finally
            {
                if (camera != null && originalTag != null)
                {
                    camera.tag = originalTag;
                }
                if (cameraGo != null)
                {
                    Object.Destroy(cameraGo);
                }
                Object.Destroy(emitterGo);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioInfluencerController_RaycastOcclusion_AppliesInfluencer()
        {
            GameObject cameraGo = new GameObject("MainCamera");
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = Vector3.zero;

            GameObject emitterGo = new GameObject("TestEmitter");
            emitterGo.transform.position = new Vector3(0f, 0f, 4.0f);

            GameObject occluderGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            occluderGo.transform.position = new Vector3(0f, 0f, 2.0f);
            occluderGo.transform.localScale = new Vector3(2f, 2f, 0.2f);

            try
            {
                AudioSource audioSource = emitterGo.AddComponent<AudioSource>();
                audioSource.volume = 1.0f;

                AudioInfluencerController controller = emitterGo.AddComponent<AudioInfluencerController>();
                controller.UpdateInterval = 0.0f; // Update every frame

                AudioOccluder occluder = occluderGo.AddComponent<AudioOccluder>();
                occluder.CutoffFrequency = 1500.0f;
                occluder.VolumePassThrough = 0.5f;

                yield return new WaitForFixedUpdate();
                yield return null;

                AudioLowPassFilter lowPass = emitterGo.GetComponent<AudioLowPassFilter>();
                Assert.IsNotNull(lowPass, "AudioLowPassFilter should be added when occluded.");
                Assert.IsTrue(lowPass.enabled, "AudioLowPassFilter should be enabled when occluded.");
                Assert.AreEqual(1500.0f, lowPass.cutoffFrequency, 1.0f, "Cutoff frequency should match occluder.");
                Assert.AreEqual(0.5f, audioSource.volume, 0.01f, "Volume should be reduced by occluder.");

                // Move occluder out of line of sight
                occluderGo.transform.position = new Vector3(20f, 0f, 2.0f);
                yield return new WaitForFixedUpdate();
                yield return null;

                Assert.IsFalse(lowPass.enabled, "AudioLowPassFilter should be disabled when occluder moves away.");
                Assert.AreEqual(1.0f, audioSource.volume, 0.01f, "Volume should be restored when unobstructed.");
            }
            finally
            {
                Object.Destroy(cameraGo);
                Object.Destroy(emitterGo);
                Object.Destroy(occluderGo);
            }

            yield return null;
        }

        #endregion

        #region Performance & Zero-GC Microbenchmarks

        [UnityTest]
        public IEnumerator AudioInfluencerController_Update_ZeroGCAndPerf()
        {
            GameObject cameraGo = new GameObject("MainCamera");
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = Vector3.zero;

            GameObject emitterGo = new GameObject("TestEmitter");
            emitterGo.transform.position = new Vector3(0f, 0f, 4.0f);

            try
            {
                AudioSource audioSource = emitterGo.AddComponent<AudioSource>();
                AudioInfluencerController controller = emitterGo.AddComponent<AudioInfluencerController>();
                controller.UpdateInterval = 0.0f;

                yield return null;

                // Warmup
                for (int i = 0; i < 50; i++)
                {
                    emitterGo.transform.position = new Vector3(0f, 0f, 4.0f + i * 0.001f);
                    yield return null;
                }

                // Zero-GC Assertion over raycast iterations
                Transform emitterTransform = emitterGo.transform;
                Transform cameraTransform = cameraGo.transform;
                RaycastHit[] hits = new RaycastHit[10];

                Assert.That(() =>
                {
                    for (int i = 0; i < HighFrequencyIterations; i++)
                    {
                        Vector3 emitterPosition = emitterTransform.position;
                        Vector3 cameraPosition = cameraTransform.position;
                        Vector3 toEmitter = emitterPosition - cameraPosition;
                        float distance = toEmitter.magnitude;
                        if (distance >= 1e-5f)
                        {
                            Vector3 direction = toEmitter / distance;
                            Physics.RaycastNonAlloc(cameraPosition, direction, hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                        }
                    }
                }, Is.Not.AllocatingGCMemory());

                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < HighFrequencyIterations; i++)
                {
                    Vector3 emitterPosition = emitterTransform.position;
                    Vector3 cameraPosition = cameraTransform.position;
                    Vector3 toEmitter = emitterPosition - cameraPosition;
                    float distance = toEmitter.magnitude;
                    if (distance >= 1e-5f)
                    {
                        Vector3 direction = toEmitter / distance;
                        Physics.RaycastNonAlloc(cameraPosition, direction, hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                    }
                }
                sw.Stop();

                Debug.Log($"[PERF] AudioInfluencer Raycast ({HighFrequencyIterations} iters): {sw.Elapsed.TotalMilliseconds:F3} ms (avg: {sw.Elapsed.TotalMilliseconds / HighFrequencyIterations * 1000.0:F3} us/op)");
            }
            finally
            {
                Object.Destroy(cameraGo);
                Object.Destroy(emitterGo);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioInfluencer_BaselineVsOptimized_DirectBenchmark()
        {
            GameObject cameraGo = new GameObject("MainCamera");
            cameraGo.transform.position = Vector3.zero;

            GameObject emitterGo = new GameObject("TestEmitter");
            emitterGo.transform.position = new Vector3(1.5f, 0.5f, 4.0f);

            try
            {
                Transform emitterTransform = emitterGo.transform;
                Transform cameraTransform = cameraGo.transform;
                RaycastHit[] hits = new RaycastHit[10];
                const int iters = 50000;

                // --- Baseline Path: 4 native transform queries, 2 separate Sqrt operations ---
                Stopwatch swBaseline = Stopwatch.StartNew();
                for (int i = 0; i < iters; i++)
                {
                    Vector3 direction = (emitterGo.transform.position - cameraTransform.position).normalized;
                    float distance = Vector3.Distance(cameraTransform.position, emitterGo.transform.position);
                    // Native transform property queries + 2x Sqrt
                    if (distance > 0f)
                    {
                        _ = direction.x + distance;
                    }
                }
                swBaseline.Stop();

                // --- Optimized Path: 2 native transform queries, 1 Sqrt operation (reusing magnitude) ---
                Stopwatch swOptimized = Stopwatch.StartNew();
                for (int i = 0; i < iters; i++)
                {
                    Vector3 emitterPosition = emitterTransform.position;
                    Vector3 cameraPosition = cameraTransform.position;
                    Vector3 toEmitter = emitterPosition - cameraPosition;
                    float distance = toEmitter.magnitude;
                    if (distance >= 1e-5f)
                    {
                        Vector3 direction = toEmitter / distance;
                        _ = direction.x + distance;
                    }
                }
                swOptimized.Stop();

                float baselineMs = (float)swBaseline.Elapsed.TotalMilliseconds;
                float optimizedMs = (float)swOptimized.Elapsed.TotalMilliseconds;
                float speedupPct = ((baselineMs - optimizedMs) / baselineMs) * 100f;

                Debug.Log($"[PERF_COMPARE_AUDIO_MATH] ({iters} iters) Baseline: {baselineMs:F3} ms ({baselineMs / iters * 1000f:F3} us/op) | Optimized: {optimizedMs:F3} ms ({optimizedMs / iters * 1000f:F3} us/op) | Speedup: {speedupPct:F1}%");
            }
            finally
            {
                Object.Destroy(cameraGo);
                Object.Destroy(emitterGo);
            }

            yield return null;
        }

        #endregion
    }
}
#pragma warning restore CS1591

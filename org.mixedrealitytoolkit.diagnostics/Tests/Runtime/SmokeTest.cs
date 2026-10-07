// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using MixedReality.Toolkit.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.Performance.Tests
{
    internal class SmokeTest
    {
        [UnityTest]
        public IEnumerator PerformancePackageTest()
        {
            yield return null;
        }

        [UnityTest]
        public IEnumerator SimpleProfilerFollowsMainCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();

            GameObject profilerObject = new GameObject("Simple Profiler");
            SimpleProfiler profiler = profilerObject.AddComponent<SimpleProfiler>();
            profiler.enabled = false;
            profiler.FollowCamera = true;
            profilerObject.SendMessage("LateUpdate");

            Assert.That(Vector3.Distance(
                profilerObject.transform.position,
                camera.transform.position + (camera.transform.forward * 0.45f)), Is.LessThan(0.0001f));
            Assert.That(Vector3.Angle(profilerObject.transform.forward, camera.transform.forward), Is.LessThan(0.0001f));

            Object.Destroy(profilerObject);
            Object.Destroy(cameraObject);
            yield return null;
        }
    }
}
#pragma warning restore CS1591

// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Diagnostics
{
    /// <summary>
    /// Script to display performance statistics as simple text objects.
    /// </summary>
    [AddComponentMenu("MRTK/Diagnostics/Simple Profiler")]
    public class SimpleProfiler : MonoBehaviour
    {
        #region Frame properties

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the frame rate.")]
        private TMP_Text frameRate = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the application frame rate.
        /// </summary>
        public TMP_Text FrameRate
        {
            get => frameRate;
            set => frameRate = value;
        }

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the display refresh rate.")]
        private TMP_Text refreshRate = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the display refresh rate.
        /// </summary>
        public TMP_Text RefreshRate
        {
            get => refreshRate;
            set => refreshRate = value;
        }

        #endregion Frame properties

        #region Memory properties

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the RAM addressable by the application.")]
        private TMP_Text ramLimit = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the RAM addressable
        /// by the application.
        /// </summary>
        public TMP_Text RamLimit
        {
            get => ramLimit;
            set => ramLimit = value;
        }

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the allocated RAM.")]
        private TMP_Text allocatedRam = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the allocated RAM.
        /// </summary>
        public TMP_Text AllocatedRam
        {
            get => allocatedRam;
            set => allocatedRam = value;
        }

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the peak allocated RAM.")]
        private TMP_Text peakAllocatedRam = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the peak allocated RAM.
        /// </summary>
        public TMP_Text PeakAllocatedRam
        {
            get => peakAllocatedRam;
            set => peakAllocatedRam = value;
        }

        #endregion Memory properties

        #region Positioning properties

        [SerializeField]
        [Tooltip("Whether the profiler should remain near the main camera's field of view.")]
        private bool followCamera = false;

        /// <summary>
        /// Whether the profiler should remain near the main camera's field of view.
        /// </summary>
        public bool FollowCamera
        {
            get => followCamera;
            set
            {
                followCamera = value;
                recenterNextUpdate = value;
            }
        }

        [SerializeField]
        [Min(0.0f)]
        [Tooltip("The distance at which the profiler is placed when it needs to be recentered.")]
        private float defaultDistance = 0.45f;

        [SerializeField]
        [Min(0.0f)]
        [Tooltip("The minimum distance the profiler can be from the main camera before it is recentered.")]
        private float minDistance = 0.4f;

        [SerializeField]
        [Min(0.0f)]
        [Tooltip("The maximum distance the profiler can be from the main camera before it is recentered.")]
        private float maxDistance = 0.5f;

        [SerializeField]
        [Range(0.0f, 180.0f)]
        [Tooltip("The maximum horizontal angle from the main camera's forward direction before the profiler is recentered.")]
        private float maxViewHorizontalDegrees = 30.0f;

        [SerializeField]
        [Range(0.0f, 180.0f)]
        [Tooltip("The maximum vertical angle from the main camera's forward direction before the profiler is recentered.")]
        private float maxViewVerticalDegrees = 80.0f;

        [SerializeField]
        [Min(0.0f)]
        [Tooltip("The approximate time, in seconds, used to smooth the profiler toward its recentered position.")]
        private float moveLerpTime = 1.0f;

        #endregion Positioning properties

        private const string noData = "--";
        private bool recenterNextUpdate = true;

        private void OnEnable()
        {
            recenterNextUpdate = followCamera;
        }

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary> 
        private void Start()
        {
            RamLimit.text = noData;
            AllocatedRam.text = noData;
            PeakAllocatedRam.text = noData;

            FrameRate.text = noData;
            RefreshRate.text = noData;
        }

        private void LateUpdate()
        {
            if (!followCamera || Camera.main == null)
            {
                return;
            }

            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraToProfiler = transform.position - cameraTransform.position;
            float distance = cameraToProfiler.magnitude;

            if (!recenterNextUpdate &&
                distance >= minDistance &&
                distance <= maxDistance &&
                IsInsideView(cameraTransform, cameraToProfiler))
            {
                return;
            }

            float recenterDistance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);
            Vector3 goalPosition = cameraTransform.position + (cameraTransform.forward * recenterDistance);
            Quaternion goalRotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);

            if (recenterNextUpdate || moveLerpTime <= 0.0f)
            {
                transform.SetPositionAndRotation(goalPosition, goalRotation);
            }
            else
            {
                float lerpAmount = 1.0f - Mathf.Exp(-Time.unscaledDeltaTime / moveLerpTime);
                transform.SetPositionAndRotation(
                    Vector3.Lerp(transform.position, goalPosition, lerpAmount),
                    Quaternion.Slerp(transform.rotation, goalRotation, lerpAmount));
            }

            recenterNextUpdate = false;
        }

        private bool IsInsideView(Transform cameraTransform, Vector3 cameraToProfiler)
        {
            if (cameraToProfiler.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            Vector3 localDirection = cameraTransform.InverseTransformDirection(cameraToProfiler.normalized);
            if (localDirection.z <= 0.0f)
            {
                return false;
            }

            float horizontalAngle = Mathf.Abs(Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg);
            float verticalAngle = Mathf.Abs(
                Mathf.Atan2(localDirection.y, new Vector2(localDirection.x, localDirection.z).magnitude) *
                Mathf.Rad2Deg);

            return horizontalAngle <= maxViewHorizontalDegrees &&
                verticalAngle <= maxViewVerticalDegrees;
        }

        /// <summary>
        /// A Unity event function that is called at an framerate independent frequency, and is only called if this object is enabled.
        /// </summary>
        private void FixedUpdate()
        {
            if (PerformanceStatsHelpers.Subsystem != null)
            {
                ulong limitBytes = PerformanceStatsHelpers.Subsystem.RamLimit;
                float limitMegabytes = MathUtilities.BytesToMegabytes(limitBytes);
                RamLimit.text = $"{limitBytes} ({limitMegabytes:0.00})";

                ulong allocatedBytes = PerformanceStatsHelpers.Subsystem.AllocatedRam;
                float allocatedMegabytes = MathUtilities.BytesToMegabytes(allocatedBytes);
                AllocatedRam.text = $"{allocatedBytes} ({allocatedMegabytes:0.00})";

                ulong peakBytes = PerformanceStatsHelpers.Subsystem.PeakAllocatedRam;
                float peakMegabytes = MathUtilities.BytesToMegabytes(peakBytes);
                PeakAllocatedRam.text = $"{peakBytes} ({peakMegabytes:0.00})";

                FrameRate.text = PerformanceStatsHelpers.Subsystem.FrameRate.ToString("0.00");
            }

            float refRate = -1f;
            bool? haveRefreshRate = XRSubsystemHelpers.DisplaySubsystem?.TryGetDisplayRefreshRate(out refRate);
            RefreshRate.text = (haveRefreshRate.HasValue && haveRefreshRate.Value) ?
                refRate.ToString("0.00") : noData;
        }
    }
}

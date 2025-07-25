﻿// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

using SliderEvent = UnityEngine.Events.UnityEvent<MixedReality.Toolkit.UX.SliderEventData>;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// A slider that can be moved by grabbing / pinching a slider thumb
    /// </summary>
    [HelpURL("https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/sliders")]
    [AddComponentMenu("MRTK/UX/Slider")]
    public class Slider : StatefulInteractable, ISnapInteractable
    {
        #region Serialized Fields and Public Properties

        [Header("Slider Options")]

        [SerializeField]
        [Tooltip("Whether or not this slider is manipulatable by IPokeInteractors.\nIf true, IGrabInteractors will have no effect.")]
        // We want to remove this and rely solely on DisabledInteractorTypes in the future
        private bool isTouchable;

        /// <summary>
        /// Whether or not this slider is manipulatable by IPokeInteractors.
        /// If true, IGrabInteractors will have no effect.
        /// </summary>
        /// <remarks> We want to remove this and rely solely on DisabledInteractorTypes in the future for disabling touch interactions </remarks>
        public bool IsTouchable
        {
            get => isTouchable;
            set => isTouchable = value;
        }

        [SerializeField]
        [Tooltip("Whether or not this slider snaps to the designated position on the slider.\nGrab interactions will not snap, regardless of the value of this property.")]
        private bool snapToPosition;

        private bool snapToPositionErrorLogged = false;

        /// <summary>
        /// Determines whether or not this slider snaps to the designated position on the slider.
        /// Grab interactions will not snap, regardless of the value of this property.
        /// </summary>
        public bool SnapToPosition
        {
            get => snapToPosition && trackCollider != null;
            set
            {
                if (trackCollider != null)
                {
                    snapToPosition = value;
                    trackCollider.enabled = value;
                }
                else
                {
                    // only log an error if we're trying to set snapToPosition to true
                    if (value && !snapToPositionErrorLogged)
                    {
                        snapToPositionErrorLogged = true;
                        Debug.LogError("SnapToPosition will not be functional so long as a track collider is not associated with the slider");
                    }
                }
            }
        }

        #region ISnapInteractable

        [SerializeField]
        [Tooltip("Transform of the handle affordance")]
        private Transform handleTransform;

        /// <inheritdoc/>
        public Transform HandleTransform => handleTransform;

        #endregion ISnapInteractable

        [SerializeField]
        [Tooltip("Collider that covers the entire slider track area.")]
        private Collider trackCollider = null;

        /// <summary>
        /// Collider that covers the entire slider track area.
        /// </summary>
        public Collider TrackCollider
        {
            get => trackCollider;
            set => trackCollider = value;
        }

        [SerializeField]
        [Tooltip("The minimum value that the slider can take on")]
        private float minValue = 0.0f;

        /// <summary>
        /// The minimum value that the slider can take on
        /// </summary>
        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = Mathf.Min(value, maxValue);
            }
        }

        [SerializeField]
        [Tooltip("The maximum value that the slider can take on")]
        private float maxValue = 1.0f;

        /// <summary>
        /// The maximum value that the slider can take on
        /// </summary>
        public float MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = Mathf.Max(minValue, value);
            }
        }

        [VariableRange("minValue", "maxValue")]
        [FormerlySerializedAs("sliderValue")]
        [SerializeField]
        private float value = 0.5f;

        /// <summary>
        /// The current value of the slider.
        /// </summary>
        [Obsolete("Use Value instead")]
        public float SliderValue => Value;

        /// <summary>
        /// The current value of the slider
        /// </summary>
        public float Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    value = Mathf.Clamp(value, minValue, maxValue);
                    var oldSliderValue = this.value;
                    this.value = value;
                    OnValueUpdated.Invoke(new SliderEventData(oldSliderValue, value));
                }
            }
        }

        /// <summary>
        /// The slider's normalized value calculated using the <see cref="MinValue"/> and <see cref="MaxValue"/> properties.
        /// </summary>
        public float NormalizedValue => (MaxValue - MinValue) != 0 ? (value - MinValue) / (MaxValue - MinValue) : 0;

        [SerializeField]
        [Tooltip("Controls whether this slider is increments in steps or continuously.")]
        private bool useSliderStepDivisions;

        /// <summary>
        /// Property accessor of useSliderStepDivisions, it determines whether the slider steps according to subdivisions or continuously.
        /// </summary>
        public bool UseSliderStepDivisions
        {
            get => useSliderStepDivisions;
            set => useSliderStepDivisions = value;
        }

        [SerializeField]
        [Min(1)]
        [Tooltip("Number of subdivisions the slider is split into.")]
        private int sliderStepDivisions = 1;

        /// <summary>
        /// Property accessor of sliderStepDivisions, it holds the number of subdivisions the slider is split into.
        /// </summary>
        public int SliderStepDivisions
        {
            get => sliderStepDivisions;
            set => sliderStepDivisions = value;
        }

        [Header("Layout")]
        [SerializeField]
        [Tooltip("A transform marking the starting point of the slider track.")]
        private Transform sliderStart;

        /// <summary>
        /// A transform marking the starting point of the slider track.
        /// </summary>
        public Transform SliderStart
        {
            get => sliderStart;
            set => sliderStart = value;
        }

        [SerializeField]
        [Tooltip("A transform marking the end point of the slider track.")]
        private Transform sliderEnd;

        /// <summary>
        /// A transform marking the end point of the slider track.
        /// </summary>
        public Transform SliderEnd
        {
            get => sliderEnd;
            set => sliderEnd = value;
        }

        /// <summary>
        /// Returns the vector from the slider start to end positions
        /// </summary>
        public Vector3 SliderTrackDirection => SliderEnd.position - SliderStart.position;

        #endregion

        #region Event Handlers

        [Header("Slider Events")]

        [SerializeField]
        [FormerlySerializedAs("OnValueUpdated")]
        [Tooltip("A Unity event that is invoked when the slider's value has changed.")]
        private SliderEvent onValueUpdated = new SliderEvent();

        /// <summary>
        /// A Unity event that is invoked when <see cref="Value"/> changes.
        /// </summary>
        public SliderEvent OnValueUpdated => onValueUpdated;
        #endregion

        #region Private Fields

        /// <summary>
        /// Private member used to adjust slider values
        /// </summary>
        private float SliderStepVal => (MaxValue - MinValue) / sliderStepDivisions;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Float value that holds the starting value of the slider.
        /// </summary>
        protected float StartSliderValue { get; private set; }

        /// <summary>
        /// The interaction point at the beginning of an interaction.
        /// </summary>
        protected Vector3 StartInteractionPoint { get; private set; }

        #endregion

        #region Unity methods

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            ApplyRequiredSettings();
        }

        /// <summary>
        /// A Unity event function that is called when the script should reset it's default values
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            ApplyRequiredSettings();
        }

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary> 
        protected virtual void Start()
        {
            // Turn on/off colliders at Start() to avoid bugs with
            // colliders not calling OnTriggerExit on interactors
            SnapToPosition = snapToPosition;

            // If no handle transform set, use the transform of the primary
            // collider.
            if (handleTransform == null)
            {
                Debug.LogWarning("Slider " + name + " has no handle transform. Please fix! Using primary collider instead.");
                handleTransform = colliders[0].transform;
            }

            // Ensure the interactable knows about our track collider.
            if (!colliders.Contains(trackCollider))
            {
                colliders.Add(trackCollider);
            }

            if (useSliderStepDivisions)
            {
                InitializeStepDivisions();
            }

            OnValueUpdated.Invoke(new SliderEventData(value, value));
        }

        /// <summary>
        /// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            ApplyRequiredSettings();

            // Ensure that the proper constraints are applied to the possible values of the slider
            MinValue = minValue;
            MaxValue = maxValue;
            Value = value;
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Invoked on <see cref="Start"/>, <see cref="Awake"/>, and <see cref="Reset"/> to apply required 
        /// settings to this <see cref="Slider"/> instance.
        /// </summary>
        /// <remarks>
        /// This is used to force the interactable into single select mode.
        /// </remarks>
        protected virtual void ApplyRequiredSettings()
        {
            // Sliders use InteractableSelectMode.Single to ignore
            // incoming interactors after a first/valid interactor has
            // been acquired.
            selectMode = InteractableSelectMode.Single;
        }
        #endregion Protected Methods

        #region Private Methods
        /// <summary> 
        /// Private method used to adjust initial slider value to stepwise values
        /// </summary>
        private void InitializeStepDivisions()
        {
            Value = SnapSliderToStepPositions(Value);
        }

        private float SnapSliderToStepPositions(float value)
        {
            var stepCount = value / SliderStepVal;
            var snappedValue = SliderStepVal * Mathf.RoundToInt(stepCount);
            Mathf.Clamp(snappedValue, 0f, 1.0f);
            return snappedValue;
        }

        private void UpdateSliderValue()
        {
            Vector3 interactionPoint = interactorsSelecting[0].GetAttachTransform(this).position;
            Vector3 interactorDelta = interactionPoint - StartInteractionPoint;

            var handDelta = Vector3.Dot(SliderTrackDirection.normalized, interactorDelta);

            var normalizedStartValue = (StartSliderValue - MinValue) / (MaxValue - MinValue);
            float normalizedValue = Mathf.Clamp(normalizedStartValue + handDelta / SliderTrackDirection.magnitude, 0f, 1.0f);

            var unsnappedValue = normalizedValue * (MaxValue - MinValue) + MinValue;
            Value = useSliderStepDivisions ? SnapSliderToStepPositions(unsnappedValue) : unsnappedValue;
        }

        #endregion

        #region XRI methods

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            // Snap to position by setting the startPosition
            // to the slider start, and start value to MinValue.
            // However, don't snap when using grabs.
            if (snapToPosition && !(args.interactorObject is IGrabInteractor))
            {
                StartInteractionPoint = SliderStart.position;
                StartSliderValue = MinValue;
            }
            else
            {
                StartInteractionPoint = args.interactorObject.GetAttachTransform(this).position;
                StartSliderValue = value;
            }
        }

        /// <inheritdoc />
        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            // Only allow the first interactor selecting the slider to be able to control it.
            if (isSelected)
            {
                return base.IsSelectableBy(interactor) && interactor == interactorsSelecting[0];
            }

            // Only allow grabbing if the slider is not touchable.
            if (interactor is IGrabInteractor)
            {
                return !isTouchable && base.IsSelectableBy(interactor);
            }

            if (interactor is IPokeInteractor)
            {
                return isTouchable && base.IsSelectableBy(interactor);
            }

            return base.IsSelectableBy(interactor);
        }

        private static readonly ProfilerMarker ProcessInteractablePerfMarker =
            new ProfilerMarker("[MRTK] PinchSlider.ProcessInteractable");

        ///<inheritdoc />
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            using (ProcessInteractablePerfMarker.Auto())
            {
                base.ProcessInteractable(updatePhase);

                if (trackCollider != null)
                {
                    // Make sure our track collider is enabled if
                    // we have snapToPosition enabled.
                    if (snapToPosition != trackCollider.enabled)
                    {
                        trackCollider.enabled = snapToPosition;
                    }
                }

                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
                {
                    UpdateSliderValue();
                }
            }
        }

        #endregion
    }
}

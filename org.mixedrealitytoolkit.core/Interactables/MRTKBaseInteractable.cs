// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Extended version of XRBaseInteractable to enable multi-hand interactions.
    /// </summary>
    [AddComponentMenu("MRTK/Core/MRTK Base Interactable")]
    public class MRTKBaseInteractable : XRBaseInteractable
    {
        #region Gaze

        private List<IGazeInteractor> hoveringGazeInteractors;

        /// <summary>
        /// (Read Only) The list of <see cref="IGazeInteractor"/> components currently gazing this object.
        /// </summary>
        public List<IGazeInteractor> HoveringGazeInteractors => hoveringGazeInteractors ??= new List<IGazeInteractor>();

        #endregion Gaze

        #region GazePinch

        private List<IGazePinchInteractor> hoveringGazePinchInteractors;

        /// <summary>
        /// (Read Only) The list of <see cref="IGazePinchInteractor"/> components currently hovering this object.
        /// </summary>
        public List<IGazePinchInteractor> HoveringGazePinchInteractors => hoveringGazePinchInteractors ??= new List<IGazePinchInteractor>();

        private List<IGazePinchInteractor> selectingGazePinchInteractors;

        /// <summary>
        /// (Read Only) The list of <see cref="IGazePinchInteractor"/> components currently selecting this object.
        /// </summary>
        public List<IGazePinchInteractor> SelectingGazePinchInteractors => selectingGazePinchInteractors ??= new List<IGazePinchInteractor>();

        #endregion GazePinch

        #region Poke

        private List<IPokeInteractor> hoveringPokeInteractors;

        /// <summary>
        /// (Read Only) The list of <see cref="IPokeInteractor"/> components currently hovering this object.
        /// </summary>
        public List<IPokeInteractor> HoveringPokeInteractors => hoveringPokeInteractors ??= new List<IPokeInteractor>();

        #endregion Poke

        #region Grab

        private List<IGrabInteractor> hoveringGrabInteractors;

        /// <summary>
        /// (Read Only) The list of <see cref="IGrabInteractor"/> components currently hovering this object.
        /// </summary>]
        public List<IGrabInteractor> HoveringGrabInteractors => hoveringGrabInteractors ??= new List<IGrabInteractor>();

        private List<IGrabInteractor> selectingGrabInteractors;

        /// <summary>
        /// (Read Only) The list of <see cref="IGrabInteractor"/> components currently selecting this object.
        /// </summary>
        public List<IGrabInteractor> SelectingGrabInteractors => selectingGrabInteractors ??= new List<IGrabInteractor>();

        #endregion Grab

        #region Ray

        private List<IRayInteractor> hoveringRayInteractors;

        /// <summary>
        /// (Read Only) The list of <see cref="IRayInteractor"/> components currently hovering this object.
        /// </summary>]
        public List<IRayInteractor> HoveringRayInteractors => hoveringRayInteractors ??= new List<IRayInteractor>();

        #endregion Ray

        #region TimedFlags

        [SerializeField]
        [Tooltip("Is this object selected by a gaze-pinch interactor?")]
        private TimedFlag isGazePinchSelected = new TimedFlag();

        /// <summary>
        /// Is this object selected by a gaze-pinch interactor?
        /// </summary>
        public TimedFlag IsGazePinchSelected => isGazePinchSelected;

        [SerializeField]
        [Tooltip("Is this object selected by a non-gaze ray interactor?")]
        private TimedFlag isRaySelected = new TimedFlag();

        /// <summary>
        /// Is this object selected by a non-gaze ray interactor?
        /// </summary>
        public TimedFlag IsRaySelected => isRaySelected;

        [SerializeField]
        [Tooltip("Is this object selected by a poke interactor?")]
        private TimedFlag isPokeSelected = new TimedFlag();

        /// <summary>
        /// Is this object selected by a poke interactor?
        /// </summary>
        public TimedFlag IsPokeSelected => isPokeSelected;

        [SerializeField]
        [Tooltip("Is this object selected by a grab interactor?")]
        private TimedFlag isGrabSelected = new TimedFlag();

        /// <summary>
        /// Is this object selected by a grab interactor?
        /// </summary>
        public TimedFlag IsGrabSelected => isGrabSelected;

        [SerializeField]
        [Tooltip("Is this object hovered by any gaze interactor?")]
        private TimedFlag isGazeHovered = new TimedFlag();

        /// <summary>
        /// Is this object hovered by any gaze interactor?
        /// </summary>
        public TimedFlag IsGazeHovered => isGazeHovered;

        [SerializeField]
        [Tooltip("Is this object hovered by a gaze-pinch interactor?")]
        private TimedFlag isGazePinchHovered = new TimedFlag();

        /// <summary>
        /// Is this object hovered by a gaze-pinch interactor?
        /// </summary>
        public TimedFlag IsGazePinchHovered => isGazePinchHovered;

        [SerializeField]
        [Tooltip("Is this object hovered by a non-gaze ray interactor?")]
        private TimedFlag isRayHovered = new TimedFlag();

        /// <summary>
        /// Is this object hovered by a non-gaze ray interactor?
        /// </summary>
        public TimedFlag IsRayHovered => isRayHovered;

        [SerializeField]
        [Tooltip("Is this object hovered by a grab interactor?")]
        private TimedFlag isGrabHovered = new TimedFlag();

        /// <summary>
        /// Is this object hovered by a grab interactor?
        /// </summary>
        public TimedFlag IsGrabHovered => isGrabHovered;

        [SerializeField]
        [Tooltip("Is this object hovered by a near touch/poke interactor?")]
        [FormerlySerializedAs("isTouchHovered")]
        private TimedFlag isPokeHovered = new TimedFlag();

        /// <summary>
        /// Is this object hovered by a near touch/poke interactor?
        /// </summary>
        public TimedFlag IsPokeHovered => isPokeHovered;

        /// <summary>
        /// Is this object hovered by any interactor other than passive targeting interactors?
        /// </summary>
        public TimedFlag IsActiveHovered => isActiveHovered;

        [SerializeField]
        [Tooltip("Is this object hovered by any interactor other than only passive targeting interactors?")]
        private TimedFlag isActiveHovered = new TimedFlag();

        #endregion

        #region Interactor Filtering

        // private field to ensure serialization
        // todo: can we rework/get rid of this? in the NEAR FUTURE??
        [SerializeField]
        [Tooltip("Subtractively specifies the set of interactors allowed to select this interactable")]
        [Implements(typeof(IXRInteractor), TypeGrouping.ByNamespaceFlat, AllowAbstract = true)]
        private List<SystemInterfaceType> disabledInteractorTypes = new List<SystemInterfaceType>();

        /// <summary>
        /// Adds the specified type to the set of interactors which cannot select this interactable
        /// </summary>
        public void DisableInteractorType(SystemInterfaceType type)
        {
            if (!disabledInteractorTypes.Contains(type))
            {
                disabledInteractorTypes.Add(type);
            }
        }
        /// <summary>
        /// Removes the specified type from the set of interactors which cannot select this interactable
        /// </summary>
        public void EnableInteractorType(SystemInterfaceType type)
        {
            disabledInteractorTypes.Remove(type);
        }

        /// <summary>
        /// Is the given type of interactor permitted to interact with this interactable?
        /// </summary>
        public bool IsInteractorTypeValid(IXRInteractor interactor)
        {
            if (disabledInteractorTypes == null || disabledInteractorTypes.Count == 0)
            {
                return true;
            }

            if (interactor == null)
            {
                return false;
            }

            // Cache queried interactor type to extract from hot loop.
            Type interactorType = interactor.GetType();
            int count = disabledInteractorTypes.Count;

            for (int i = 0; i < count; i++)
            {
                Type disabledType = disabledInteractorTypes[i]?.Type;
                if (disabledType != null && disabledType.IsAssignableFrom(interactorType))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion Interactor Filtering

        // Refcount-style trackers to avoid LINQs to query
        // selection types.
        private int raySelectCount = 0;
        private int gazePinchSelectCount = 0;
        private int grabSelectCount = 0;
        private int pokeSelectCount = 0;
        private int activeHoverCount = 0;

        private void UpdateHoverFlags()
        {
            IsGazeHovered.Active = hoveringGazeInteractors != null && hoveringGazeInteractors.Count > 0;
            IsGazePinchHovered.Active = hoveringGazePinchInteractors != null && hoveringGazePinchInteractors.Count > 0;
            IsRayHovered.Active = hoveringRayInteractors != null && hoveringRayInteractors.Count > 0;
            IsGrabHovered.Active = hoveringGrabInteractors != null && hoveringGrabInteractors.Count > 0;
            IsPokeHovered.Active = hoveringPokeInteractors != null && hoveringPokeInteractors.Count > 0;
            IsActiveHovered.Active = activeHoverCount > 0;
        }

        private void UpdateSelectFlags(bool increment, IXRInteractor interactor)
        {
            if (interactor is IRayInteractor)
            {
                raySelectCount = increment ? raySelectCount + 1 : (raySelectCount > 0 ? raySelectCount - 1 : 0);
            }
            if (interactor is IGazePinchInteractor)
            {
                gazePinchSelectCount = increment ? gazePinchSelectCount + 1 : (gazePinchSelectCount > 0 ? gazePinchSelectCount - 1 : 0);
            }
            if (interactor is IGrabInteractor)
            {
                grabSelectCount = increment ? grabSelectCount + 1 : (grabSelectCount > 0 ? grabSelectCount - 1 : 0);
            }
            if (interactor is IPokeInteractor)
            {
                pokeSelectCount = increment ? pokeSelectCount + 1 : (pokeSelectCount > 0 ? pokeSelectCount - 1 : 0);
            }

            isRaySelected.Active = raySelectCount > 0;
            isGazePinchSelected.Active = gazePinchSelectCount > 0;
            isGrabSelected.Active = grabSelectCount > 0;
            isPokeSelected.Active = pokeSelectCount > 0;
        }

        #region XRI methods

        /// <inheritdoc />
        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            return base.IsSelectableBy(interactor) && IsInteractorTypeValid(interactor);
        }

        /// <inheritdoc />
        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            return base.IsHoverableBy(interactor) && IsInteractorTypeValid(interactor);
        }

        /// <inheritdoc />
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);

            UpdateSelectFlags(true, args.interactorObject);

            if (args.interactorObject is IGazePinchInteractor gazePinchInteractor)
            {
                (selectingGazePinchInteractors ??= new List<IGazePinchInteractor>()).Add(gazePinchInteractor);
            }

            if (args.interactorObject is IGrabInteractor grabInteractor)
            {
                (selectingGrabInteractors ??= new List<IGrabInteractor>()).Add(grabInteractor);
            }
        }

        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);

            UpdateSelectFlags(false, args.interactorObject);

            if (args.interactorObject is IGazePinchInteractor gazePinchInteractor)
            {
                selectingGazePinchInteractors?.Remove(gazePinchInteractor);
            }

            if (args.interactorObject is IGrabInteractor grabInteractor)
            {
                selectingGrabInteractors?.Remove(grabInteractor);
            }
        }

        /// <inheritdoc />
        protected override void OnHoverEntering(HoverEnterEventArgs args)
        {
            base.OnHoverEntering(args);

            if (args.interactorObject is IGazeInteractor gazeInteractor)
            {
                (hoveringGazeInteractors ??= new List<IGazeInteractor>()).Add(gazeInteractor);
            }
            else // TODO: possibly move to IPassiveInteractor instead of hardcoding on gaze
            {
                activeHoverCount++;
            }

            if (args.interactorObject is IGazePinchInteractor gazePinchInteractor)
            {
                (hoveringGazePinchInteractors ??= new List<IGazePinchInteractor>()).Add(gazePinchInteractor);
            }

            if (args.interactorObject is IGrabInteractor grabInteractor)
            {
                (hoveringGrabInteractors ??= new List<IGrabInteractor>()).Add(grabInteractor);
            }

            if (args.interactorObject is IPokeInteractor pokeInteractor)
            {
                (hoveringPokeInteractors ??= new List<IPokeInteractor>()).Add(pokeInteractor);
            }

            if (args.interactorObject is IRayInteractor rayInteractor)
            {
                (hoveringRayInteractors ??= new List<IRayInteractor>()).Add(rayInteractor);
            }

            UpdateHoverFlags();
        }

        /// <inheritdoc />
        protected override void OnHoverExiting(HoverExitEventArgs args)
        {
            base.OnHoverExiting(args);

            if (args.interactorObject is IGazeInteractor gazeInteractor)
            {
                hoveringGazeInteractors?.Remove(gazeInteractor);
            }
            else // TODO: possibly move to IPassiveInteractor instead of hardcoding on gaze
            {
                if (activeHoverCount > 0) { activeHoverCount--; }
            }

            if (args.interactorObject is IGazePinchInteractor gazePinchInteractor)
            {
                hoveringGazePinchInteractors?.Remove(gazePinchInteractor);
            }

            if (args.interactorObject is IGrabInteractor grabInteractor)
            {
                hoveringGrabInteractors?.Remove(grabInteractor);
            }

            if (args.interactorObject is IPokeInteractor pokeInteractor)
            {
                hoveringPokeInteractors?.Remove(pokeInteractor);
            }

            if (args.interactorObject is IRayInteractor rayInteractor)
            {
                hoveringRayInteractors?.Remove(rayInteractor);
            }

            UpdateHoverFlags();
        }

        #endregion
    }
}

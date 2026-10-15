// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// Adds the chosen tap sound to the NonNativeKeyboard buttons
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class KeyboardAudio : MonoBehaviour
    {
        private AudioSource clickSoundPlayer;
        private Button[] buttons;

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary>
        private void Start()
        {
            EnableTouch();
        }

        private void EnableTouch()
        {
            clickSoundPlayer = GetComponent<AudioSource>();
            buttons = GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                button.onClick.AddListener(PlayClick);
            }
        }

        private void OnDestroy()
        {
            if (buttons != null)
            {
                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        button.onClick.RemoveListener(PlayClick);
                    }
                }
            }
        }

        private void PlayClick()
        {
            if (clickSoundPlayer != null && clickSoundPlayer.isActiveAndEnabled)
            {
                clickSoundPlayer.Play();
            }
        }
    }
}

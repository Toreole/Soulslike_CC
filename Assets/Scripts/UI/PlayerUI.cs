using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Soulslike.UI
{
    /// <summary>
    /// Handles the UI elements for the player. Supposed to get all its info via events from the Player.
    /// </summary>
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private Slider healthSlider;
        [SerializeField]
        private Slider staminaSlider;
        [SerializeField]
        private float sizePerPoint = 1f;

        private RectTransform staminaRect;
        private RectTransform healthRect;

        private void Awake()
        {
            staminaRect = staminaSlider.transform as RectTransform;
            healthRect = healthSlider.transform as RectTransform;
        }

        /// <summary>
        /// Updates the health slider to the new value for health.
        /// </summary>
        /// <param name="value"></param>
        public void OnHealthChanged(float value)
        {
            healthSlider.value = value;
        }

        /// <summary>
        /// Updates the stamina slider to the new value for stamina.
        /// </summary>
        /// <param name="value"></param>
        public void OnStaminaChanged(float value)
        {
            staminaSlider.value = value;
        }

        public void OnMaxHealthChanged(float maxHealth)
        {
            float width = maxHealth * sizePerPoint;
            SetWidth(healthRect, width);
        }

        public void OnMaxStaminaChanged(float maxStamina)
        {
            float width = maxStamina * sizePerPoint;
            SetWidth(staminaRect, width);
        }

        private void SetWidth(RectTransform rt, float width)
        {
            float currentWidth = rt.rect.width;
            float delta = width - currentWidth;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            Vector2 pos = rt.anchoredPosition;
            pos.x += delta * 0.5f;
            rt.anchoredPosition = pos;
        }


    }
}

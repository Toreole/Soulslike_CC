using System.Collections;
using UnityEngine;

namespace Soulslike
{
    public class PlayerEntity : MonoBehaviour, IDamageable
    {
        [SerializeField]
        private PlayerMachine playerMachine;
        [SerializeField]
        private float baseHealth;
        [SerializeField]
        private UI.PlayerUI playerUI;

        //RUNTIME
        private float maxHealth;
        private float health;
        private bool invincible;

        //UNITY MESSAGES
        void Awake()
        {
            maxHealth = baseHealth; //+ ...
            health = maxHealth;
        }

        private void OnEnable()
        {
            playerMachine.OnStaminaChanged += playerUI.OnStaminaChanged;
            playerMachine.OnMaxStaminaChanged += playerUI.OnMaxStaminaChanged;
            playerUI.OnMaxHealthChanged(maxHealth);
            playerUI.OnHealthChanged(health);
        }

        private void OnDisable()
        {
            playerMachine.OnStaminaChanged -= playerUI.OnStaminaChanged;
            playerMachine.OnMaxStaminaChanged -= playerUI.OnMaxStaminaChanged;
        }

        //private void Update()
        //{
        //    //health -= 2 * Time.deltaTime;
        //    //playerUI.OnHealthChanged(health);
        //}

        //INTERFACES
        //this is designed to process incoming damage from attacks.
        //damage-over-time effects or fall damage are not attacks on their own, and need to be handled seperately.
        public void Damage(float amount)
        {
            if (invincible)
                return; //ignore incoming damage events when invincible.
            health -= amount;
            if(health <= 0)
            {
                //playerMachine.OnDeath();
            }
            else
            {
                //playerMachine.OnReceiveHit();
            }
        }
    }
}
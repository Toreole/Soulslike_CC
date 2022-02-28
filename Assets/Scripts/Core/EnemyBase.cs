using UnityEngine;

namespace Soulslike
{
    /// <summary>
    /// The base class for ALL enemies in the game.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [SerializeField]
        private float baseHealth;

        public abstract void Damage(float damage);

        /// <summary>
        /// Enemies should register as "visible" to the player camera when they start being rendered.
        /// </summary>
        private void OnBecameVisible()
        {
            CameraController.RegisterEnemyInView(this);
            Debug.Log("visible!");
        }

        /// <summary>
        /// Just like OnBecameVisible, enemies should unregister as "visible" when they stop being rendered.
        /// </summary>
        private void OnBecameInvisible()
        {
            CameraController.UnregisterEnemyInView(this);
            Debug.Log("INvisible!");
        }

        /// <summary>
        /// See OnBecameInvisible.
        /// </summary>
        protected virtual void OnDisable()
        {
            CameraController.UnregisterEnemyInView(this);
        }

    }
}

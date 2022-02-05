using UnityEngine;

namespace Soulslike
{
    public class TargetDummy : MonoBehaviour, IDamageable
    {
        public void Damage(float amount)
        {
            Debug.Log($"Dummy hit for {amount} damage.");
        }
    }
}

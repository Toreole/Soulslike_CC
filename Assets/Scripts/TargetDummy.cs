using UnityEngine;

namespace Soulslike
{
    public class TargetDummy : EnemyBase
    {
        public override void Damage(float amount)
        {
            Debug.Log($"Dummy hit for {amount} damage.");
        }
    }
}

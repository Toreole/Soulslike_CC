using System.Collections;
using UnityEngine;

namespace Soulslike
{
    public interface IEntity : ITargetable, IDamageable
    {

    }

    public interface ITargetable
    {
        Transform GetLockPosition(Transform camera);
    }
}
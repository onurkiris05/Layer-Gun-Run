using UnityEngine;

namespace Game.Collectables
{
    public abstract class BaseCollectable : MonoBehaviour
    {

        protected abstract void OnTriggerEnter(Collider other);
    }
}
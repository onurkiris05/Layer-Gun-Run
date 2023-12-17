using UnityEngine;

namespace Game.Gates
{
    public abstract class BaseGate : MonoBehaviour
    {
        protected bool _isKilled;


        #region UNITY EVENTS

        protected virtual void Start() => InitGate();

        protected abstract void OnTriggerEnter(Collider other);

        #endregion


        #region RPOTECTED ABSTRACT METHODS

        protected abstract void KillGate();

        protected abstract void InitGate();

        #endregion
    }
}
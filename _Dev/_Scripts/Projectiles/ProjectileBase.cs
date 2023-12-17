using System;
using TMPro;
using UnityEngine;

namespace Game.Projectiles
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected float projectileSpeed;

        [Space] [Header("Components")]
        [SerializeField] protected TextMeshPro powerText;

        public float Power => _power;

        protected TrailRenderer _trailRenderer;
        protected Rigidbody _rigidbody;
        protected Collider _collider;
        protected Action _projectileBehaviour;

        protected Vector3 _startPos;
        protected float _range;
        protected float _power;


        #region UNITY EVENTS

        protected void Awake()
        {
            System.Globalization.NumberFormatInfo _numberInfo = new System.Globalization.NumberFormatInfo();
            //_numberInfo.CurrencyDecimalSeparator =;
            _trailRenderer = GetComponent<TrailRenderer>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }


        protected void OnDisable() => _trailRenderer.Clear();


        protected virtual void Update()
        {
            _projectileBehaviour?.Invoke();
        }

        #endregion


        #region PUBLIC METHODS

        public abstract void Init(ProjectileData data);

        public abstract void Kill();

        #endregion


        #region PROTECTED METHODS

        protected virtual void SetTrail(bool state)
        {
            _trailRenderer.enabled = state;
            _trailRenderer.emitting = state;
        }


        protected virtual void Behaviour_Standard()
        {
            if (Vector3.Distance(_startPos, transform.position) > _range)
                Kill();
        }

        #endregion
    }
}
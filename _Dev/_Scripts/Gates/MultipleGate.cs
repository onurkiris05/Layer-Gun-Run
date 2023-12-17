using System.Collections.Generic;
using UnityEngine;

namespace Game.Gates
{
    public class MultipleGate : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private List<GenericGate> childGates;


        private void Start()
        {
            InitChildGates();
        }


        public void DeactivateAll(GenericGate killedGate)
        {
            childGates.Remove(killedGate);

            foreach (var childGate in childGates)
                childGate.DisableGate();
        }


        private void InitChildGates()
        {
            foreach (var childGate in childGates)
                childGate.SetParentGate(this);
        }
    }
}
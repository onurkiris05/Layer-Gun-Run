using Game.Interfaces;
using UnityEngine;

namespace Game.Managers
{
    [System.Serializable]
    public class AnimatorData
    {
        public string Name;
        public Animator Animator;
    }

    public class AnimationManager : MonoBehaviour, IAnimationManager
    {
        [Header("Settings")]
        [SerializeField] private AnimatorData[] animators;


        #region PUBLIC METHODS

        public void SetAnimationBool(string animatorName, string animationName, bool state)
        {
            var selectedAnimator = GetAnimator(animatorName);
            if (selectedAnimator == null) return;

            selectedAnimator.SetBool(animationName, state);
        }

        public void SetAnimationTrigger(string animatorName, string triggerName)
        {
            var selectedAnimator = GetAnimator(animatorName);
            if (selectedAnimator == null) return;

            selectedAnimator.SetTrigger(triggerName);
        }

        #endregion

        #region PRIVATE METHODS

        private Animator GetAnimator(string name)
        {
            foreach (var animatorData in animators)
            {
                if (animatorData.Name == name)
                    return animatorData.Animator;
            }

            Debug.LogError($"{gameObject.name} - Animator name not found!!!");
            return null;
        }

        #endregion
    }
}
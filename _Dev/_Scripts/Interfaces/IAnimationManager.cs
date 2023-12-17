namespace Game.Interfaces
{
    public interface IAnimationManager
    {
        void SetAnimationBool(string animatorName, string animationName, bool state);
        void SetAnimationTrigger(string animatorName, string triggerName);
    }
}
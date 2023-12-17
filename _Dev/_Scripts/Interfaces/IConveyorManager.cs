using Game.Collectables;

namespace Game.Interfaces
{
    public interface IConveyorManager
    {
        void SendBulletToUpgradeGate(BulletRewardData bullet);
        void MoveNextUpgradeGate();
    }
}
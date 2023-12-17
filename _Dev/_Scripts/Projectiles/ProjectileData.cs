using UnityEngine;

namespace Game.Projectiles
{
    public class ProjectileData
    {
        public float Range;
        public float Power;
        public Vector3 Scale;

        public ProjectileData(
            float range,
            float power,
            Vector3 scale)
        {
            Range = range;
            Power = power;
            Scale = scale;
        }
    }
}
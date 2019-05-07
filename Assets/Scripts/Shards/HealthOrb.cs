using System;

namespace Shards
{
    public class HealthOrb : Shard
    {
        public float healValue;
        protected override void AffectPlayer()
        {
            Heal();
        }
        
        private void Heal()
        {
            target.currentHealth += healValue;
        }

        protected override void SetColor()
        {
            
        }
    }
}
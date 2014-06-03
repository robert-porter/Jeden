using System;
using System.Collections.Generic;
using Jeden.Engine;
using Jeden.Engine.Object;
using Jeden.Game.Physics;

namespace Jeden.Game
{

    class DamageMessage : Message 
    {
        public float Damage { get; set; }

        public DamageMessage(Component sender, float damage) : base(sender)
        {
            Damage = damage;
        }
    }


    /// <summary>
    /// Represents a GameObjects health
    /// </summary>
    class HealthComponent : Component
    {
        /// <summary>
        /// The parent's maximum health.
        /// </summary>
        public float MaxHealth { get; set; }

        /// <summary>
        /// The parent's current health.
        /// </summary>
        public float CurrentHealth { get; set; }

        public float MaxShield { get; set; }
        public float CurrentShield { get; set; }

        public HealthComponent(GameObject parent, float maxHealth, float maxShield)
            : base(parent)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MaxShield = maxShield;
            CurrentShield = maxShield;
        }

        public override void Update(GameTime gameTime)
        {
            if (CurrentHealth <= 0)
            {
                Parent.HandleMessage(new InvalidateMessage(this));
            }

            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        public override void HandleMessage(Message message)
        {
            base.HandleMessage(message);
            if (message is DamageMessage)
            {
                DamageMessage damageMessage = message as DamageMessage;

                if (CurrentShield > 0)
                {
                    if (damageMessage.Damage > CurrentShield)
                    {
                        float healthDamage = damageMessage.Damage - CurrentShield;
                        CurrentShield = 0;
                        CurrentHealth -= healthDamage;
                    }
                    else
                    {
                        CurrentShield -= damageMessage.Damage;
                    }
                    GameObjectFactory.CreateShieldDamageEffect(Parent.Position);
                }
                else
                {
                    CurrentHealth -= damageMessage.Damage;
                }
            }
        }
    }


}

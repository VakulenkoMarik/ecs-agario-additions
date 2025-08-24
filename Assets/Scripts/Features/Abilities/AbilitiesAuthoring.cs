using System.Collections.Generic;
using Features.Abilities.Types;
using Unity.Entities;
using UnityEngine;

namespace Features.Abilities
{
    [InternalBufferCapacity(2)]
    public struct Ability : IBufferElementData
    {
        public AbilityType type;
        public Entity prefab;
        public double cooldown;
        public double cooldownTimestamp;
    }
    
    public class AbilitiesAuthoring : MonoBehaviour
    {
        public List<AbilitySO> abilities = new ();
    }
    
    public class AbilitiesBaker : Baker<AbilitiesAuthoring>
    {
        public override void Bake(AbilitiesAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<Ability>(entity);
            foreach (var abilityAuthoring in authoring.abilities)
            {
                buffer.Add(new Ability
                {
                    type = abilityAuthoring.type,
                    cooldown = abilityAuthoring.cooldown,
                    prefab = GetEntity(abilityAuthoring.prefab, TransformUsageFlags.Dynamic),
                    cooldownTimestamp = 0.0f,
                });
            }
        }
    }
}
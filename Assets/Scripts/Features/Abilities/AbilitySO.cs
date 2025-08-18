using Features.Abilities.Types;
using UnityEngine;

namespace Features.Abilities
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Abilities/Ability")]
    public class AbilitySO : ScriptableObject
    {
        public AbilityType type;
        public double cooldown;
        public GameObject prefab;
    }
}
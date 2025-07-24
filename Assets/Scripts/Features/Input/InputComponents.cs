using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Features.Input
{
    public struct InputActionsComponent : IComponentData
    {
    }

    public struct PlayerInputComponent : IComponentData, IEquatable<PlayerInputComponent>, IEnableableComponent
    {
        private const float PressedThreshold = 0.2f;
        
        public float2 moveValue;
        public float2 lookValue;
        public float attackValue;
        public float jumpValue;
        
        public bool IsAttackPressed => attackValue > PressedThreshold;
        public bool IsJumpPressed => jumpValue > PressedThreshold;

        public bool Equals(PlayerInputComponent other)
        {
            return moveValue.Equals(other.moveValue) && lookValue.Equals(other.lookValue) && attackValue == other.attackValue && jumpValue == other.jumpValue;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerInputComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(moveValue, lookValue, attackValue, jumpValue);
        }
        
        public static bool operator ==(PlayerInputComponent left, PlayerInputComponent right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(PlayerInputComponent left, PlayerInputComponent right)
        {
            return !left.Equals(right);
        }
    }
}
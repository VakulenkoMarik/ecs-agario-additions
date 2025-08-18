using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;

namespace Features.Input
{
    public struct InputBridge : IComponentData
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool isFocussed;
    }

    public struct GameCommands : IComponentData, IEquatable<GameCommands>, IEnableableComponent
    {
        private const float PressedThreshold = 0.2f;
        
        [MarshalAs(UnmanagedType.U1)]
        public bool isTargetValid;
        public float2 targetValue;
        public float2 moveValue;
        public float feedValue;
        public float jumpValue;
        
        public bool IsFeedPressed => feedValue > PressedThreshold;
        public bool IsJumpPressed => jumpValue > PressedThreshold;

        public bool Equals(GameCommands other)
        {
            return moveValue.Equals(other.moveValue) && targetValue.Equals(other.targetValue) && feedValue == other.feedValue && jumpValue == other.jumpValue;
        }

        public override bool Equals(object obj)
        {
            return obj is GameCommands other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(moveValue, targetValue, feedValue, jumpValue);
        }
        
        public static bool operator ==(GameCommands left, GameCommands right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(GameCommands left, GameCommands right)
        {
            return !left.Equals(right);
        }
    }
}
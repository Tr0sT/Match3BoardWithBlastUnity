#nullable enable
using Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct PositionComponent : IComponent
    {
        public int2 Value;

        public PositionComponent(int2 position)
        {
            Value = position;
        }

        public int2 GetBottomPosition() => new(Value.x, Value.y + 1);
        public int2 GetTopPosition() => new(Value.x, Value.y - 1);
        public int2 GetLeftPosition() => new(Value.x - 1, Value.y);
        public int2 GetRightPosition() => new(Value.x + 1, Value.y);
        public int2 GetTopLeftPosition() => new(Value.x - 1, Value.y - 1);
        public int2 GetTopRightPosition() => new(Value.x + 1, Value.y - 1);
        public int2 GetBottomLeftPosition() => new(Value.x - 1, Value.y + 1);
        public int2 GetBottomRightPosition() => new(Value.x + 1, Value.y + 1);
    }
}
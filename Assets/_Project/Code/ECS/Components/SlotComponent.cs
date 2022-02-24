#nullable enable
using Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct SlotComponent : IComponent
    {
        public Entity? ChipEntity;
        
        public bool IsEmpty => ChipEntity == null;
        public bool IsStable => !IsEmpty && !ChipEntity.Has<MovingComponent>();
    }

}
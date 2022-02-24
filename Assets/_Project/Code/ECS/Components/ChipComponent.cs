#nullable enable
using Morpeh;
using Unity.IL2CPP.CompilerServices;
using Views;

namespace Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct ChipComponent : IComponent
    {
        public enum ChipType
        {
            One, Two, Three, Four, Five, Six
        }
        public ChipType Type { get; }
        public BaseChipView ChipView { get; }

        public ChipComponent(ChipType type, BaseChipView chipView)
        {
            Type = type;
            ChipView = chipView;
        }
    }
}
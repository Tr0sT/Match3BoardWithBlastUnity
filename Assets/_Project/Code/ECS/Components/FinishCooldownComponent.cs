#nullable enable
using Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct FinishCooldownComponent : IComponent
    {
        public float CooldownPeriod { get; }

        public FinishCooldownComponent(float cooldownPeriod)
        {
            CooldownPeriod = cooldownPeriod;
        }
    }
}
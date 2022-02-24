#nullable enable
using Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct CooldownComponent : IComponent
    {
        public float CurrentCooldownPeriod { get; private set; }
        public float CooldownIncrement { get; private set; }
        public float TimeInCooldown;

        public float NextCooldownPeriod => CurrentCooldownPeriod + CooldownIncrement;

        public CooldownComponent(float startCooldown, float cooldownIncrement)
        {
            CurrentCooldownPeriod = startCooldown;
            CooldownIncrement = cooldownIncrement;
            TimeInCooldown = 0.0f;
        }

        public void UpdateCooldown(float removedCooldownCooldownPeriod)
        {
            CurrentCooldownPeriod = removedCooldownCooldownPeriod;
        }
    }
}
#nullable enable
using Components;
using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Systems/" + nameof(CooldownSystem))]
    public sealed class CooldownSystem : UpdateSystem
    {
        private Filter cooldownFilter = null!;
        private Filter finishedCooldownFilter = null!;
        private Filter resumedFinishedCooldownFilter = null!;

        public override void OnAwake()
        {
            cooldownFilter = World.Filter.With<CooldownComponent>().Without<FinishCooldownComponent>();
            finishedCooldownFilter = World.Filter.With<FinishCooldownComponent>().Without<CooldownComponent>();
            resumedFinishedCooldownFilter = World.Filter.With<CooldownComponent>().With<FinishCooldownComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var removedCooldownEntity in finishedCooldownFilter)
                removedCooldownEntity.RemoveComponent<FinishCooldownComponent>();
            
            foreach (var cooldownEntity in cooldownFilter)
            {
                ref var cooldown = ref cooldownEntity.GetComponent<CooldownComponent>();
                if (cooldown.TimeInCooldown > cooldown.CurrentCooldownPeriod)
                {
                    cooldownEntity.SetComponent(new FinishCooldownComponent(cooldown.NextCooldownPeriod));
                    cooldownEntity.RemoveComponent<CooldownComponent>();
                }
                else
                    cooldown.TimeInCooldown += deltaTime;
            }

            foreach (var resumedCooldownEntity in resumedFinishedCooldownFilter)
            {
                var removedCooldown = resumedCooldownEntity.GetComponent<FinishCooldownComponent>();
                ref var cooldown = ref resumedCooldownEntity.GetComponent<CooldownComponent>();
                cooldown.UpdateCooldown(removedCooldown.CooldownPeriod);
                resumedCooldownEntity.RemoveComponent<FinishCooldownComponent>();
                cooldown.TimeInCooldown += deltaTime;
            }
        }
    }
}
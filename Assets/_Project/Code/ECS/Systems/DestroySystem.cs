#nullable enable
using Commands;
using Components;
using Controllers;
using Morpeh;
using Morpeh.Helpers;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Systems/" + nameof(BlastSystem))]
    public sealed class DestroySystem : UpdateSystem
    {
        private ChipsFactory chipsFactory = null!;
        private Filter destroyAllFilter = null!;
        private Filter chipsFilter = null!;
        private Filter destroyChipsFilter = null!;
        private Filter slotsFilter = null!;

        public DestroySystem Init(ChipsFactory chipsFactory)
        {
            this.chipsFactory = chipsFactory;
            return this;
        }
        
        public override void OnAwake()
        {
            destroyAllFilter = World.Filter.With<DestroyAllCommand>();
            chipsFilter = World.Filter.With<ChipComponent>();
            destroyChipsFilter = World.Filter.With<DestroyComponent>();
            slotsFilter = World.Filter.With<SlotComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!destroyAllFilter.IsEmpty())
            {
                destroyAllFilter.RemoveAllEntities(World);
                foreach (var chip in chipsFilter)
                    if (!chip.Has<DestroyComponent>())
                        chip.AddComponent<DestroyComponent>();
            }

            var addAnalyzeCommand = false;
            foreach (var chip in destroyChipsFilter)
            {
                foreach (var entity in slotsFilter)
                {
                    ref var slot = ref entity.GetComponent<SlotComponent>();
                    if (slot.ChipEntity != chip) 
                        continue;
                    
                    slot.ChipEntity = null;
                    if (!addAnalyzeCommand)
                    {
                        entity.AddComponent<AnalyzeCommand>();
                        addAnalyzeCommand = true;
                    }

                    break;
                }
                chipsFactory.Release(chip);
            }
            
        }
    }
}
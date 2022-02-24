#nullable enable
using Commands;
using Components;
using Controllers;
using Morpeh;
using Morpeh.Helpers;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using Utilities;

namespace Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Systems/" + nameof(ChipsGenerationSystem))]
    public sealed class ChipsGenerationSystem : UpdateSystem
    {
        private const float START_COOLDOWN_PERIOD = 0.25f;
        private const float COOLDOWN_PERIOD_INCREMENT = 0.03f;

        private ChipsFactory chipsFactory = null!;

        private Filter chipGeneratorFilter = null!;
        private Filter cellFilter = null!;
        private Filter generateFilter = null!;

        private bool pause;

        public ChipsGenerationSystem Init(ChipsFactory chipsFactory)
        {
            this.chipsFactory = chipsFactory;
            return this;
        }

        public override void OnAwake()
        {
            chipGeneratorFilter = World.Filter.With<ChipsGeneratorComponent>().Without<CooldownComponent>();
            cellFilter = World.Filter.With<CellComponent>().Without<ChipsGeneratorComponent>();
            generateFilter = World.Filter.With<GenerateCommand>();

            pause = true;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!generateFilter.IsEmpty())
            {
                generateFilter.RemoveAllEntities(World);
                pause = false;
            }

            if (pause)
                return;
                
            Generate();
        }

        private void Generate()
        {
            var cellsCache = BoardUtilities.CreateCellsCache(cellFilter);
            foreach (var entity in chipGeneratorFilter)
            {
                var bottomCell = BoardUtilities.GetNeighbourCellFor(entity, cellsCache, (p) => p.GetBottomPosition());
                SystemExtensions.ThrowWhenNull(bottomCell);
                
                var bottomCellSlot = bottomCell.GetComponent<SlotComponent>();
                if (!bottomCellSlot.IsEmpty)
                    continue;
                
                var position = entity.GetComponent<PositionComponent>().Value;
                ref var slot = ref entity.GetComponent<SlotComponent>();
                slot.ChipEntity = chipsFactory.GetRandom(position.x, position.y);

                entity.SetComponent(new CooldownComponent(START_COOLDOWN_PERIOD, COOLDOWN_PERIOD_INCREMENT));
                entity.AddComponent<AnalyzeCommand>();
            }
        }
    }
}
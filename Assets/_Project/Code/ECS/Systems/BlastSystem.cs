#nullable enable
using Commands;
using Components;
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
    [CreateAssetMenu(menuName = "ECS/Systems/" + nameof(BlastSystem))]
    public sealed class BlastSystem : UpdateSystem
    {
        private Filter blastFilter = null!;
        private Filter cellsFilter = null!;

        public override void OnAwake()
        {
            blastFilter = World.Filter.With<BlastCommand>();
            cellsFilter = World.Filter.With<CellComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (blastFilter.IsEmpty())
                return;

            var cellsCache = BoardUtilities.CreateCellsCache(cellsFilter);
            var blastAdded = true;
            while (blastAdded)
            {
                blastAdded = false;
                foreach (var cell in cellsFilter)
                {
                    var slot = cell.GetComponent<SlotComponent>();
                    if (!slot.IsStable || slot.ChipEntity.Has<BlastCommand>())
                        continue;

                    SystemExtensions.ThrowWhenNull(slot.ChipEntity);
                    var topCell = BoardUtilities.GetNeighbourCellFor(cell, cellsCache, (p) => p.GetTopPosition());
                    blastAdded = TryAddBlast(slot.ChipEntity, topCell, blastAdded);
                    if (blastAdded)
                        continue;
                    
                    var bottomCell = BoardUtilities.GetNeighbourCellFor(cell, cellsCache, (p) => p.GetBottomPosition());
                    blastAdded = TryAddBlast(slot.ChipEntity, bottomCell, blastAdded);
                    if (blastAdded)
                        continue;
                    
                    var leftCell = BoardUtilities.GetNeighbourCellFor(cell, cellsCache, (p) => p.GetLeftPosition());
                    blastAdded = TryAddBlast(slot.ChipEntity, leftCell, blastAdded);
                    if (blastAdded)
                        continue;
                    
                    var rightCell = BoardUtilities.GetNeighbourCellFor(cell, cellsCache, (p) => p.GetRightPosition());
                    blastAdded = TryAddBlast(slot.ChipEntity, rightCell, blastAdded);
                }
            }

            World.UpdateFilters();
            foreach (var entity in blastFilter)
            {
                entity.RemoveComponent<BlastCommand>();
                
                if (!entity.Has<DestroyComponent>())
                    entity.AddComponent<DestroyComponent>();
            }
        }

        private static bool TryAddBlast(Entity chip, Entity? neighbourCell, bool blastAdded)
        {
            if (neighbourCell == null)
                return blastAdded;
            
            var slot = neighbourCell.GetComponent<SlotComponent>();
            if (slot.IsStable && slot.ChipEntity.Has<BlastCommand>())
            {
                if (chip.GetComponent<ChipComponent>().Type == slot.ChipEntity.GetComponent<ChipComponent>().Type)
                {
                    chip.AddComponent<BlastCommand>();
                    blastAdded = true;
                }
            }

            return blastAdded;
        }
    }
}
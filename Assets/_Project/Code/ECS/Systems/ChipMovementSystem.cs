#nullable enable
using System.Collections.Generic;
using System.Linq;
using Commands;
using Components;
using Morpeh;
using Morpeh.Helpers;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using Utilities;

namespace Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Systems/" + nameof(ChipMovementSystem))]
    public sealed class ChipMovementSystem : UpdateSystem
    {
        private const float MOVE_TIME = 0.2f;
        private IRandomizer randomizer = null!;
        
        private Filter slotFilter = null!;
        private Filter analyzeFilter = null!;

        private Dictionary<int2, Entity> cellsCache = null!;
        public ChipMovementSystem Init(IRandomizer randomizer)
        {
            this.randomizer = randomizer;
            return this;
        }

        public override void OnAwake()
        {
            slotFilter = World.Filter.With<SlotComponent>();
            analyzeFilter = World.Filter.With<AnalyzeCommand>();
        }

        //TODO: possible bottleneck, need optimization
        // all this difficult logic for this scenario:
        // 1 x 1 x 
        // x 0 x 0
        // where 1 - ball, 0 - empty cell, x - blocked cell
        // i assume that the result should always be:
        // 0 x 0 x
        // x 1 x 1
        // and never:
        // 1 x 0 x
        // 0 1 x 0
        public override void OnUpdate(float deltaTime)
        {
            if (analyzeFilter.IsEmpty())
                return;
            
            analyzeFilter.RemoveComponentForAll<AnalyzeCommand>();
            
            var emptyCells = slotFilter.Where(entity => entity.GetComponent<SlotComponent>().IsEmpty).ToList();
            cellsCache = BoardUtilities.CreateCellsCache(slotFilter);

            randomizer.Shuffle(emptyCells);
            var wasRain = true;
            while (wasRain)
            {
                while (CheckVerticalRain(emptyCells)) { }

                wasRain = CheckDiagonalRain(emptyCells);
            }
        }
        
        private bool CheckVerticalRain(List<Entity> emptyCells)
        {
            foreach (var entityCell in emptyCells)
            {
                var topCell = BoardUtilities.GetNeighbourCellFor(entityCell, cellsCache, (p) => p.GetTopPosition());
                
                if (topCell == null)
                    continue;

                var slot = topCell.GetComponent<SlotComponent>();
                if (slot.IsEmpty == false && slot.ChipEntity.Has<MovingComponent>() == false)
                {
                    Rain(topCell, entityCell, emptyCells);
                    return true;
                }
            }

            return false;
        }

        private bool CheckDiagonalRain(List<Entity> emptyCells)
        {
            List<(Entity emptyCell, List<Entity> topDiagonalCells)> rainyCells = new();
            foreach (var cell in emptyCells)
            {
                var nearCells = new List<Entity>();
                
                var topLeftCell = BoardUtilities.GetNeighbourCellFor(cell, cellsCache, (p) => p.GetTopLeftPosition());
                if (topLeftCell != null && topLeftCell.GetComponent<SlotComponent>().IsStable)
                    nearCells.Add(topLeftCell);
                var topRightCell = BoardUtilities.GetNeighbourCellFor(cell, cellsCache, (p) => p.GetTopRightPosition());
                if (topRightCell != null && topRightCell.GetComponent<SlotComponent>().IsStable)
                    nearCells.Add(topRightCell);
                
                if (nearCells.Count == 0)
                    continue;
                rainyCells.Add((cell, nearCells));
            }
            
            randomizer.Shuffle(rainyCells);
            for (var i = 0; i < rainyCells.Count; i++)
            {
                // 1  x  1 x 
                // x (0) x 0
                var emptyCell = rainyCells[i].Item1;
                // (1)  x  (1) x 
                //  x   0   x  0
                var nearCells = rainyCells[i].Item2;
                if (nearCells.Count == 1)
                {
                    if (!nearCells[0].GetComponent<SlotComponent>().IsStable)
                        continue;
                    Rain(nearCells[0], rainyCells[i].Item1, emptyCells);
                    return true;
                }

                for (var j = nearCells.Count - 1; j >= 0; j--)
                {
                    if (!nearCells[j].GetComponent<SlotComponent>().IsStable)
                        nearCells.RemoveAt(j);
                }
                if (nearCells.Count == 1)
                {
                    Rain(nearCells[0], rainyCells[i].Item1, emptyCells);
                    return true;
                }

                if (nearCells.Count == 0)
                    continue;
                
                // 1 x (1) x 
                // x 0  x  0
                var randomCell = randomizer.GetRandomElement(nearCells);
                if (GetAnotherDiagonalDestinationFor(randomCell, emptyCell) == null)
                {
                    Rain(randomCell, emptyCell, emptyCells);
                    return true;
                }
                // (1) x 1 x 
                //  x  0 x 0
                var anotherCell = nearCells[0] == randomCell ? nearCells[1] : nearCells[0];
                
                var cellsInTopRowCount = 2;
                var cellsInBottomRowCount = 1;
                // x0
                // xx
                var firstCellOnTop = anotherCell.GetComponent<PositionComponent>().Value.x < emptyCell.GetComponent<PositionComponent>().Value.x;
                
                //   1 x 1  x 
                //() x 0 x (0)
                var anotherCellPossibleDestination = GetAnotherDiagonalDestinationFor(anotherCell, emptyCell);
                var nextTopCell = anotherCell;
                while (anotherCellPossibleDestination != null)
                {
                    cellsInBottomRowCount++;
                    nextTopCell = GetAnotherDiagonalSourceFor(anotherCellPossibleDestination, nextTopCell);
                    if (nextTopCell == null)
                    {
                        firstCellOnTop = !firstCellOnTop;
                        break;
                    }

                    cellsInTopRowCount++;
                    anotherCellPossibleDestination = GetAnotherDiagonalDestinationFor(nextTopCell, anotherCellPossibleDestination);
                }

                // 1 x 1  x  
                // x 0 x (0)
                anotherCellPossibleDestination = GetAnotherDiagonalDestinationFor(randomCell,
                    emptyCell);
                nextTopCell = anotherCell;
                while (anotherCellPossibleDestination != null)
                {
                    cellsInBottomRowCount++;
                    // 1 x 1 x ()  
                    // x 0 x 0
                    nextTopCell = GetAnotherDiagonalSourceFor(anotherCellPossibleDestination, nextTopCell);
                    if (nextTopCell == null) 
                        break;
                    cellsInTopRowCount++;
                    anotherCellPossibleDestination = GetAnotherDiagonalDestinationFor(nextTopCell, anotherCellPossibleDestination);
                }

                if (cellsInTopRowCount != cellsInBottomRowCount)
                {
                    Rain(randomCell, emptyCell, emptyCells);
                    return true;
                }

                Rain(BoardUtilities.GetNeighbourCellFor(emptyCell, cellsCache, (p) =>
                {
                    return firstCellOnTop ? p.GetTopLeftPosition() : p.GetTopRightPosition();
                })!, emptyCell, emptyCells);
                return true;
            }
            return false;
        }

        private Entity? GetAnotherDiagonalDestinationFor(Entity entity, Entity forbiddenDestination)
        {
            List<Entity> GetPossibleDiagonalDestinations(Entity entity)
            {
                var res = new List<Entity>();
                var bottomLeftCell = BoardUtilities.GetNeighbourCellFor(entity, cellsCache, (p) => p.GetBottomLeftPosition());
                if (bottomLeftCell != null && bottomLeftCell.GetComponent<SlotComponent>().IsEmpty)
                    res.Add(bottomLeftCell);
                var bottomRightCell = BoardUtilities.GetNeighbourCellFor(entity, cellsCache, (p) => p.GetBottomRightPosition());
                if (bottomRightCell != null && bottomRightCell.GetComponent<SlotComponent>().IsEmpty)
                    res.Add(bottomRightCell);
                return res;
            }
            
            return GetPossibleDiagonalDestinations(entity).FirstOrDefault(e => e != forbiddenDestination);
        }
        
        private Entity? GetAnotherDiagonalSourceFor(Entity entity, Entity forbiddenSource)
        {
            List<Entity> GetPossibleDiagonalSources(Entity entity)
            {
                var res = new List<Entity>();
                var topLeftCells = BoardUtilities.GetNeighbourCellFor(entity, cellsCache, (p) => p.GetTopLeftPosition());
                if (topLeftCells != null && topLeftCells.GetComponent<SlotComponent>().IsEmpty)
                    res.Add(topLeftCells);
                var topRightCell = BoardUtilities.GetNeighbourCellFor(entity, cellsCache, (p) => p.GetTopRightPosition());
                if (topRightCell != null && topRightCell.GetComponent<SlotComponent>().IsEmpty)
                    res.Add(topRightCell);
                return res;
            }
            
            return GetPossibleDiagonalSources(entity).FirstOrDefault(e => e != forbiddenSource);
        }
        
        private void Rain(Entity fromCell, Entity toCell, List<Entity> emptyCells)
        {
            emptyCells.Remove(toCell);
            emptyCells.Add(fromCell);
            ref var fromSlot = ref fromCell.GetComponent<SlotComponent>();
            var chipEntity = fromSlot.ChipEntity;
            
            ref var toSlot = ref toCell.GetComponent<SlotComponent>();
            toSlot.ChipEntity = chipEntity;

            fromSlot.ChipEntity = null;
            
            chipEntity.AddComponent<MovingComponent>();
            ref var chipPosition = ref chipEntity.GetComponent<PositionComponent>();
            var position = toCell.GetComponent<PositionComponent>().Value;
            chipPosition.Value = position;
            chipEntity.GetComponent<ChipComponent>().ChipView.MoveTo(position.x, position.y, MOVE_TIME, () =>
            {
                chipEntity.RemoveComponent<MovingComponent>();
                chipEntity.AddComponent<AnalyzeCommand>();
            });
        }
    }
}
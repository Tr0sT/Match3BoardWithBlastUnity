#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using Morpeh;
using Unity.Mathematics;

namespace Utilities
{
    public static class BoardUtilities
    {
        public static Dictionary<int2, Entity> CreateCellsCache(Filter cellFilter) => cellFilter.ToDictionary(cell => cell.GetComponent<PositionComponent>().Value);

        public static Entity? GetNeighbourCellFor(Entity entityCell, Dictionary<int2, Entity> cellsCache,  Func<PositionComponent, int2> positionTransformation)
        {
            var position = entityCell.GetComponent<PositionComponent>();

            var neighbourPosition = positionTransformation.Invoke(position);

            if (cellsCache.ContainsKey(neighbourPosition))
                return cellsCache[neighbourPosition];

            return null;
        }
    }
}
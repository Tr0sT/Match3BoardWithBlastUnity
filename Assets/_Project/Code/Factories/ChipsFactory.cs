#nullable enable
using System;
using System.Collections.Generic;
using Commands;
using Components;
using Morpeh;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using Utilities;
using Views;

namespace Controllers
{
    public class ChipsFactory
    {
        private readonly World world;
        private readonly IRandomizer randomizer;
        private readonly Dictionary<ChipComponent.ChipType, IObjectPool<ChipView>> pools = new();
        public ChipsFactory(World world, IRandomizer randomizer, List<ChipView> chipViewTemplates, Transform poolsParent, Transform boardParent)
        {
            this.world = world;
            this.randomizer = randomizer;
            
            foreach (var elementViewTemplate in chipViewTemplates)
            {
                var defaultCapacity = 10;
                var pool = new ObjectPool<ChipView>(() => 
                    GameObject.Instantiate(elementViewTemplate.gameObject, poolsParent).GetComponent<ChipView>(), 
                    view =>
                {
                    view.gameObject.SetActive(true);
                    view.transform.parent = boardParent;
                }, view =>
                    {
                        view.DeInit();
                        view.transform.parent = poolsParent;
                    }, view =>
                {
                    GameObject.Destroy(view.gameObject);
                }, true, defaultCapacity);
                var pollObjects = new List<ChipView>(defaultCapacity);
                for (var i = 0; i < defaultCapacity; i++)
                    pollObjects.Add(pool.Get());
                foreach (var pollObject in pollObjects)
                    pool.Release(pollObject);

                pools.Add(elementViewTemplate.ChipType, pool);
            }
        }

        public Entity GetRandom(int x, int y)
        {
            var randomType = (ChipComponent.ChipType)randomizer.Range(0, Enum.GetNames(typeof(ChipComponent.ChipType)).Length);
            var chipView = pools[randomType].Get();
            chipView.Init(x, y);
            
            var chipEntity = world.CreateEntity();
            chipEntity.SetComponent(new ChipComponent(randomType, chipView));
            chipEntity.SetComponent(new PositionComponent(new int2(x, y)));

            chipView.OnClick += () =>
            {
                if (chipEntity.Has<MovingComponent>())
                    return;
                chipEntity.AddComponent<BlastCommand>();
            }; 
            
            return chipEntity;
        }

        public void Release(Entity entity)
        {
            var chip = entity.GetComponent<ChipComponent>();
            pools[chip.Type].Release((ChipView)chip.ChipView);

            world.RemoveEntity(entity);
        }
    }
}
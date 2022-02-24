#nullable enable
using System.Collections.Generic;
using Components;
using Controllers;
using DG.Tweening;
using LeopotamGroup.Globals;
using Models;
using Morpeh;
using Systems;
using Unity.Mathematics;
using UnityEngine;
using Utilities;
using Views;

namespace Core
{
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private Transform poolTransform = null!;
        [SerializeField] private Transform boardTransform = null!;
        
        [SerializeField] private BoardScheme boardScheme = null!;
        
        [SerializeField] private List<CellView> cellViewTemplates = new();
        [SerializeField] private GeneratorCellView generatorCellViewTemplate = null!;
        
        [SerializeField] private List<ChipView> elementViewTemplates = new();

        private void Awake()
        {
            DOTween.SetTweensCapacity(200, 125);
            
            var randomizer = new Randomizer();
            Service<IRandomizer>.Set(randomizer);
            
            var world = World.Create();
            CreateBoard(world, boardScheme, boardTransform, cellViewTemplates, generatorCellViewTemplate);
            var chipViewsFactory = new ChipsFactory(world, randomizer, elementViewTemplates, poolTransform, boardTransform);
            
            var systemsGroup = world.CreateSystemsGroup();
            systemsGroup.AddSystem(ScriptableObject.CreateInstance<ChipsGenerationSystem>().Init(chipViewsFactory));
            systemsGroup.AddSystem(ScriptableObject.CreateInstance<ChipMovementSystem>().Init(randomizer));
            systemsGroup.AddSystem(ScriptableObject.CreateInstance<BlastSystem>());
            systemsGroup.AddSystem(ScriptableObject.CreateInstance<CooldownSystem>());
            systemsGroup.AddSystem(ScriptableObject.CreateInstance<InputSystem>());
            systemsGroup.AddSystem(ScriptableObject.CreateInstance<DestroySystem>().Init(chipViewsFactory));
            world.AddSystemsGroup(0, systemsGroup);
            world.UpdateByUnity = true;
        }

    #region BoardCreation
        private static void CreateBoard(World world, BoardScheme boardScheme, Transform parent, List<CellView> cellViewTemplates, GeneratorCellView generatorCellViewTemplate)
        {
            for (var x = 0; x < boardScheme.Width; x++)
            {
                for (var y = 0; y < boardScheme.Height; y++)
                {
                    if (!boardScheme.Cells[x, y])
                        continue;

                    CreateCell(world, x, y, cellViewTemplates, parent);
                }

                if (boardScheme.Cells[x, 0])
                    CreateGeneratorCell(world, x, -1, generatorCellViewTemplate, parent);
            }

            parent.localPosition = new Vector3((-boardScheme.Width + 1) / 2.0f * BaseCellView.Width, (boardScheme.Height  - 1) / 2.0f * BaseCellView.Height);
        }

        private static void CreateCell(World world, int x, int y, List<CellView> cellViewTemplates, Transform parent)
        {
            var cellViewTemplateType = (x + y) % cellViewTemplates.Count;
            var cellView = GameObject.Instantiate(cellViewTemplates[cellViewTemplateType].gameObject, parent.transform).GetComponent<CellView>();
            cellView.Init(x, y);
            
            var cellEntity = world.CreateEntity();
            cellEntity.SetComponent(new CellComponent(cellView));
            cellEntity.SetComponent(new PositionComponent(new int2(x, y)));
            cellEntity.AddComponent<SlotComponent>(); 
        }
        
        private static void CreateGeneratorCell(World world, int x, int y, GeneratorCellView generatorCellViewTemplate, Transform parent)
        {
            var generatorCellView = GameObject.Instantiate(generatorCellViewTemplate.gameObject, parent.transform).GetComponent<GeneratorCellView>();
            generatorCellView.Init(x, y);
            
            var cellEntity = world.CreateEntity();
            cellEntity.SetComponent(new CellComponent(generatorCellView));
            cellEntity.SetComponent(new PositionComponent(new int2(x, y)));
            cellEntity.AddComponent<SlotComponent>();
            cellEntity.AddComponent<ChipsGeneratorComponent>();
        }
        #endregion
    }
}
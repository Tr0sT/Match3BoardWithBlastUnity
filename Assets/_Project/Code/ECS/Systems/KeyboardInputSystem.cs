#nullable enable
using Commands;
using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Systems/" + nameof(InputSystem))]
    public sealed class InputSystem : UpdateSystem
    {
        public override void OnAwake()
        {
        }

        public override void OnUpdate(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                World.CreateEntity().AddComponent<GenerateCommand>();
            if (Input.GetKeyDown(KeyCode.Escape))
                World.CreateEntity().AddComponent<DestroyAllCommand>();
        }
    }
}
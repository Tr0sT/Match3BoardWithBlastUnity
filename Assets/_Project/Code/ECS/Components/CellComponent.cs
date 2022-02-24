#nullable enable
using Morpeh;
using Unity.IL2CPP.CompilerServices;
using Views;

namespace Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct CellComponent : IComponent
    {
        public BaseCellView CellView { get; }

        public CellComponent(BaseCellView cellView)
        {
            CellView = cellView;
        }
    }

}
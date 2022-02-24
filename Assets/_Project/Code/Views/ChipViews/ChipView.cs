#nullable enable
using Components;
using UnityEngine;

namespace Views
{
    public class ChipView : BaseChipView
    {
        [SerializeField] private ChipComponent.ChipType chipType;
        public ChipComponent.ChipType ChipType => chipType;
    }
}
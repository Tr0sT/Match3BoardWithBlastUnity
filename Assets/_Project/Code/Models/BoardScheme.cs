using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace Models
{
    [CreateAssetMenu(menuName = "Models/" + nameof(BoardScheme))]
    public class BoardScheme : SerializedScriptableObject
    {
        [TableMatrix(DrawElementMethod = nameof(DrawCell))]
        public readonly bool[,] Cells = null!;

        public int Width => Cells.GetLength(0);
        public int Height => Cells.GetLength(1);

        private static bool DrawCell(Rect rect, bool value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }

            EditorGUI.DrawRect(
                rect.Padding(1), 
                value ? 
                    new Color(0.1f, 0.8f, 0.2f) : 
                    new Color(0, 0, 0, 0.5f));
            
            return value;
        }
    }
}
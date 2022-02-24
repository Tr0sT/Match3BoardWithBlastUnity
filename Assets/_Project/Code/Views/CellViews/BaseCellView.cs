using UnityEngine;

#nullable enable
namespace Views
{
    public abstract class BaseCellView : MonoBehaviour 
    {
        public const int Width = 180 / 3;
        public const int Height = 180 / 3;

        public virtual void Init(int x, int y)
        {
            transform.localPosition = new Vector3(Width * x, -Height * y);
            gameObject.SetActive(true);
        }
    }
}
using UnityEngine;

namespace Engine.Grid
{
    public class Cell : MonoBehaviour
    {
        public Stickman.Stickman objectOn;
        public int index;

        public enum StickmanCellType
        {
            Forward,
            Back
        }
    }
}
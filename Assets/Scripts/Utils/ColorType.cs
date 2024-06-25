using System;

namespace Utils
{
    [Serializable]
    public class StickmanData
    {
        public ColorType colorType;
        public int cellIndex;
        public bool isMystery;
        public bool isYuplenen;
    }

    public enum ColorType
    {
        Green,
        Yellow,
        Red,
        OpenBlue,
        Pink,
        BetweenYellowAndRed,
        None,
    }
}
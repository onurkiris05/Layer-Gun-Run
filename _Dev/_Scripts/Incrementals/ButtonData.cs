using System;

namespace Game.Incrementals
{
    [Serializable]
    public class ButtonData
    {
        public ButtonType Type;
        public ButtonStat[] Stats;
    }

    [Serializable]
    public class ButtonStat
    {
        public int Level;
        public int Cost;
        public int Amount;
    }

    public enum ButtonType
    {
        Power,
        Rate,
        Range
    }
}
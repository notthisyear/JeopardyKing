using System;

namespace JeopardyKing.GameComponents
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GameAnswerModeTypeAttribute : Attribute
    {
        public string DisplayText { get; }

        public string ToolTip { get; }

        public GameAnswerModeTypeAttribute(string displayText, string toolTip)
        {
            DisplayText = displayText;
            ToolTip = toolTip;
        }
    }
    public enum GameAnswerMode
    {
        [GameAnswerModeType("Delayed", "Block input until the game manager allows it")]
        BlockUntilSet,

        [GameAnswerModeType("Immediate", "Allow input as soon as the question is shown")]
        AllowImmediately
    }
}

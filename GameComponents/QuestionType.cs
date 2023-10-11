using System;

namespace JeopardyKing.GameComponents
{
    [AttributeUsage(AttributeTargets.Field)]
    public class QuestionTypeInfoAttribute : Attribute
    {
        public string DisplayText { get; }

        public QuestionTypeInfoAttribute(string displayText)
        {
            DisplayText = displayText;
        }
    }
    public enum QuestionType
    {
        [QuestionTypeInfo("Text")]
        Text,

        [QuestionTypeInfo("Image")]
        Image,

        [QuestionTypeInfo("Audio")]
        Audio,

        [QuestionTypeInfo("Video")]
        Video,

        [QuestionTypeInfo("YouTube video")]
        YoutubeVideo
    }
}

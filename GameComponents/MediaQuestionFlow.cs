using System;

namespace JeopardyKing.GameComponents
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MediaQuestionFlowInfoAttribute : Attribute
    {
        public string DisplayText { get; }

        public MediaQuestionFlowInfoAttribute(string displayText)
        {
            DisplayText = displayText;
        }
    }
    public enum MediaQuestionFlow
    {
        [MediaQuestionFlowInfo("None")]
        None,

        [MediaQuestionFlowInfo("First text, then media")]
        TextThenMedia,

        [MediaQuestionFlowInfo("First media, then text")]
        MediaThenText,

        [MediaQuestionFlowInfo("Media and text simultaneously")]
        MediaAndText
    }
}

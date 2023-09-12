using System.Collections.Generic;
using System.Windows.Media;

namespace JeopardyKing.WpfComponents
{
    public enum IconType
    {
        None,
        Text,
        Audio,
        Image,
        Video,
        Star,
        Play,
        Pause
    }

    public static class IconDataProvider
    {
        private const string TextIconPathData = "m 22 3 h -14 c -2.76 0 -5 2.24 -5 5 v 14 c 0 2.76 2.24 5 5 5 h 14 c 2.76 0 5 -2.24 5 -5 v -14 c 0 -2.76 -2.24 -5 -5 -5 z m 3 19 c 0 1.65 -1.35 3 -3 3 h -14 c -1.65 0 -3 -1.35 -3 -3 v -14 c 0 -1.65 1.35 -3 3 -3 h 14 c 1.65 0 3 1.35 3 3 v 14 z m -17 -9 c 0 -0.55 0.45 -1 1 -1 h 9 c 0.55 0 1 0.45 1 1 s -0.45 1 -1 1 h -9 c -0.55 0 -1 -0.45 -1 -1 z m 0 -4 c 0 -0.55 0.45 -1 1 -1 h 6 c 0.55 0 1 0.45 1 1 s -0.45 1 -1 1 h -6 c -0.55 0 -1 -0.45 -1 -1 z m 14 8 c 0 0.55 -0.45 1 -1 1 h -12 c -0.55 0 -1 -0.45 -1 -1 s 0.45 -1 1 -1 h 12 c 0.55 0 1 0.45 1 1 z m -9 4 c 0 0.55 -0.45 1 -1 1 h -3 c -0.55 0 -1 -0.45 -1 -1 s 0.45 -1 1 -1 h 3 c 0.55 0 1 0.45 1 1 z";
        private const string AudioIconPathData = "m 25.5 3.5 a 2.992 2.992 0 0 0 -2.469 -0.638 l -11.999 2.247 a 3 3 0 0 0 -2.448 2.951 v 11.305 a 3.959 3.959 0 0 0 -2 -0.556 a 4 4 0 1 0 4 4 v -10.58 l 14 -2.62 v 6.761 a 3.959 3.959 0 0 0 -2 -0.561 a 4 4 0 1 0 4 4 v -14 a 3 3 0 0 0 -1.084 -2.309 z";
        private const string ImageIconPathData = "m 22.5 3 h -15 a 4.505 4.505 0 0 0 -4.5 4.5 v 15 a 4.505 4.505 0 0 0 4.5 4.5 h 15 a 4.505 4.505 0 0 0 4.5 -4.5 v -15 a 4.505 4.505 0 0 0 -4.5 -4.5 z m -15 3 h 15 a 1.5 1.5 0 0 1 1.5 1.5 v 15 a 1.492 1.492 0 0 1 -0.44 1.06 l -8.732 -8.732 a 4 4 0 0 0 -5.656 0 l -3.172 3.172 v -10.5 a 1.5 1.5 0 0 1 1.5 -1.5 z m 11.5 4.5 m 2.5 0 a 2.5 2.5 0 1 0 -5 0 a 2.5 2.5 0 1 0 5 0";
        private const string VideoIconPathData = "m 22 4 h -14 c -2.757 0 -5 2.243 -5 5 v 12 c 0 2.757 2.243 5 5 5 h 14 c 2.757 0 5 -2.243 5 -5 v -12 c 0 -2.757 -2.243 -5 -5 -5 z m 3 6 h -3.894 l 3.066 -3.066 c 0.512 0.538 0.828 1.266 0.828 2.066 v 1 z m -2.734 -3.988 l -3.973 3.973 s -0.009 0.01 -0.014 0.015 h -3.423 l 4 -4 h 3.144 c 0.09 0 0.178 0.005 0.266 0.012 z m -6.238 -0.012 l -3.764 3.764 c -0.071 0.071 -0.13 0.151 -0.175 0.236 h -3.483 l 4 -4 h 3.422 z m -8.028 0 h 1.778 l -4 4 h -0.778 v -1 c 0 -1.654 1.346 -3 3 -3 z m 14 18 h -14 c -1.654 0 -3 -1.346 -3 -3 v -9 h 20 v 9 c 0 1.654 -1.346 3 -3 3 z m -3.953 -5.2 l -4.634 2.48 c -0.622 0.373 -1.413 -0.075 -1.413 -0.8 v -4.961 c 0 -0.725 0.791 -1.173 1.413 -0.8 l 4.634 2.48 c 0.604 0.362 0.604 1.238 0 1.6 z";
        private const string StarIconPathData = "m 22.5 26.5 l -7.467 -5.488 l -7.467 5.488 l 2.867 -8.863 l -7.463 -5.453 h 9.214 l 2.849 -8.878 l 2.849 8.878 h 9.213 l -7.462 5.453 z";
        private const string PlayIconPathData = "m 27 18.8 l -19.2474 10.545 c -2.8342 1.5947 -6.3344 -0.4551 -6.3344 -3.7037 v -21.0863 c 0 -3.2523 3.5002 -5.2984 6.3344 -3.7037 l 19.2474 10.545 c 2.8897 1.6243 2.8897 5.7831 0 7.4074 z";
        private const string PauseIconPathData = "m 8.125 0 a 4.375 4.375 90 0 0 -4.375 4.375 v 21.25 a 4.375 4.375 90 0 0 8.75 0 v -21.25 a 4.375 4.375 90 0 0 -4.375 -4.375 z m 13.75 0 a 4.375 4.375 90 0 0 -4.375 4.375 v 21.25 a 4.375 4.375 90 0 0 8.75 0 v -21.25 a 4.375 4.375 90 0 0 -4.375 -4.375 z";

        private static readonly Dictionary<IconType, Geometry> s_iconGeometryMap = new()
        {
            { IconType.None, Geometry.Parse(string.Empty) },
            { IconType.Text, Geometry.Parse(TextIconPathData) },
            { IconType.Audio, Geometry.Parse(AudioIconPathData) },
            { IconType.Image, Geometry.Parse(ImageIconPathData) },
            { IconType.Video, Geometry.Parse(VideoIconPathData) },
            { IconType.Star, Geometry.Parse(StarIconPathData) },
            { IconType.Play, Geometry.Parse(PlayIconPathData) },
            { IconType.Pause, Geometry.Parse(PauseIconPathData) }
        };

        public static Geometry GetDataForIcon(IconType type)
        {
            if (s_iconGeometryMap.TryGetValue(type, out var geometry))
                return geometry;
            return s_iconGeometryMap[IconType.None];
        }
    }
}

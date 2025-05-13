using TwitchDownloaderCore.Tools;

namespace TwitchDownloaderWPF.Utils
{
    public static class CommUtils
    {
        public static long ValidateUrl(string text)
        {
            var vodIdMatch = IdParse.MatchVideoId(text);
            if (vodIdMatch is { Success: true } && long.TryParse(vodIdMatch.ValueSpan, out var vodId))
            {
                return vodId;
            }

            return -1;
        }

    }
}

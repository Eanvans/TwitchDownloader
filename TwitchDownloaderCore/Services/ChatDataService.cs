namespace TwitchDownloaderCore.Services
{
    public static class ChatDataServices{
        
        public static void ExtractHotTimeline(ChatRoot root){
            
        }


        // use iqr analyze the hot time interval
        // 1. re-sort the comments count in 5mins interval decending
        // 2. pick the first 25% as Q1
        // 3. pick the first 75% as Q3
        // 4. iqr = Q3 - Q1
        // 4. use Q3 + 1.5*iqr as the baseline to extract the stream hot time interval
        private static void IqrAnalyze(List<>){

        }
    }
}
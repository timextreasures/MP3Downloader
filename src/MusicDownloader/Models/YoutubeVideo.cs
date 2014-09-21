namespace MusicDownloader.Models
{
    public class YoutubeVideo
    {
        public YoutubeVideo(string videoId, string title)
        {
            VideoUrl = "http://www.youtube.com/watch?v=" + videoId;
            Title = title;
        }

        public string Title { get; set; }
        public string VideoUrl { get; set; }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MusicDownloader.Models;

namespace MusicDownloader.Data
{
    /// <summary>
    ///     YouTube Data API v3 sample: search by keyword.
    ///     Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    ///     See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
    ///     Set ApiKey to the API key value from the APIs & auth > Registered apps tab of
    ///     https://cloud.google.com/console
    ///     Please ensure that you have enabled the YouTube Data API for your project.
    /// </summary>
    public class YouTube
    {
        public async Task<List<YoutubeVideo>> SearchYoutubeVideo(string query)
        {
            YouTubeService youTubeService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = "AIzaSyD9ayV0qmeljrKYzIOftCTrj_sV1221lHI",
                ApplicationName = "YoutubeDownloader"
            });

            var searchListRequest = youTubeService.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = 30;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            return (from item in searchListResponse.Items
                where item.Id.Kind == "youtube#video"
                select new YoutubeVideo(item.Id.VideoId, item.Snippet.Title)).ToList();
        }
    }
}
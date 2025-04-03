using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Diagnostics;

public static class YouTubeApiPlayer
{
    const string API_KEY = "MY_KEY";

    public static void PlayRandomMusic()
    {
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = API_KEY,
            ApplicationName = "SomeForMyProject"
        });

        var searchRequest = youtubeService.Search.List("snippet");
        searchRequest.Q = "music playlist";
        searchRequest.Type = "playlist";
        searchRequest.MaxResults = 50;

        var searchResponse = searchRequest.Execute();

        if (searchResponse.Items.Count > 0)
        {
            Random random = new Random();
            var randomPlaylist = searchResponse.Items[random.Next(searchResponse.Items.Count)];
            string playlistUrl = $"https://www.youtube.com/playlist?list={randomPlaylist.Id.PlaylistId}";

            Process.Start(new ProcessStartInfo
            {
                FileName = playlistUrl,
                UseShellExecute = true
            });
        }
    }
}
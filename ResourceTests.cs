using ArtistStats_web.Helpers;
using ArtistStats_web.Models;
using System;
using System.Collections.Generic;
using Xunit;

/************************************************************************************
 Solution fetches expected resources and parses them into corresponding objects
************************************************************************************/
namespace ArtistStats.Test
{
    public class ResourceTests
    {
        public static IEnumerable<object[]> GetReleaseTestData()
        {
            string fileWithValues = "data/release-pac-againstworld-1";
            string fileWithNoValues = "data/release-pac-againstworld-2";
            string track = "Temptations";

            var releases = TestHelper.GetFileData(fileWithValues);
            var noReleases = TestHelper.GetFileData(fileWithNoValues);

            yield return new object[] { releases, track };
            yield return new object[] { noReleases, track };

            //TODO: avoids picking duplicate records for final track repository
        }
        [Theory]
        [InlineData("data/release-pac-againstworld-1.json", "Temptations")]
        [InlineData("data/release-pac-againstworld-2.json", "Temptations")]
        public void ParsesARelease(string filePath, string expectedTrack5)
        {
            var json = TestHelper.GetFileData(filePath);
            var sut = new DeserializeJson<Release>();
            var release = sut.Deserialize(json);
            Assert.Equal(release.Media[0].Tracks[4].Title, expectedTrack5);
        }
        [Theory]
        [InlineData("data/artists-query-2pac.json", "382f1005-e9ab-4684-afd4-0bdae4ee37f2")]
        public void ParsesArtistResults(string filePath, string ArtistID)
        {
            var json = TestHelper.GetFileData(filePath);

            var sut = new DeserializeJson<ArtistResults>();

            ArtistResults artists = sut.Deserialize(json);

            Assert.Equal(28, artists.Count);
            Assert.Equal(25, artists.Artists.Length);
            Assert.Equal(ArtistID, artists.Artists[1].ID);
            Assert.Equal("92a4d187-168d-4422-8d04-d194bea5da47", artists.Artists[0].ID);
            Assert.Equal("92a4d187-168d-4422-8d04-d194bea5da47", artists.Artists[0].ID);
        }
        /// <summary>
        /// When an artist name is saught after, the results back can contain 0 or more releases.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetReleasesTestData()
        {
            string fileWithValues = "data/releases-artist-2pac.json";
            string fileWithNoValues = "data/releases-artist-tupac-shakur.json";

            var releases = TestHelper.GetFileData(fileWithValues);
            var noReleases = TestHelper.GetFileData(fileWithNoValues);

            yield return new object[] { releases, true };
            yield return new object[] { noReleases, false };
            //TODO: avoids picking an artist with no releases (moq combined with a 'Selector' Class) This test will be in SelectionTests.cs SelectsValidArtist
        }
        [Theory]
        [MemberData(nameof(GetReleasesTestData))]
        public void ParsesReleaseResults(string json, bool hasResults)
        {
            var sut = new DeserializeJson<ReleaseResults>();

            ReleaseResults releases = sut.Deserialize(json);
            bool releasesExist = releases.Count > 0;
            Assert.Equal(releasesExist, hasResults);
            if (releasesExist)
            {
                for (int i = 0; i < releases.Releases.Count; i++)
                {
                    var cmd = ArtistStats_web.Commands.GetReleasesByArtistsIDAsync.FillTracksAsyncCommands(releases.Releases[i], new ArtistStats_web.Services.MusicStatService());
                    var tracksCmd = cmd.ExecuteAsync();
                    tracksCmd.Wait();
                    var media = cmd.Release.Media;
/*                    Assert.NotNull(cmd.Release.Media);
                    Assert.NotNull(cmd.Release.Media[0].Tracks[0]);*/
                }
            }

            //TODO: add checks to compare IDs, counts, etc against what's in the file..
        }
        /*[Theory]
        [MemberData(nameof(GetLyricsTestData))]
        public void ParsesTrackLyrics(string filePath)
        {
            var json = TestHelper.GetFileData(filePath);
            Assert.False(true);
        }
        [Theory]
        [InlineData("data/artists-query-2pac.json")]
        public void SumsTracksLyrics(string filePath)
        {
            var json = TestHelper.GetFileData(filePath);
            Assert.False(true);
        }
        [Theory]
        [InlineData("data/artists-query-2pac.json")]
        public void CalculatesAverage(string filePath)
        {
            var json = TestHelper.GetFileData(filePath);
            Assert.False(true);
        }*/
    }
}
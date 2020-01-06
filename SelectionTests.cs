using Xunit;
using ArtistStats_web.Models;
using System.Collections.Generic;
using System.Linq;
using ArtistStats_web.Helpers;
using ArtistStats_web.Services;
using ArtistStats_web.Commands;
using System.Threading.Tasks;
using Moq;
using System;
/************************************************************************************
Solution selects the best options regards to an artist or a release:
- Artists must a) match with the input artist & b) have the most relea ses if duplicates exist for the artist or 
c) if no duplicates exist and the match doesn't have any releases select the second option (based on the sorting of score)
- Releases must a) have the most tracks if there are duplicates and b) their artist matches with the input artist 
* TODO: Add fake record to an album to show a merge between two albums with identical names
* TODO: avoids picking duplicate records for final track repository
* TODO: Test if a valid artist has releases?
* ************************************************************************************/
namespace ArtistStats.Test
{
    public class SelectionTests
    {
        public static IEnumerable<object[]> GetDuplicatesTestParams()
        {
            string artistName = "Tupac shakur";
            var mockService = new Mock<IMusicStatService>();
            var realService = new MusicStatService();
            Func<object, string> getReleasesFileData = (object path) =>
            {
                return TestHelper.GetFileData((string)path);
            };
            Func<object, string> getTracksData = (object releaseID) =>
            {
                Task<string> tracksTask = realService.getTracks((string)releaseID);
                tracksTask.Wait();
                return tracksTask.Result;
            };
            Func<object, object, Task<string>> getTracksLyrics = (object artistName, object trackTitle) =>
            {
                return realService.getLyrics((string)artistName, (string)trackTitle);
            };

            mockService.Setup(mss => mss.getArtists(artistName))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/artists-query-2pac.json"));
            mockService.Setup(mss => mss.getReleases("92a4d187-168d-4422-8d04-d194bea5da47"))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/releases-artist-tupac-shakur.json"));
            mockService.Setup(mss => mss.getReleases("382f1005-e9ab-4684-afd4-0bdae4ee37f2"))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/releases-artist-2pac.json"));

            mockService.Setup((mss) => mss.getTracks(It.IsAny<string>()))
                 .Returns<string>((s) => Task<string>.Factory.StartNew(getTracksData, s));
            mockService.Setup((mss) => mss.getLyrics(It.IsAny<string>(), It.IsAny<string>()))
                 .Returns<string, string>(async (artistName, trackTitle) => await await Task.Factory.StartNew(() => getTracksLyrics(artistName, trackTitle)));
            var mock = mockService.Object;
            var real = new MusicStatService();
            yield return new object[] { mock, artistName };
        }
        public static IEnumerable<object[]> GetTracksAsyncTestParams()
        {
            string artistName = "Tupac shakur";
            var mockService = new Mock<IMusicStatService>();
            var realService = new MusicStatService();
            Func<object, string> getReleasesFileData = (object path) =>
            {
                return TestHelper.GetFileData((string)path);
            };
            Func<object, string> getTracksData = (object releaseID) =>
            {
                Task<string> tracksTask = realService.getTracks((string)releaseID);
                tracksTask.Wait();
                return tracksTask.Result;
            };
            Func<object, object, Task<string>> getTracksLyrics = (object artistName, object trackTitle) =>
            {
                return realService.getLyrics((string)artistName, (string)trackTitle);
            };

            mockService.Setup(mss => mss.getArtists(artistName))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/artists-query-2pac.json"));
            mockService.Setup(mss => mss.getReleases("92a4d187-168d-4422-8d04-d194bea5da47"))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/releases-artist-tupac-shakur.json"));
            mockService.Setup(mss => mss.getReleases("382f1005-e9ab-4684-afd4-0bdae4ee37f2"))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/releases-artist-2pac.json"));

            mockService.Setup((mss) => mss.getTracks(It.IsAny<string>()))
                 .Returns<string>((s) => Task<string>.Factory.StartNew(getTracksData, s));
            mockService.Setup((mss) => mss.getLyrics(It.IsAny<string>(), It.IsAny<string>()))
                 .Returns<string, string>(async (artistName, trackTitle) => await await Task.Factory.StartNew(() => getTracksLyrics(artistName, trackTitle)));
            var mock = mockService.Object;
            Task<string> artistsResultsTask = mock.getArtists(artistName);
            artistsResultsTask.Wait();
            string artistsResultsJson = artistsResultsTask.Result;
            DeserializeJson<ArtistResults> deserializeJson = new DeserializeJson<ArtistResults>();
            ArtistResults artistResults = deserializeJson.Deserialize(artistsResultsJson);
            var real = new MusicStatService();
            yield return new object[] { mock, artistResults.Artists[1] };
        }
        public static IEnumerable<object[]> GetAveraveWordsTestParams()
        {
            string artistName = "Tupac shakur";
            var mockService = new Mock<IMusicStatService>();
            var realService = new MusicStatService();
            Func<object, string> getReleasesFileData = (object path) =>
            {
                return TestHelper.GetFileData((string)path);
            };
            Func<object, string> getTracksData = (object releaseID) =>
            {
                Task<string> tracksTask = realService.getTracks((string)releaseID);
                tracksTask.Wait();
                return tracksTask.Result;
            };
            Func<object, object, Task<string>> getTracksLyrics = (object artistName, object trackTitle) =>
            {
                return realService.getLyrics((string)artistName, (string)trackTitle);
            };

            mockService.Setup(mss => mss.getArtists(artistName))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/artists-query-2pac.json"));
            mockService.Setup(mss => mss.getReleases("92a4d187-168d-4422-8d04-d194bea5da47"))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/releases-artist-tupac-shakur.json"));
            mockService.Setup(mss => mss.getReleases("382f1005-e9ab-4684-afd4-0bdae4ee37f2"))
                 .Returns(Task<string>.Factory.StartNew(getReleasesFileData, "data/releases-artist-2pac.json"));

            mockService.Setup((mss) => mss.getTracks(It.IsAny<string>()))
                 .Returns<string>((s) => Task<string>.Factory.StartNew(getTracksData, s));
            mockService.Setup((mss) => mss.getLyrics(It.IsAny<string>(), It.IsAny<string>()))
                 .Returns<string, string>(async (artistName, trackTitle) => await await Task.Factory.StartNew(() => getTracksLyrics(artistName, trackTitle)));
            var mock = mockService.Object;
            var real = new MusicStatService();
            yield return new object[] { mock, artistName };
            artistName = "Drake";
            yield return new object[] { real, artistName };
        }
        [Theory]
        [MemberData(nameof(GetAveraveWordsTestParams))]
        public void getsAndCalculatesAverageWords(IMusicStatService service, string artistName)
        {
            MusicInterpretor sut = new MusicInterpretor(service);
            Artist artist = sut.PickArtist(artistName);
            var average = sut.calculateArtistStats(artist);
            Assert.True(average.AverageWords > 0);
        }
        [Fact]
        public void getsStdDev()
        {

        }
        [Fact]
        public void SelectsValidArtist()
        {
            //TODO: avoids picking an artist with no releases 
            //Picks most releases if duplicates exist or
            //if no duplicates exist and 
            //the first match has no releases
            MusicInterpretor sut = new MusicInterpretor(new MusicStatService());
            var artist = sut.PickArtist("Tupac Shakur");
            Assert.False(artist == null);
            Assert.False(artist.Releases.Length == 0);
        }
        [Theory]
        [MemberData(nameof(GetTracksAsyncTestParams))]
        public void GetsTrackAsync(IMusicStatService service, Artist artist)
        {
            GetReleasesByArtistsIDAsyncFactory getReleasesByArtistsIDAsync = new GetReleasesByArtistsIDAsyncFactory(artist, service);
            Command getReleasesByArtistsIDAsync1 = getReleasesByArtistsIDAsync.GetCommand();
            getReleasesByArtistsIDAsync1.ExecuteAsync().Wait();
            GetReleasesByArtistsIDAsync cmd = (GetReleasesByArtistsIDAsync)getReleasesByArtistsIDAsync1;
            var tracks = cmd.ReleaseResults.Releases.SelectMany(r => r.Media[0].Tracks).ToList();
            Assert.True(tracks.Count > 0);
        }
        [Theory]
        [MemberData(nameof(GetDuplicatesTestParams))]
        public void FiltersDuplicateTracksFromReleases(IMusicStatService service, string artistName)
        {
            MusicInterpretor sut = new MusicInterpretor(service);
            var artist = sut.PickArtist(artistName);
            var releases = new List<Release>(artist.Releases);
            List<Track> uniqueTracks = releases.SelectMany(x => x.Media[0].Tracks).ToList();
            //??
            
            for (int i = 0; i < releases.Count; i++)
            {

            }
            /*for (int i = 0; i < rawTracks.Count; i++)
            {
                Track verification = uniqueTracks.Find(x => x.Title == rawTracks[i].Title);
                Assert.NotNull(verification);
            }*/
        }
        [Fact]
        public void SelectsValidReleasesWithTracks()
        {
            string fileWithValues = "data/releases-artist-2pac.json";
            var values = TestHelper.GetFileData(fileWithValues);
            var deserializeJson = new DeserializeJson<ReleaseResults>();
            var releaseResults = deserializeJson.Deserialize(values);
            MusicStatService sut = new MusicStatService();
            MusicInterpretor sut2 = new MusicInterpretor(sut);
            List<Release> Releases = new List<Release>();
            for (int i = 0; i < releaseResults.Releases.Count; i++)
            {
                CommandFactory commandFactory = new GetTracksByReleaseIDFactory(releaseResults.Releases[i], sut);
                Command command = commandFactory.GetCommand();
                Task t = command.ExecuteAsync();
                t.ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        GetTracksByReleaseID getTracksByReleaseID = (GetTracksByReleaseID)command;//doesn't filter non-unique tracks
                        Releases.Add(getTracksByReleaseID.Release);
                    }
                    else
                    {
                        Assert.False(true);
                    }
                });
            }
            var totalTracks = Releases.Where(x => (x != null && x.Media != null)).SelectMany(x => x.Media).SelectMany(x => x.Tracks).ToList();

            Assert.Equal(13, Releases.Count);
            Assert.True(totalTracks.Count > 0);//why are we gettin random tracks back?

        }
        [Fact]
        public void SelectsValidReleasesWithTracksWithMultiThread()
        {

            MusicStatService sut = new MusicStatService();
            MusicInterpretor sut2 = new MusicInterpretor(sut);
            List<Release> Releases = new List<Release>();
            /*
                        CommandFactory commandFactory = new GetReleasesByArtistsIDFactory("382f1005-e9ab-4684-afd4-0bdae4ee37f2", sut);
                        Command command = commandFactory.GetCommand();
                        command.ExecuteAsync().Wait();
                        GetReleasesByArtistsID getTracksByReleaseID = (GetReleasesByArtistsID)command;//doesn't filter non-unique tracks
                        Releases = getTracksByReleaseID.ReleaseResults.Releases;

                        var totalTracks = Releases.Where(x => (x != null && x.Media != null)).SelectMany(x => x.Media).SelectMany(x => x.Tracks).ToList();

                        Assert.Equal(14, Releases.Count);
                        Assert.True(totalTracks.Count > 0);//why are we gettin random tracks back?
            */
        }
    }

}
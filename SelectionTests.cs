using Xunit;
using ArtistStats_web.Models;
using System.Collections.Generic;
using System.Linq;
using ArtistStats_web.Helpers;
using ArtistStats_web.Services;
using ArtistStats_web.Commands;
using System.Threading.Tasks;
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
        [Fact]
        public void SelectsValidReleases()
        {
            string fileWithValues = "data/releases-artist-2pac.json";
            var values = TestHelper.GetFileData(fileWithValues);
            var deserializeJson = new DeserializeJson<ReleaseResults>();
            var releaseResults = deserializeJson.Deserialize(values);
            MusicStatService sut = new MusicStatService();
            MusicInterpretor sut2 = new MusicInterpretor(sut);
            List<Release> enhancedReleases = new List<Release>();
            for (int i = 0; i < releaseResults.Releases.Count; i++)
            {
                CommandFactory commandFactory = new GetTracksByReleaseIDFactory(releaseResults.Releases[i].ID, sut);
                Command command = commandFactory.GetCommand();
                Task t = command.ExecuteAsync();
                t.Wait();
                GetTracksByReleaseID getTracksByReleaseID = (GetTracksByReleaseID)command;//doesn't filter non-unique tracks
                if (getTracksByReleaseID.Release == null)
                    continue;
                var uniqueTracks = getTracksByReleaseID.Release.Media[0].Tracks;
                var duplicates = uniqueTracks.GroupBy(x => x.Title)
                              .Where(g => g.Count() > 1)
                              .Select(y => y)
                              .ToList();
                Assert.Empty(duplicates);
                enhancedReleases.Add(getTracksByReleaseID.Release);
                /*var temp = releaseResults.Releases[i].Media[0];
                temp.TrackCount = uniqueTracks.Count;
                temp.Tracks = new List<Track>(uniqueTracks);
                releaseResults.Releases[i].Media[0] = temp;
                enhancedReleases.Add(releaseResults.Releases[i]);*/
            }
            var totalTracks = enhancedReleases.SelectMany(x => x.Media).SelectMany(x => x.Tracks).ToList();

            Assert.Equal(25, enhancedReleases.Count);

        }
    }

}
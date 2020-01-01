using System;
using Xunit;
using Moq;
using ArtistStats_web.Models;
using System.Collections.Generic;
using System.Linq;
using ArtistStats_web.Helpers;
using System.Threading.Tasks;
using ArtistStats_web.Services;
/************************************************************************************
Solution selects the best options regards to an artist or a release:
- Artists must a) match with the input artist & b) have the most relea ses if duplicates exist for the artist or 
c) if no duplicates exist and the match doesn't have any releases select the second option (based on the sorting of score)
- Releases must a) have the most tracks if there are duplicates and b) their artist matches with the input artist 
************************************************************************************/
namespace ArtistStats.Test
{
    public class SelectionTests
    {
        [Theory]
        [InlineData("hot hot hot")]
        public void SelectsValidArtist(string dummy)
        {
            //TODO: avoids picking an artist with no releases 
            //Picks most releases if duplicates exist or
            //if no duplicates exist and 
            //the first match has no releases
            MusicInterpretor sut = new MusicInterpretor(new ArtistStats_web.Services.MusicStatService());
            var artist = sut.PickArtist("Tupac Shakur");
            artist.Wait();
            Assert.False(artist.Result.Releases.Length == 0);
        }
        [Fact]
        public void SelectsValidReleases()
        {
            string fileWithValues = "data/releases-artist-2pac.json";
            var values = TestHelper.GetFileData(fileWithValues);
            var deserializeJson = new DeserializeJson<ReleaseResults>();
            var releaseResults = deserializeJson.Deserialize(values);//Add fake record to an album to show a merge between two albums with identical names
            //TODO: avoids picking duplicate records for final track repository
            MusicInterpretor sut = new MusicInterpretor(new ArtistStats_web.Services.MusicStatService());
            var filteredReleases = sut.FilterDuplicates(releaseResults.Releases);
            Assert.False(filteredReleases.Count == 0);

        }
    }
    public class MusicInterpretor
    {
        private MusicStatService _service;
        public Track[] GetTracks(Artist artist)
        {
            List<Track> tracks = new List<Track>();
            List<Task<List<Track>>> tasks = new List<Task<List<Track>>>();
            async Task<List<Track>> getTrackFromReleaseID(string releaseId, ArtistStats_web.Services.MusicStatService service)
            {
                var fetchTracks = new GetTracksByReleaseID(releaseId, service);
                var tracks = await fetchTracks.Execute();
                return tracks;
            }
            for (int i = 0; i < artist.Releases.Length; i++)
            {
                getTrackFromReleaseID(artist.Releases[i].ID, _service).Wait();
                var t = getTrackFromReleaseID(artist.Releases[i].ID, _service).Result;
                for (int j = 0; j < t.Count; j++)
                {
                    tracks.Add(t[j]);
                }
            }
            return tracks.ToArray();
        }
        public MusicInterpretor(ArtistStats_web.Services.MusicStatService service)
        {
            _service = service;
        }
        /// <summary>
        /// Filters a list of releases if they contain duplicates; picking releases that contain the majority of tracks. 
        /// </summary>
        /// <param name="rawReleases"></param>
        /// <returns></returns>
        public List<Release> FilterDuplicates(List<Release> releases)
        {
            var duplicates = releases.GroupBy(x => x.Title)
                                        .Where(g => g.Count() > 1)
                                        .Select(y => y)
                                        .ToList();
            return duplicates.SelectMany(x => x).ToList();
        }
        public async Task<Artist> PickArtist(string artistName)
        {
            var getArtistsByName = new GetArtistsByName(artistName, _service);
            Artist[] artistsResults = await getArtistsByName.Execute();
            GetReleasesByArtistsID artistsFetcher = new GetReleasesByArtistsID(artistsResults[0].ID, _service);
            var releases = await artistsFetcher.Execute();
            
            bool firstArtistHasreleases = releases.Count > 0;
            if (firstArtistHasreleases)
            {
                artistsResults[0].Releases = releases.ToArray();
                return artistsResults[0];
            }
            else
            {
                artistsFetcher = new GetReleasesByArtistsID(artistsResults[1].ID, _service);
                releases = await artistsFetcher.Execute();
                if(releases.Count > 0)
                {
                    artistsResults[1].Releases = releases.ToArray();
                    return artistsResults[1];
                }
            }
            throw new Exception("No Release data found on top 2 artists");
        }
    }
    public class GetTracksByReleaseID
    {
        private string _releaseID;

        private ArtistStats_web.Services.MusicStatService _service;
        public GetTracksByReleaseID(string releaseID, ArtistStats_web.Services.MusicStatService service)
        {
            _releaseID = releaseID;
            _service = service;
        }
        public async Task<List<Track>> Execute()
        {
            string releasesJson = await _service.getReleases(_releaseID);
            Release releaseResults = new DeserializeJson<Release>().Deserialize(releasesJson);
            List<Track> tracks = new List<Track>();
            for (int i = 0; i < releaseResults.Media.Count; i++)
            {
                for (int j = 0; j < releaseResults.Media[i].Tracks.Count; j++)
                {
                    tracks.Add(releaseResults.Media[i].Tracks[j]);
                }
            }
            return tracks;
        }
    }
    public class GetReleasesByArtistsID
    {
        private string _artistID;

        private ArtistStats_web.Services.MusicStatService _service;
        public GetReleasesByArtistsID(string artistID, ArtistStats_web.Services.MusicStatService service)
        {
            _artistID = artistID;
            _service = service;
        }
        public async Task<List<Release>> Execute()
        {
            string releasesJson = await _service.getReleases(_artistID);
            ReleaseResults releaseResults = new DeserializeJson<ReleaseResults>().Deserialize(releasesJson);
            return releaseResults.Releases;
        }
    }
    public class GetArtistsByName
    {
        private string _artistName;

        private ArtistStats_web.Services.MusicStatService _service;
        public GetArtistsByName(string artistName, ArtistStats_web.Services.MusicStatService service)
        {
            _artistName = artistName;
            _service = service;
        }
        public async Task<Artist[]> Execute()
        {
            string artistsJson = await _service.getArtists(_artistName);
            ArtistResults artistsResults = new DeserializeJson<ArtistResults>().Deserialize(artistsJson);
            return artistsResults.Artists;
        }
    }
}
using System.Configuration;
using System.Collections.Specialized;
using Newtonsoft.Json;
using SpotifyPlaylistFromArtists.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace SpotifyPlaylistFromArtists
{
    static class Program
    {


        public static string SpotifyURL = "https://api.spotify.com/v1/";




        /*
         * Takes a list of artists, gets the top rated song by them, and adds to a playlist, also outputting the results to a CSV file. Handy for looking at large list
         * of bands, for example applications for a festival etc
         * 
         * Config required::
         * APIKey - Key provided by Spotify
         * Playlist - Playlist we'll add the songs to
         * PreferredGenres - Used to set the genres we'll look at first (comma seperated list)
         * 
         * Arist list comes from artist come from artists.txt in the working folder (CR seperated) TODO: Source from anywhere else e.g. Google docs
         */

        /* Sample config - no point trying to get info from API key!
         *
          <?xml version="1.0" encoding="utf-8" ?>
            <configuration>
                <startup> 
                    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2" />
                </startup>
                <appSettings>
                    <add key="Playlist" value="My playlist name" />
                    <add key="APIKey" value="nuAXjn3gNPGEYq-1x_3ZN1gZ05GxQ9TNhhN1Yu7oTmcGwCpYdk-tcAmDIvPU97CkCK6Cpryv5hdWYNLIYx7p3AVl1oqifdcH7D_ZjdSnlooj3jne9w1qamt9BNsFays1zLQAYEX0kZZIk_-z9371nrSXXtwExzjbAsi9q8H5WBQCL8ngVGkvaggGdSmy9zjqXjQPMorXZBncA_dZ" />
                    <add key="PreferredGenres" value="Rock,Metal,Hardcore" />
                </appSettings>
            </configuration>
        */

        [STAThread]
        static void Main()
        {
            List<ArtistResult> Results = new List<ArtistResult>();

            List<string> Artists = new List<string>();
            string PlaylistName = ConfigurationManager.AppSettings.Get("Playlist");


            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(@"artists.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line)) Artists.Add(line.Trim());
            }

            // Sort duplicates
            Artists = Artists.Distinct().ToList();


            var playlists = JsonConvert.DeserializeObject<PlaylistResponse>(GetResponse($@"{SpotifyURL}me/playlists", "", "GET"));
            var playlist = playlists.items.Single(x => x.name == PlaylistName);
            var playlistTracks = JsonConvert.DeserializeObject<PlaylistTracksResponse>(GetResponse($@"{SpotifyURL}users/{playlist.owner.id}/playlists/{playlist.id}/tracks","","GET"));
            string[] preferredGenres = ConfigurationManager.AppSettings.Get("PreferredGenres").Split(',');

            foreach (var ar in Artists)
            {
                var r = new ArtistResult() { Name = ar };

                var j = JsonConvert.DeserializeObject<ArtishSearchResponse>(GetResponse($@"{SpotifyURL}search", $@"q={HttpUtility.UrlEncode(ar)}&limit=100&type=artist", "GET")).artists.items;

                if (j.Count()==0)
                {
                    Results.Add(r);
                    continue;
                }

                // One result - don't have much choice!!
                var matchedArtist = j.Count() == 1 ? j.Single() : null;

                // Otherwise...
                if (matchedArtist == null)
                {
                    // If there are exact matches, only use these
                    var exactMatches = j.Where(x => x.name.ToLower() == ar.ToLower()).ToArray();
                    if (exactMatches.Any()) j = exactMatches;

                    // If none of these, try exact matches excluding special characters and only use these
                    if (!exactMatches.Any())
                    {
                        var exactMatchesWithoutCharactes = j.Where(x => x.name.ToLower().RemoveSpecialCharacters() == ar.ToLower().RemoveSpecialCharacters()).ToArray();
                        if (exactMatchesWithoutCharactes.Any()) j = exactMatchesWithoutCharactes;
                    }

                    // TODO: Few more things to play with - "the", case etc

                    // If one exact match use this
                    matchedArtist = matchedArtist ?? (j.Where(x => x.name.ToLower() == ar.ToLower()).Count() == 1 ? j.Where(x => x.name.ToLower() == ar.ToLower()).Single() : null);
                    // Looks for bands with the preffered genre and prioritise these
                    matchedArtist = matchedArtist ?? j.OrderByDescending(x => x.popularity).FirstOrDefault(x => x.genres.Any(y => preferredGenres.Any(z=>y.Contains(z))));
                    // Then prioritise no genre (if it's not the above we probaby don't want it
                    matchedArtist = matchedArtist ?? j.OrderByDescending(x => x.popularity).FirstOrDefault(x => !x.genres.Any());
                    // Just take the most popular left over one
                    matchedArtist = matchedArtist ?? j.OrderByDescending(x => x.popularity).First();
                }
                if (matchedArtist != null)
                {
                    r.MatchedArtist = matchedArtist.name;
                    r.ArtistId = matchedArtist.id;
                    r.Popularity = matchedArtist.popularity;
                    r.Genres = matchedArtist.genres.ToList();
                    r.Followers = matchedArtist.followers.total;

                    // Check if the artist is already in the playlist
                    var existingTrack = playlistTracks.items.SingleOrDefault(x => x.track.artists.First().id == matchedArtist.id);


                    if (existingTrack!=null)
                    {
                        r.Existing = true;
                        r.Title = existingTrack.track.name;
                        r.SongId = existingTrack.track.id;
                    }
                    else
                    {
                        var t = JsonConvert.DeserializeObject<TopTracksResponse>(GetResponse($@"{SpotifyURL}artists/{matchedArtist.id}/top-tracks", "country=GB&limit=50", "GET"));

                        var artistTracks = t.tracks.FirstOrDefault();
                        if (artistTracks != null)
                        {
                            r.Title = artistTracks.name;
                            r.SongId = artistTracks.id;
                            r.SongPopularity = artistTracks.popularity;
                            r.SongsCount = t.tracks.Count();
                            if (artistTracks != null)
                            {
                                // Add to playlist!
                                GetResponse($@"{SpotifyURL}users/{playlist.owner.id}/playlists/{playlist.id}/tracks", $@"uris=spotify%3Atrack%3A{artistTracks.id}", "POST");

                            }
                        }
                    }

                }
                Results.Add(r);
            }

            using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter($@"results{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv", false))
            {
                outputFile.WriteLine(string.Join("", Results.OrderByDescending(x=>x.Popularity).ThenByDescending(x=>x.MatchedArtist=="").Select(x => x.GetData())));
            }


        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string GetResponse(string endpoint, string query, string type)
        {


            if (query!="") endpoint += "?" + query;

            var request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Method = type;
            request.Headers.Add("Authorization: Bearer " + ConfigurationManager.AppSettings.Get("APIKey"));

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException)
            {
                throw;
            }


        }
    }
}

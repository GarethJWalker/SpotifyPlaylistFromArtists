using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyPlaylistFromArtists.Models
{
    public class ArtistResult
    {

            public string Name { get; set; } // The name we fed in
            public string MatchedArtist { get; set; }  // The artist we got from Spotify
            public string ArtistId { get; set; }
            public int Popularity { get; set; }
            public bool Existing { get; set; }
            public string Title { get; set; }
            public string SongId { get; set; }
            public int SongsCount { get; set; }
            public int SongPopularity { get; set; }
            public List<string> Genres { get; set; }
            public int Followers { get; set; }

            public string GetData()
            {

                return "\"" + Name + "\",\"" + MatchedArtist + "\"," + "https://open.spotify.com/artist/" + ArtistId + "," + ArtistId + "," + Popularity + "," + Followers + "," + Existing + ",\"" + Title + "\",\"" + SongId + "\"," + SongsCount + "," + SongPopularity + ",\"" + (Genres != null ? string.Join(",", Genres) : "") + "\"\r\n";
            }
        }

    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyPlaylistFromArtists.Models
{
    class PlaylistResponse
    {


            public string href { get; set; }
            public Item[] items { get; set; }
            public int limit { get; set; }
            public object next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
        

        public class Item
        {
            public bool collaborative { get; set; }
            public External_Urls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public Image[] images { get; set; }
            public string name { get; set; }
            public Owner owner { get; set; }
            public bool _public { get; set; }
            public string snapshot_id { get; set; }
            public Tracks tracks { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }

        public class External_Urls
        {
            public string spotify { get; set; }
        }

        public class Owner
        {
            public string display_name { get; set; }
            public External_Urls1 external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }

        public class External_Urls1
        {
            public string spotify { get; set; }
        }

        public class Tracks
        {
            public string href { get; set; }
            public int total { get; set; }
        }

        public class Image
        {
            public int? height { get; set; }
            public string url { get; set; }
            public int? width { get; set; }
        }

    }
}

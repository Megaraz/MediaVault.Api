using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.Tmdb
{


    public sealed class TmdbResult
    {
        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        //[JsonPropertyName("backdrop_path")]
        //public string? BackdropPath { get; set; }

        //public float popularity { get; set; }
        //public int vote_count { get; set; }
        //public bool video { get; set; }
        //public float vote_average { get; set; }

        //public bool adult { get; set; }
        //public string overview { get; set; }
        //public string release_date { get; set; }
        //public int[] genre_ids { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Instagraph.DataProcessor.Dtos.Export
{
    public class UncommentedPostDto
    {
	    [Required]
	    public int Id { get; set; }

	    [Required]
		[JsonProperty("Picture")]
	    public string Path { get; set; }

	    [Required]
	    [JsonProperty("User")]
		public string Username { get; set; }
	}
}

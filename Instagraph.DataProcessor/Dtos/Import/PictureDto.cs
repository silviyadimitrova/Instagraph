using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Instagraph.DataProcessor.Dtos.Import
{
    public class PictureDto
    {
	    [Required]
	    public string Path { get; set; }

	    [Required]
	    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
		public decimal Size { get; set; }
	}
}

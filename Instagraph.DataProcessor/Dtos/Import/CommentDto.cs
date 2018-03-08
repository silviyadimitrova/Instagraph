using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace Instagraph.DataProcessor.Dtos.Import
{
	[XmlType("comment")]
    public class CommentDto
    {
	    [XmlElement("content")]
		[Required]
	    [StringLength(250)]
	    public string Content { get; set; }

		[Required]
	    [XmlElement("user")]
	    public string User { get; set; }

	    [XmlElement("post")]
		[Required]
	    public PostIdInfo Id { get; set; }
	}

	public class PostIdInfo
	{
		[XmlAttribute("id")]
		[Required]
		public int Id { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Instagraph.DataProcessor.Dtos.Export
{
	[XmlType("user")]
    public class MostCommentedPostDto
    {
	    public string Username { get; set; }

	    public int? MostComments { get; set; }
    }
}

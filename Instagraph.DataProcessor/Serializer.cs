using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper.QueryableExtensions;
using Instagraph.Data;
using Instagraph.DataProcessor.Dtos.Export;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Instagraph.DataProcessor
{
    public class Serializer
    {
        public static string ExportUncommentedPosts(InstagraphContext context)
        {
	        var uncommentedPosts = context.Posts.Where(p => p.Comments.Count == 0).Include(p => p.User).Include(p => p.Picture).ToArray();

			var uncommentedPostsDtos = new List<UncommentedPostDto>();

	        foreach (var uncommp in uncommentedPosts.OrderBy(p => p.Id))
	        {
		        var uncommentedPostDto = new UncommentedPostDto
		        {
			        Username = uncommp.User.Username,
			        Path = uncommp.Picture.Path,
			        Id = uncommp.Id
		        };

				uncommentedPostsDtos.Add(uncommentedPostDto);
	        }

	        var jsonUncommentedPosts = JsonConvert.SerializeObject(uncommentedPostsDtos, Formatting.Indented);

	        return jsonUncommentedPosts;
		}

		public static string ExportPopularUsers(InstagraphContext context)
		{
			var users = context.Users
				.Where(u => u.Posts
					.Any(p => p.Comments
						.Any(c => u.Followers
							.Any(f => f.FollowerId == c.UserId))))
				.OrderBy(u => u.Id)
				.ProjectTo<PopularUserDto>()
				.ToArray();

			string jsonString = JsonConvert.SerializeObject(users, Formatting.Indented);

			return jsonString;
		}

		public static string ExportCommentsOnPosts(InstagraphContext context)
		{
			var sb = new StringBuilder();

			var users = context.Users
				.Include(u => u.Posts)
				.ThenInclude(p => p.Comments)
				.Select(u => new MostCommentedPostDto
				{
					Username = u.Username,
					MostComments = u.Posts.Select(p => p.Comments.Count)
						.OrderByDescending(e => e)
						.FirstOrDefault()
				});

			var usersDtoList = new MostCommentedPostDto[users.Count()];
			int i = 0;
			foreach (var user in users)
			{
				if (user.MostComments == null)
				{
					user.MostComments = 0;
				}

				usersDtoList[i] = user;
				i++;
			}

			var resultList = usersDtoList.OrderByDescending(u => u.MostComments).ThenBy(u => u.Username).ToArray();

			var serializer = new XmlSerializer(typeof(MostCommentedPostDto[]), new XmlRootAttribute("users"));

			serializer.Serialize(new StringWriter(sb), resultList, new XmlSerializerNamespaces(new[] {XmlQualifiedName.Empty}));

			return sb.ToString();
		}
    }
}

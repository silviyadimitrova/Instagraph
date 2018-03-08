using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;

using Instagraph.Data;
using Instagraph.DataProcessor.Dtos.Import;
using Instagraph.Models;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Instagraph.DataProcessor
{
	public class Deserializer
	{
		public const string SuccessMsgPicture = "Successfully imported Picture {0}.";
		public const string SuccessMsgUser = "Successfully imported User {0}.";
		public const string SuccessMsgPost = "Successfully imported Post {0}.";
		public const string SuccessMsgComment = "Successfully imported Comment {0}.";
		public const string SuccessMsgUserFollower = "Successfully imported Follower {0} to User {1}.";
		public const string ErrorMsg = "Error: Invalid data.";


		public static string ImportPictures(InstagraphContext context, string jsonString)
		{
			var deserializedPicturesDto = JsonConvert.DeserializeObject<PictureDto[]>(jsonString);

			var resultSb = new StringBuilder();

			var validPictures = new List<Picture>();

			foreach (var pictureDto in deserializedPicturesDto)
			{
				bool repeatedPicture = validPictures.Any(p => p.Path == pictureDto.Path) && context.Pictures.Any(p => p.Path == pictureDto.Path);

				if (!IsValid(pictureDto) || repeatedPicture || String.IsNullOrEmpty(pictureDto.Path) || String.IsNullOrWhiteSpace(pictureDto.Path))
				{
					resultSb.AppendLine(ErrorMsg);
					continue;
				}

				var picture = new Picture
				{
					Path = pictureDto.Path,
					Size = pictureDto.Size
				};

				validPictures.Add(picture);
				resultSb.AppendLine(String.Format(SuccessMsgPicture, picture.Path));
			}

			context.Pictures.AddRange(validPictures);
			context.SaveChanges();

			return resultSb.ToString();
		}

		public static string ImportUsers(InstagraphContext context, string jsonString)
		{
			var deserializedUsersDto = JsonConvert.DeserializeObject<UserDto[]>(jsonString);

			var resultSb = new StringBuilder();

			var validUsers = new List<User>();

			foreach (var userDto in deserializedUsersDto)
			{
				bool pictureExists = context.Pictures.Any(p => p.Path == userDto.ProfilePicture);

				if (!IsValid(userDto) || !pictureExists)
				{
					resultSb.AppendLine(ErrorMsg);
					continue;
				}

				var user = new User()
				{
					Username = userDto.Username,
					Password = userDto.Password,
					ProfilePicture = context.Pictures.SingleOrDefault(p => p.Path == userDto.ProfilePicture)
				};

				validUsers.Add(user);
				resultSb.AppendLine(String.Format(SuccessMsgUser, user.Username));
			}

			context.Users.AddRange(validUsers);
			context.SaveChanges();
			Console.WriteLine(resultSb.ToString());
			return resultSb.ToString();
		}

		public static string ImportFollowers(InstagraphContext context, string jsonString)
		{
			var followersDto = JsonConvert.DeserializeObject<UserFollowerDto[]>(jsonString);

			var validUsersFollowers = new List<UserFollower>();

			var resultSb = new StringBuilder();

			foreach (var followerDto in followersDto)
			{
				var userExists = context.Users.Any(u => u.Username == followerDto.User);
				var followerExists = context.Users.Any(f => f.Username == followerDto.Follower);

				var alreadyFollowed = validUsersFollowers.Any(uf => uf.User.Username == followerDto.User &&
																	uf.Follower.Username == followerDto.Follower);

				if (!userExists || !followerExists || alreadyFollowed)
				{
					resultSb.AppendLine(ErrorMsg);
					continue;
				}

				var userFollower = new UserFollower()
				{
					User = context.Users.FirstOrDefault(u => u.Username == followerDto.User),
					Follower = context.Users.FirstOrDefault(f => f.Username == followerDto.Follower),
					UserId = context.Users.FirstOrDefault(u => u.Username == followerDto.User).Id,
					FollowerId = context.Users.FirstOrDefault(u => u.Username == followerDto.Follower).Id
				};

				validUsersFollowers.Add(userFollower);
				resultSb.AppendLine(String.Format(SuccessMsgUserFollower, userFollower.Follower.Username, userFollower.User.Username));
			}

			context.UsersFollowers.AddRange(validUsersFollowers);
			context.SaveChanges();

			return resultSb.ToString();
		}

		public static string ImportPosts(InstagraphContext context, string xmlString)
		{
			var serializer = new XmlSerializer(typeof(PostDto[]), new XmlRootAttribute("posts"));
			var deserializedPosts = (PostDto[])serializer.Deserialize(new StringReader(xmlString));

			var sb = new StringBuilder();

			var validPosts = new List<Post>();

			foreach (var postDto in deserializedPosts)
			{

				var userExists = context.Users.Any(u => u.Username == postDto.User);
				var pictureExists = context.Pictures.Any(p => p.Path == postDto.Picture);

				if (!userExists || !pictureExists)
				{
					sb.AppendLine(ErrorMsg);
					continue;
				}

				bool inputIsValid = !String.IsNullOrWhiteSpace(postDto.Caption) &&
				                    !String.IsNullOrWhiteSpace(postDto.User) &&
				                    !String.IsNullOrWhiteSpace(postDto.Picture);

				if (!inputIsValid)
				{
					sb.AppendLine(ErrorMsg);
					continue;
				}

				var post = new Post
				{
					User = context.Users.FirstOrDefault(u => u.Username == postDto.User),
					Picture = context.Pictures.FirstOrDefault(p => p.Path == postDto.Picture),
					Caption = postDto.Caption
				};

				validPosts.Add(post);
				sb.AppendLine(String.Format(SuccessMsgPost, postDto.Caption));
			}

			context.Posts.AddRange(validPosts);
			context.SaveChanges();

			var result = sb.ToString();
			return result;
		}

		public static string ImportComments(InstagraphContext context, string xmlString)
		{
			var serializer = new XmlSerializer(typeof(CommentDto[]), new XmlRootAttribute("comments"));
			var deserializedComments = (CommentDto[])serializer.Deserialize(new StringReader(xmlString));

			var sb = new StringBuilder();

			var validComments = new List<Comment>();

			foreach (var commentDto in deserializedComments)
			{
				if (commentDto.Id == null)
				{
					sb.AppendLine(ErrorMsg);
					continue;
				}
				var userExists = context.Users.Any(u => u.Username == commentDto.User);
				var postExists = context.Posts.Any(p => p.Id == commentDto.Id.Id);

				if (!userExists || !postExists)
				{
					sb.AppendLine(ErrorMsg);
					continue;
				}

				bool inputIsValid = !String.IsNullOrWhiteSpace(commentDto.User) &&
				                    !String.IsNullOrWhiteSpace(commentDto.Content);

				if (!inputIsValid)
				{
					sb.AppendLine(ErrorMsg);
					continue;
				}

				var comment = new Comment
				{
					User = context.Users.FirstOrDefault(u => u.Username == commentDto.User),
					Post = context.Posts.FirstOrDefault(p => p.Id == commentDto.Id.Id),
					Content = commentDto.Content
				};

				validComments.Add(comment);
				sb.AppendLine(String.Format(SuccessMsgComment, commentDto.Content));
			}

			context.Comments.AddRange(validComments);
			context.SaveChanges();

			var result = sb.ToString();

			return result;
		}

		private static bool IsValid(object obj)
		{
			var validationContext = new ValidationContext(obj);
			var validationResult = new List<ValidationResult>();

			var isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);

			return isValid;
		}

	}
}

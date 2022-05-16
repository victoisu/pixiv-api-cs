using PixivApi.ResultJson.Illust;
using System;
using System.Linq;

/// <summary>
/// More usable data classes. Is a big mess, needs structural work.
/// </summary>
namespace PixivApi.Data
{
	public interface IWork
	{
		public int Id { get; }
		public string Title { get; }
		public int IllustType { get; }
		public string[] PublicTags { get; }
		public string[] PersonalTags { get; }
		public int UserId { get; }
		public string UserName { get; }
		public int Width { get; }
		public int Height { get; }
		public int PageCount { get; }
		public bool IsBookmarkable { get; }
		public bool IsPublicBookmark { get; }
		public long BookmarkId { get; }
		public DateTime CreateDate { get; }
		public DateTime UpdateDate { get; }
		public bool IsUnlisted { get; }
		public string ThumbnailUrl { get; }
		public string OriginalUrl { get; }
		public int BookmarkCount { get; }
		public int LikeCount { get; }
		public int CommentCount { get; }
		public int ViewCount { get; }
	}

	public record ListWork : IWork
	{
		public string ThumbnailUrl { get; init; }

		public int Id { get; init; }
		public string Title { get; init; }
		public int IllustType { get; init; }
		public string[] PublicTags { get; init; }
		public int UserId { get; init; }
		public string UserName { get; init; }
		public int Width { get; init; }
		public int Height { get; init; }
		public int PageCount { get; init; }
		public bool IsBookmarkable { get; init; }
		public bool IsPublicBookmark { get; init; }
		public long BookmarkId { get; init; }
		public DateTime CreateDate { get; init; }
		public DateTime UpdateDate { get; init; }
		public bool IsUnlisted { get; init; }

		public string[] PersonalTags => null;

		public string OriginalUrl => null;

		public int BookmarkCount => -1;

		public int LikeCount => -1;

		public int CommentCount => -1;

		public int ViewCount => -1;
	}

	public record BookmarkWork : ListWork
	{
		public new string[] PersonalTags { get; init; }

		internal static BookmarkWork FromRecord(ResultJson.Bookmarks.Work other, string[] personalTags)
		{
			return new BookmarkWork
			{
				// BookmarkWork members
				PersonalTags = personalTags,

				// Special attention
				IsPublicBookmark = !other.bookmarkData._private,
				BookmarkId = other.bookmarkData.id,

				// Basically copy-paste
				Id = other.id,
				UserId = other.userId,
				Title = other.title,
				IllustType = other.illustType,
				PublicTags = other.tags,
				ThumbnailUrl = other.url,
				UserName = other.userName,
				Width = other.width,
				Height = other.height,
				PageCount = other.pageCount,
				IsBookmarkable = other.isBookmarkable,
				CreateDate = other.createDate,
				UpdateDate = other.updateDate,
				IsUnlisted = other.isUnlisted
			};
		}
	}

	public record Work : IWork
	{
		public string OriginalUrl { get; init; }
		public int BookmarkCount { get; init; }
		public int LikeCount { get; init; }
		public int CommentCount { get; init; }
		public int ViewCount { get; init; }

		internal static Work FromRecord(ResultJson.Illust.Body other)
		{
			return new Work
			{
				// Work members
				OriginalUrl = other.urls.original,
				BookmarkCount = other.bookmarkCount,
				LikeCount = other.likeCount,
				CommentCount = other.commentCount,
				ViewCount = other.viewCount,

				// Special attention
				IsPublicBookmark = (!other.bookmarkData?._private) ?? false,
				BookmarkId = other.bookmarkData?.id ?? 0,

				// Basically copy-paste
				Id = other.id,
				UserId = other.userId,
				Title = other.title,
				IllustType = other.illustType,
				PublicTags = TagsToStrings(other.tags),
				UserName = other.userName,
				Width = other.width,
				Height = other.height,
				PageCount = other.pageCount,
				IsBookmarkable = other.isBookmarkable,
				CreateDate = other.createDate,
				UpdateDate = other.uploadDate,
				IsUnlisted = other.isUnlisted
			};
		}

		private static string[] TagsToStrings(Tags tags)
		{
			return tags.tags.ToList().Select(t => t.tag).ToArray();
		}

		public int Id { get; init; }
		public string Title { get; init; }
		public int IllustType { get; init; }
		public string[] PublicTags { get; init; }
		public int UserId { get; init; }
		public string UserName { get; init; }
		public int Width { get; init; }
		public int Height { get; init; }
		public int PageCount { get; init; }
		public bool IsBookmarkable { get; init; }
		public bool IsPublicBookmark { get; init; }
		public long BookmarkId { get; init; }
		public DateTime CreateDate { get; init; }
		public DateTime UpdateDate { get; init; }
		public bool IsUnlisted { get; init; }

		public string[] PersonalTags => null;

		public string ThumbnailUrl => null;
	}

	public record Page
	{
		public int WorkId { get; init; }
		public int PageNumber { get; set; }
		public string OriginalUrl { get; init; }
		public int Width { get; init; }
		public int Height { get; init; }
		public string Extension { get; init; }
		public string FileName { get; init; }

		public static Page[] FromRecord(ResultJson.Pages.Rootobject other, int workId = 0)
		{
			int pageNumber = 0;
			return other.body.ToList().Select(p => new Page
			{
				WorkId = workId,
				PageNumber = pageNumber++,
				OriginalUrl = p.urls.original,
				Width = p.width,
				Height = p.height,
				Extension = p.urls.original.Split(".").Last(),
				FileName = p.urls.original.Split("/").Last()
			}).ToArray();
		}
	}
}

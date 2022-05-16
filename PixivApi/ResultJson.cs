using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Almost-raw JSON results from calling the Pixiv web API.
/// This serves solely as a first step past raw JSON.
/// </summary>
namespace PixivApi.ResultJson
{
	namespace Bookmarks
	{
		internal record Rootobject
		{
			public bool error { get; init; }
			public string message { get; init; }
			public Body body { get; init; }
		}

		internal record Body
		{
			public Work[] works { get; init; }
			public int total { get; init; }
			public Zoneconfig zoneConfig { get; init; }
			public Extradata extraData { get; init; }
			[JsonConverter(typeof(BookmarkTagsConverter))]
			public Dictionary<long, string[]> bookmarkTags { get; init; }

		}

		internal record Zoneconfig
		{
			public Header header { get; init; }
			public Footer footer { get; init; }
			public Logo logo { get; init; }
			public _500X500 _500x500 { get; init; }
		}

		internal record Header
		{
			public string url { get; init; }
		}

		internal record Footer
		{
			public string url { get; init; }
		}

		internal record Logo
		{
			public string url { get; init; }
		}

		internal record _500X500
		{
			public string url { get; init; }
		}

		internal record Extradata
		{
			public Meta meta { get; init; }
		}

		internal record Meta
		{
			public string title { get; init; }
			public string description { get; init; }
			public string canonical { get; init; }
			public Ogp ogp { get; init; }
			public Twitter twitter { get; init; }
			public Alternatelanguages alternateLanguages { get; init; }
			public string descriptionHeader { get; init; }
		}

		internal record Ogp
		{
			public string description { get; init; }
			public string image { get; init; }
			public string title { get; init; }
			public string type { get; init; }
		}

		internal record Twitter
		{
			public string description { get; init; }
			public string image { get; init; }
			public string title { get; init; }
			public string card { get; init; }
		}

		internal record Alternatelanguages
		{
			public string ja { get; init; }
			public string en { get; init; }
		}

		internal record Work
		{
			[JsonNumberHandlingAttribute(JsonNumberHandling.AllowReadingFromString)]
			public int id { get; init; }
			public string title { get; init; }
			public int illustType { get; init; }
			public int xRestrict { get; init; }
			public int restrict { get; init; }
			public int sl { get; init; }
			public string url { get; init; }
			public string description { get; init; }
			public string[] tags { get; init; }

			[JsonNumberHandlingAttribute(JsonNumberHandling.AllowReadingFromString)]
			public int userId { get; init; }
			public string userName { get; init; }
			public int width { get; init; }
			public int height { get; init; }
			public int pageCount { get; init; }
			public bool isBookmarkable { get; init; }
			public Bookmarkdata bookmarkData { get; init; }
			public string alt { get; init; }
			public Titlecaptiontranslation titleCaptionTranslation { get; init; }
			public DateTime createDate { get; init; }
			public DateTime updateDate { get; init; }
			public bool isUnlisted { get; init; }
			public bool isMasked { get; init; }
			public string profileImageUrl { get; init; }
		}

		internal record Bookmarkdata
		{
			[JsonNumberHandlingAttribute(JsonNumberHandling.AllowReadingFromString)]
			public long id { get; init; }
			public bool _private { get; init; }
		}

		internal record Titlecaptiontranslation
		{
			public object workTitle { get; init; }
			public object workCaption { get; init; }
		}
	}

	namespace Illust
	{
		internal record Rootobject
		{
			public bool error { get; init; }
			public string message { get; init; }
			public Body body { get; init; }
		}

		internal record Body
		{
			[JsonNumberHandlingAttribute(JsonNumberHandling.AllowReadingFromString)]
			public int illustId { get; init; }
			public string illustTitle { get; init; }
			public string illustComment { get; init; }
			[JsonNumberHandlingAttribute(JsonNumberHandling.AllowReadingFromString)]
			public int id { get; init; }
			public string title { get; init; }
			public string description { get; init; }
			public int illustType { get; init; }
			public DateTime createDate { get; init; }
			public DateTime uploadDate { get; init; }
			public int restrict { get; init; }
			public int xRestrict { get; init; }
			public int sl { get; init; }
			public Urls urls { get; init; }
			public Tags tags { get; init; }
			public string alt { get; init; }
			public string[] storableTags { get; init; }
			[JsonNumberHandlingAttribute(JsonNumberHandling.AllowReadingFromString)]
			public int userId { get; init; }
			public string userName { get; init; }
			public string userAccount { get; init; }
			public Userillusts userIllusts { get; init; }
			public bool likeData { get; init; }
			public int width { get; init; }
			public int height { get; init; }
			public int pageCount { get; init; }
			public int bookmarkCount { get; init; }
			public int likeCount { get; init; }
			public int commentCount { get; init; }
			public int responseCount { get; init; }
			public int viewCount { get; init; }
			//public string bookStyle { get; init; }
			public bool isHowto { get; init; }
			public bool isOriginal { get; init; }
			public object[] imageResponseOutData { get; init; }
			public object[] imageResponseData { get; init; }
			public int imageResponseCount { get; init; }
			public object pollData { get; init; }
			public object seriesNavData { get; init; }
			public object descriptionBoothId { get; init; }
			public object descriptionYoutubeId { get; init; }
			public object comicPromotion { get; init; }
			public object fanboxPromotion { get; init; }
			public object[] contestBanners { get; init; }
			public bool isBookmarkable { get; init; }
			public Bookmarkdata bookmarkData { get; init; }
			public object contestData { get; init; }
			public Zoneconfig zoneConfig { get; init; }
			public Extradata extraData { get; init; }
			public Titlecaptiontranslation titleCaptionTranslation { get; init; }
			public bool isUnlisted { get; init; }
			public object request { get; init; }
			public int commentOff { get; init; }
		}

		internal record Urls
		{
			public string mini { get; init; }
			public string thumb { get; init; }
			public string small { get; init; }
			public string regular { get; init; }
			public string original { get; init; }
		}

		internal record Tags
		{
			public string authorId { get; init; }
			public bool isLocked { get; init; }
			public Tag[] tags { get; init; }
		}

		internal record Userillusts
		{
		}

		internal record Bookmarkdata
		{
			[JsonNumberHandlingAttribute(JsonNumberHandling.AllowReadingFromString)]
			public long id { get; init; }
			public bool _private { get; init; }
		}

		internal record Zoneconfig
		{
		}

		internal record Extradata
		{
			public Meta meta { get; init; }
		}

		internal record Meta
		{
			public string title { get; init; }
			public string description { get; init; }
			public string canonical { get; init; }
			public Alternatelanguages alternateLanguages { get; init; }
			public string descriptionHeader { get; init; }
			public Ogp ogp { get; init; }
			public Twitter twitter { get; init; }
		}

		internal record Alternatelanguages
		{
			public string ja { get; init; }
			public string en { get; init; }
		}

		internal record Ogp
		{
			public string description { get; init; }
			public string image { get; init; }
			public string title { get; init; }
			public string type { get; init; }
		}

		internal record Twitter
		{
			public string description { get; init; }
			public string image { get; init; }
			public string title { get; init; }
			public string card { get; init; }
		}

		internal record Titlecaptiontranslation
		{
			public object workTitle { get; init; }
			public object workCaption { get; init; }
		}

		internal record Tag
		{
			public string tag { get; init; }
			public bool locked { get; init; }
			public bool deletable { get; init; }
			public string userId { get; init; }
			public string userName { get; init; }
		}
	}

	namespace Pages
	{

		public class Rootobject
		{
			public bool error { get; set; }
			public string message { get; set; }
			public Body[] body { get; set; }
		}

		public class Body
		{
			public Urls urls { get; set; }
			public int width { get; set; }
			public int height { get; set; }
		}

		public class Urls
		{
			public string thumb_mini { get; set; }
			public string small { get; set; }
			public string regular { get; set; }
			public string original { get; set; }
		}

	}
}

using PixivApi.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PixivApi
{
	/// <summary>
	/// Singleton class for fetching bookmark info of user.
	/// Requires setup in the form of a PHP session ID.
	/// Being singleton, this setup should only happen once.
	/// </summary>
	public class Fetcher
	{
		private static Fetcher instance = null;
		public static Fetcher Instance
		{
			get
			{
				if (instance == null)
				{
					throw new InvalidOperationException("Fetcher needs to be configured with SSH session ID before use.");
				}

				return instance;
			}
		}
		public static bool IsConfigured { get; private set; } = false;

		/// <summary>
		/// Instantiates singleton with given PHP session ID.
		/// </summary>
		/// <param name="phpSession">Session ID used to perform requests that require authentication.</param>
		/// <param name="poolSize">Size of pool to use for configured instance. Dangerous to set high -- you're at the mercy of Pixiv. This is the number of requests that can be made in <paramref name="poolInterval"/> ms.</param>
		/// <param name="poolInterval">The amount of time (in ms) to limit the instance to <paramref name="poolSize"/> requests.</param>
		/// <returns>Instantiated singleton Fetcher.</returns>
		public static Fetcher Configure(string phpSession, int poolSize = defaultPoolSize, int poolInterval = defaultPoolInterval)
		{
			if (instance != null)
			{
				throw new InvalidOperationException("Fetcher can only be configured once.");
			}

			instance = new Fetcher(phpSession, poolSize, poolInterval);
			IsConfigured = true;
			return instance;
		}

		/// <summary>
		/// Simply tries to configure singleton instance with given parameters. Returns bool indicating whether the API has been successfully configured.
		/// </summary>
		/// <param name="phpSession">Session ID used to perform requests that require authentication.</param>
		/// <param name="poolSize"></param>
		/// <param name="poolInterval"></param>
		/// <returns></returns>
		public static bool TryConfigure(string phpSession, int poolSize = defaultPoolSize, int poolInterval = defaultPoolInterval)
		{
			try
			{
				Configure(phpSession, poolSize, poolInterval);
				return true;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}

		private const string UserAgentAlias = @"Mozilla/5.0 (Macintosh; Intel Mac OS X 10.11; rv:43.0) Gecko/20100101 Firefox/43.0";
		private const int BookmarkLimit = 100;

		private readonly HttpClient client;
		private readonly int id;

		// Size and interval are arbitrary, do not know specifically
		// how limited it needs to be to not incur the wrath of pixiv.
		// Interval indicates number milliseconds a single request holds
		// semaphore slot for.
		private SemaphoreSlim pool;
		private const int defaultPoolSize = 8;
		private const int defaultPoolInterval = 2000;
		private int _poolInterval;

		private Fetcher(string phpSession, int poolSize, int poolInterval)
		{
			var sections = phpSession.Split("_");
			this.id = int.Parse(sections[0]);

			client = new HttpClient();
			client.DefaultRequestHeaders.Add("User-Agent", UserAgentAlias);
			client.DefaultRequestHeaders.Add("cookie", $"PHPSESSID={phpSession};");
			client.DefaultRequestHeaders.Add("referer", @"https://www.pixiv.net/");

			pool = new SemaphoreSlim(poolSize);
			_poolInterval = poolInterval;
		}

		/// <summary>
		/// Gets the number of bookmarks the user has.
		/// May or may not be accurate to the number of works which can actually be retrieved, considering deleted, privated, or My Pixiv-limited.
		/// </summary>
		/// <returns>Number of configured user's bookmarks.</returns>
		public async Task<int> GetBookmarkCount()
		{
			string url = GenerateBookmarkUrl(0, 1);
			var result = await GetBookmarkJsonRoot(url);

			return result?.body.total ?? 0;
		}

		/// <summary>
		/// Asynchronously returns a Work object containing its details.
		/// Returns null if request failed, likely due to the work not being accessible.
		/// </summary>
		/// <param name="id">Id of work to fetch.</param>
		/// <returns>The work attempted to be fetched or null if the request fails.</returns>
		public async Task<Work> GetWork(int id)
		{
			var url = GenerateWorkUrl(id);
			var result = await GetWorkJsonRoot(url);
			return result == null ? null : Work.FromRecord(result.body);
		}
		
		/// <summary>
		/// Gets the image associated with a given <paramref name="id"/>/<paramref name="page"/> combination.
		/// </summary>
		/// <param name="id">Id of work.</param>
		/// <param name="page">0-indexed page of work to try to get.</param>
		/// <returns></returns>
		public async Task<(Stream, string)> GetWorkImage(int id, int page = 0)
		{
			var pages = await GetPages(id);
			return (await GetWorkImage(pages[page]), pages[page].FileName);
		}

		/// <summary>
		/// Gets array of pages associated with work with id <paramref name="id"/>.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Empty array if no pages were found, otherwise array of pages.</returns>
		public async Task<Page[]> GetPages(int id)
		{
			return await GetPages(id, null);
		}

		/// <summary>
		/// Gets array of pages associated with work with id <paramref name="id"/>.
		/// </summary>
		/// <param name="id">Id of work to get pages of.</param>
		/// <param name="privateLock">Optional second semaphore to allow third party to further limit API on these calls. Locks together with the main lock, not separately.</param>
		/// <returns>Empty array if no pages were found, otherwise array of pages.</returns>
		private async Task<Page[]> GetPages(int id, SemaphoreSlim privateLock)
		{
			var url = GeneratePagesUrl(id);
			var root = await GetJsonRoot<ResultJson.Pages.Rootobject>(url, privateLock);

			return root == null ? Array.Empty<Page>() : Page.FromRecord(root, id);
		}

		/// <summary>
		/// Gets enumerable of arrays of pages associated with work with id <paramref name="id"/>.
		/// </summary>
		/// <param name="ids">Ids to get pages of.</param>
		/// <param name="maxConcurrent">Max allowed concurrent requests, separate and combined with overall configured connection limit.</param>
		/// <returns>Enumerable of pages retrieved.</returns>
		public async IAsyncEnumerable<Page[]> GetPages(IEnumerable<int> ids, int maxConcurrent = defaultPoolSize)
		{
			var pageLock = new SemaphoreSlim(maxConcurrent);
			var tasks = ids.Select(id => GetPages(id, pageLock)).ToList();

			while (tasks.Any())
			{
				var workPages = await Task.WhenAny(tasks);
				tasks.Remove(workPages);
				yield return await workPages;
			}
			yield break;
		}

		/// <summary>
		/// Gets image associated with <paramref name="page"/>.
		/// </summary>
		/// <param name="page">Page to get image of.</param>
		/// <returns>Image associated with <paramref name="page"/></returns>
		public async Task<Stream> GetWorkImage(Page page)
		{
			HttpResponseMessage response = (await client.GetAsync(page.OriginalUrl)).EnsureSuccessStatusCode();
			return await response.Content.ReadAsStreamAsync();
		}

		/// <summary>
		/// Tries to get <paramref name="count"/> bookmarks.
		/// </summary>
		/// <param name="count">Number of bookmarked works to get.</param>
		/// <returns>Asynchronous enumerable of fetched works.</returns>
		public async IAsyncEnumerable<BookmarkWork> GetBookmarks(int count, IProgress<float> progress = null)
		{
			progress?.Report(0f);

			var taskList = new List<Task<List<BookmarkWork>>>();
			for (int i = 0; i < count; i += BookmarkLimit)
			{
				int n = Math.Min(BookmarkLimit, count - i);
				taskList.Add(GetBookmarksSingle(i, n));
			}

			int progressCount = 0;
			while (taskList.Any())
			{
				var pageTask = await Task.WhenAny(taskList);
				taskList.Remove(pageTask);
				var page = await pageTask;
				foreach (var work in page)
				{
					yield return work;
				}
				Interlocked.Add(ref progressCount, page.Count);
				progress?.Report(progressCount / (float)count);
			}
			
			progress?.Report(1f);
			yield break;
		}

		/// <summary>
		/// Tries to get all of user's bookmarks.
		/// </summary>
		/// <returns>Asynchronous enumerable of fetched works.</returns>
		public async IAsyncEnumerable<BookmarkWork> GetBookmarks(IProgress<float> progress = null)
		{
			int count = await GetBookmarkCount();
			await foreach (var work in GetBookmarks(count, progress))
			{
				yield return work;
			}
		}

		/// <summary>
		/// Loads a single page of bookmarks from the web API.
		/// Limit cannot be higher than 100. If the offset + the limit is less 
		/// than the number of available bookmarks, returns works up until it runs out.
		/// </summary>
		/// <param name="offset">Offset to start page on. Counts in single bookmarks, zero-indexed.</param>
		/// <param name="limit">Maximum number of works to get. Must be equal to or lower than 100.</param>
		/// <returns>Asynchronous enumerable of works on page.</returns>
		private async Task<List<BookmarkWork>> GetBookmarksSingle(int offset, int limit)
		{
			if (limit < 1 || limit > BookmarkLimit)
				throw new ArgumentOutOfRangeException($"A limit of {limit} is not valid.");

			string url = GenerateBookmarkUrl(offset, limit);
			var result = await GetBookmarkJsonRoot(url);

			return result?.body.works.ToList().Select((work) =>
			{
				return BookmarkWork.FromRecord(
					work,
					result.body.bookmarkTags.TryGetValue(work.bookmarkData.id, out var personalTags) ? personalTags : Array.Empty<string>()
					);
			}).ToList();
		}

		/// <summary>
		/// Gets Rootobject of JSON result from requesting URL.
		/// </summary>
		/// <param name="url">URL to fetch JSON from.</param>
		/// <returns>Root of requested JSON result.</returns>
		private async Task<ResultJson.Bookmarks.Rootobject> GetBookmarkJsonRoot(string url)
		{
			return await GetJsonRoot<ResultJson.Bookmarks.Rootobject>(url);
		}

		private async Task<ResultJson.Illust.Rootobject> GetWorkJsonRoot(string url)
		{
			return await GetJsonRoot<ResultJson.Illust.Rootobject>(url);
		}

		/// <summary>
		/// Generic helper method for getting JSON root object from a request.
		/// </summary>
		/// <typeparam name="T">Type of JSON root to (try to) parse.</typeparam>
		/// <param name="url">URL to fetch JSON from.</param>
		/// <param name="privateLock">Optional second lock on top of instance-wide lock.</param>
		/// <returns></returns>
		private async Task<T> GetJsonRoot<T>(string url, SemaphoreSlim privateLock = null)
		{
			if (privateLock != null)
			{
				await privateLock.WaitAsync();
			}
			await pool.WaitAsync();
			_ = Task.Delay(_poolInterval).ContinueWith(delegate { pool.Release(); if (privateLock != null) privateLock.Release(); });
			try
			{
				HttpResponseMessage response = (await client.GetAsync(url)).EnsureSuccessStatusCode();
				var result = JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
				return result;
			}
			catch (HttpRequestException)
			{
				return default;
			}
		}

		/// <summary>
		/// Generates bookmark page URL.
		/// </summary>
		/// <param name="offset">URL's offset parameter. Index of first work.</param>
		/// <param name="limit">URL's limit parameter. Maximum number of works to retrieve.</param>
		/// <returns>Bookmark page URL corresponding to the given parameters.</returns>
		private string GenerateBookmarkUrl(int offset, int limit)
		{
			return $"https://www.pixiv.net/ajax/user/{id}/illusts/bookmarks?tag=&offset={offset}&limit={limit}&rest=show";
		}

		/// <summary>
		/// Generates work API URL.
		/// </summary>
		/// <param name="id">ID of work to generate URL for.</param>
		/// <returns>API URL of work with id <paramref name="id"/></returns>
		private string GenerateWorkUrl(int id)
		{
			return $"https://www.pixiv.net/ajax/illust/{id}";
		}

		/// <summary>
		/// Generates pages API URL.
		/// </summary>
		/// <param name="id">Id of work to get pages from.</param>
		/// <returns>URL string of API for given work's pages.</returns>
		private string GeneratePagesUrl(int id)
		{
			return $"https://www.pixiv.net/ajax/illust/{id}/pages";
		}
	}
}

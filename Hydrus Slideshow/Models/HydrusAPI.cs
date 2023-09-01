using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Hydrus_Slideshow.Models
{
    public class HydrusClient
    {
        public class SimpleHydrusFile
        {
#pragma warning disable CS8618
            [JsonProperty("file_id")]
            public int FileId { get; set; }

            [JsonProperty("hash")]
            public string Hash { get; set; }
#pragma warning restore CS8618
        }
        public struct SearchParam
        {
            public FileSortTypes SortType { get; set; }
            public bool AscendingSort { get; set; }

            public SearchParam(FileSortTypes sortType = FileSortTypes.ImportTime, bool ascending = false)
            {
                SortType = sortType;
                AscendingSort = ascending;
            }

        }
        public enum FileSortTypes
        {
            FileSize = 0,
            Duration = 1,
            ImportTime = 2,
            Filetype = 3,
            Random = 4,
            Width = 5,
            Height = 6,
            Ratio = 7,
            NumberOfPixels = 8,
            NumberOfTags = 9,
            NumberOfMediaViews = 10,
            TotalMediaViewTime = 11,
            Bitrate = 12,
            HasAudio = 13,
            ModifiedTime = 14,
            Framerate = 15,
            NumberOfFrames = 16,
            LastViewedTime = 17
        }


        private readonly HttpClient client;
        public HydrusClient(Uri ip, string token)
        {
            client = new HttpClient();
            client.BaseAddress = ip;

            client.DefaultRequestHeaders.Add("Hydrus-Client-API-Access-Key", token);
        }
        private async Task<string> TryGetStringAsync(string request)
        {
            var response = await client.GetAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            var description = await response.Content.ReadAsStringAsync();
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException("Hydrus unauthorized error\r\n" + description),
                System.Net.HttpStatusCode.Forbidden => new UnauthorizedAccessException("Hydrus forbidden error\r\n" + description),
                System.Net.HttpStatusCode.BadRequest => new InvalidOperationException("Hydrus bad request error\r\n" + description),
                _ => new Exception("Hydrus error\r\n" + description),
            };
        }
        private async Task TryPostObjectAsync(string request, object content)
        {
            var response = await client.PostAsJsonAsync(request, content);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var description = await response.Content.ReadAsStringAsync();
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException("Hydrus unauthorized error\r\n" + description),
                System.Net.HttpStatusCode.Forbidden => new UnauthorizedAccessException("Hydrus forbidden error\r\n" + description),
                System.Net.HttpStatusCode.BadRequest => new InvalidOperationException("Hydrus bad request error\r\n" + description),
                _ => new Exception("Hydrus error\r\n" + description),
            };
        }

        public async IAsyncEnumerable<SimpleHydrusFile> SearchFilesSimple(SearchParam searchParam, params string[] tags)
        {
            var tagsFormatted = JsonConvert.SerializeObject(tags);
            var responseSearch = await TryGetStringAsync("/get_files/search_files?" + $"tags={tagsFormatted}" + $"&file_sort_type={(int)searchParam.SortType}" + $"&file_sort_asc={searchParam.AscendingSort.ToString().ToLower()}");

            var ids = JsonConvert.DeserializeAnonymousType(responseSearch, new { file_ids = Array.Empty<int>() })!.file_ids;

            if (ids.Length == 0) yield break;

            //batch request for metadata
            var batches = MakeBatch(ids, 1000);
            foreach (var batch in batches)
            {
                var idsFormated = JsonConvert.SerializeObject(ids);
                var responseMeta = await TryGetStringAsync("/get_files/file_metadata?" + $"file_ids={idsFormated}" + $"&only_return_identifiers=true");

                var metas = JsonConvert.DeserializeAnonymousType(responseMeta, new { metadata = Array.Empty<SimpleHydrusFile>() })!.metadata;

                foreach (var meta in metas)
                {
                    yield return meta;
                }
            }
        }

        public async Task<Stream> GetFile(SimpleHydrusFile file)
        {
            var ms = new MemoryStream();
            var stream = await client.GetStreamAsync("/get_files/file?" + $"file_id={file.FileId}");
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public async Task<bool> TrySendHashToPage(string hash, string pageName)
        {
            var response = await TryGetStringAsync("/manage_pages/get_pages");
            var pagesRoot = JsonConvert.DeserializeObject<RootHydrusPages>(response);

            var page = Flatten(pagesRoot!.Pages.Pages, p => p.Pages).FirstOrDefault(p => p.Name == pageName && p.PageType == 6);

            if (page is null) return false;

            await TryPostObjectAsync("/manage_pages/add_files", new { page_key = page.PageKey, hash = hash });
            return true;
        }

        private static IEnumerable<IEnumerable<TSource>> MakeBatch<TSource>(IEnumerable<TSource> source, int size)
        {
            TSource[]? bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                bucket ??= new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count).ToArray();
        }
        private static IEnumerable<T> Flatten<T>(IEnumerable<T> e, Func<T, IEnumerable<T>> f) => e.SelectMany(c => Flatten(f(c), f)).Concat(e);

        private class RootHydrusPages
        {
#pragma warning disable CS8618
            [JsonProperty("pages")]
            public Page Pages { get; set; }
        }
        private class Page
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("page_key")]
            public string PageKey { get; set; }

            [JsonProperty("page_type")]
            public long PageType { get; set; }

            [JsonProperty("selected")]
            public bool Selected { get; set; }

            [JsonProperty("pages", NullValueHandling = NullValueHandling.Ignore)]
            public Page[] Pages { get; set; } = Array.Empty<Page>();
#pragma warning restore CS8618
        }
    }
}

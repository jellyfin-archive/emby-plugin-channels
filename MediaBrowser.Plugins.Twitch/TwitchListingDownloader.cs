using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;

namespace MediaBrowser.Plugins.Twitch
{
    public class TwitchListingDownloader
    {

        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;

        public TwitchListingDownloader(IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            _jsonSerializer = jsonSerializer;
            _httpClient = httpClient;
        }

        public async Task<RootObject> GetStreamList(String catId, int offset, CancellationToken cancellationToken)
        {
            RootObject reg;

            using (var json = await _httpClient.Get(new HttpRequestOptions
            {
                Url = string.Format("https://api.twitch.tv/kraken/streams?game={CatId}&offset={Offset}", catId, offset)
            }).ConfigureAwait(false))
            {
                reg = _jsonSerializer.DeserializeFromStream<RootObject>(json);
            }

            return reg;
        }
    }
}

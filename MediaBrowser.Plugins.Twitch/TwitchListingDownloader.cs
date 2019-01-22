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

        public async Task<RootObject> GetStreamList(String catID, int offset, CancellationToken cancellationToken)
        {
            RootObject reg;

            using (var json = await _httpClient.Get(string.Format("https://api.twitch.tv/kraken/streams?game={0}&offset={1}", catID, offset), CancellationToken.None).ConfigureAwait(false))
            {
                reg = _jsonSerializer.DeserializeFromStream<RootObject>(json);
            }

            return reg;
        }
    }
}

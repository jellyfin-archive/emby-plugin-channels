using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;

namespace MediaBrowser.Plugins.Twitch
{
    class TwitchChannelDownloader
    {
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;

        public TwitchChannelDownloader(IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            _jsonSerializer = jsonSerializer;
            _httpClient = httpClient;
        }

        public async Task<RootObject> GetTwitchChannelList(int offset, CancellationToken cancellationToken)
        {
            RootObject reg;

            using (var json = await _httpClient.Get(new HttpRequestOptions
            {
                Url = String.Format("https://api.twitch.tv/kraken/games/top?limit=100&offset={0}", offset)
            }).ConfigureAwait(false))
            {
                reg = _jsonSerializer.DeserializeFromStream<RootObject>(json);
            }

            return reg;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.SoundCloud.ClientApi.Model;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Plugins.SoundCloud.ClientApi
{
    public class SoundCloudClient
    {
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly SoundCloudApi _api;

        public SoundCloudClient(ILogger logger, IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            _jsonSerializer = jsonSerializer;
            _httpClient = httpClient;
            _api = new SoundCloudApi(logger, jsonSerializer, httpClient);
        }

        /// <summary>
        /// Gets or sets whether the user is authenticated or not.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return _api.IsAuthenticated;
            }
        }

        public SoundCloudApi Api
        {
            get { return _api; }
        }

        public bool Authenticate(string username, string password)
        {
            var task = _api.Authenticate(username, password, CancellationToken.None);

            return task.Result;
        }

    }
}

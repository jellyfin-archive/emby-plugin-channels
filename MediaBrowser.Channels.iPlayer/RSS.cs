using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Notifications;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;

namespace MediaBrowser.Channels.iPlayer
{
    class RSS
    {
        public IEnumerable<ChannelMediaInfo> Children { get; private set; }

        string url;
        RssFeedReader _feed;

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public RSS(string url, IHttpClient httpClient, IJsonSerializer jsonSerializer, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            this.url = url;
        }

        public async Task Refresh(CancellationToken cancellationToken)
        {
            try
            {
                var httpRequest = await _httpClient.Get(new HttpRequestOptions
                {
                    Url = url,
                    CancellationToken = cancellationToken
                }).ConfigureAwait(false);

                using (XmlReader reader = new XmlTextReader(httpRequest))
                {
                    _feed = new RssFeedReader(reader);

                    Children = await GetChildren(_feed, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading feed from {Url}", url);
            }
        }

        private async Task<IEnumerable<ChannelMediaInfo>> GetChildren(
            RssFeedReader feed,
            CancellationToken cancellationToken)
        {
            var items = new List<ChannelMediaInfo>();

            if (feed == null) return items;

            _logger.LogDebug("Processing Feed");

            while (await feed.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (feed.ElementType != SyndicationElementType.Item) {
                    _logger.LogWarning("RSS reader probably broken, getting non-item elements");
                    continue;
                }

                var item = await feed.ReadItem();

                foreach (var link in item.Links)
                {
                    // TODO: Put these in items so they can be returned
                    _logger.LogDebug("Link Title: {Title}", link.Title);
                    _logger.LogDebug("URI: {Uri}", link.Uri);
                    _logger.LogDebug("RelationshipType: {Type}", link.RelationshipType);
                    _logger.LogDebug("MediaType: {Type}", link.MediaType);
                    _logger.LogDebug("Length: {Length}", link.Length);
                }
            }

            return items;
        }

        /// <summary>
        /// The audio file extensions
        /// </summary>
        public static readonly string[] AudioFileExtensions = new[]
            {
                ".mp3",
                ".flac",
                ".wma",
                ".aac",
                ".acc",
                ".m4a",
                ".m4b",
                ".wav",
                ".ape",
                ".ogg",
                ".oga"

            };

        private static readonly Dictionary<string, string> AudioFileExtensionsDictionary = AudioFileExtensions.ToDictionary(i => i, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether [is audio file] [the specified args].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if [is audio file] [the specified args]; otherwise, <c>false</c>.</returns>
        public static bool IsAudioFile(string path)
        {
            var extension = Path.GetExtension(path);

            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            return AudioFileExtensionsDictionary.ContainsKey(extension);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.Trailers.Extensions;
using Microsoft.Extensions.Logging;

// TODO: this file passes around Logger rather than storing it :( fix that

namespace MediaBrowser.Plugins.Trailers.Providers.AP
{
    /// <summary>
    /// Fetches Apple's list of current movie trailers
    /// </summary>
    public static class TrailerListingDownloader
    {
        /// <summary>
        /// The trailer feed URL
        /// </summary>
        private const string HDTrailerFeedUrl = "http://trailers.apple.com/trailers/home/xml/current_720p.xml";

        private const string TrailerFeedUrl = "http://trailers.apple.com/trailers/home/xml/current_480p.xml";

        /// <summary>
        /// Downloads a list of trailer info's from the apple url
        /// </summary>
        /// <returns>Task{List{TrailerInfo}}.</returns>
        public static async Task<List<TrailerInfo>> GetTrailerList(
            ILogger logger,
            bool hdTrailerList,
            CancellationToken cancellationToken)
        {
            var url = hdTrailerList ?
                HDTrailerFeedUrl :
                TrailerFeedUrl;

            var stream = await EntryPoint.Instance.GetAndCacheResponse(url, TimeSpan.FromDays(7), cancellationToken)
                        .ConfigureAwait(false);

            var list = new List<TrailerInfo>();

            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true }))
            {
                await reader.MoveToContentAsync().ConfigureAwait(false);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "movieinfo":
                                var trailer = FetchTrailerInfo(reader.ReadSubtree(), logger);
                                list.Add(trailer);
                                break;
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Fetches trailer info from an xml node
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>TrailerInfo.</returns>
        private static TrailerInfo FetchTrailerInfo(XmlReader reader, ILogger logger)
        {
            var trailerInfo = new TrailerInfo { };

            reader.MoveToContent();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "info":
                            FetchInfo(reader.ReadSubtree(), trailerInfo, logger);
                            break;
                        case "cast":
                            FetchCast(reader.ReadSubtree(), trailerInfo);
                            break;
                        case "genre":
                            FetchGenres(reader.ReadSubtree(), trailerInfo);
                            break;
                        case "poster":
                            FetchPosterUrl(reader.ReadSubtree(), trailerInfo);
                            break;
                        case "preview":
                            FetchTrailerUrl(reader.ReadSubtree(), trailerInfo);
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }

            return trailerInfo;
        }

        private static readonly CultureInfo UsCulture = new CultureInfo("en-US");

        /// <summary>
        /// Fetches from the info node
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="info">The info.</param>
        private static void FetchInfo(XmlReader reader, TrailerInfo info, ILogger logger)
        {
            reader.MoveToContent();
            reader.Read();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "title":
                        info.Name = reader.ReadStringSafe();
                        break;
                    case "runtime":
                        {
                            var val = reader.ReadStringSafe();

                            if (!string.IsNullOrWhiteSpace(val))
                            {
                                var parts = val.Split(':');

                                if (parts.Length == 2)
                                {
                                    int mins;
                                    int secs;

                                    if (int.TryParse(parts[0], NumberStyles.Any, UsCulture, out mins) &&
                                        int.TryParse(parts[1], NumberStyles.Any, UsCulture, out secs))
                                    {
                                        var totalSeconds = (mins*60) + secs;

                                        info.RunTimeTicks = TimeSpan.FromSeconds(totalSeconds).Ticks;
                                    }
                                }
                            }
                            break;
                        }
                    case "rating":
                        {
                            var rating = reader.ReadStringSafe();

                            if (!string.IsNullOrWhiteSpace(rating) && !string.Equals("not yet rated", rating, StringComparison.OrdinalIgnoreCase))
                            {
                                info.OfficialRating = rating;
                            }
                            break;
                        }
                    case "studio":
                        {
                            var studio = reader.ReadStringSafe();

                            if (!string.IsNullOrWhiteSpace(studio))
                            {
                                info.Studios.Add(studio);
                            }
                            break;
                        }
                    case "postdate":
                        {
                            DateTime date;

                            if (DateTime.TryParse(reader.ReadStringSafe(), UsCulture, DateTimeStyles.None, out date))
                            {
                                info.PostDate = date.ToUniversalTime();
                            }
                            break;
                        }
                    case "releasedate":
                        {
                            var val = reader.ReadStringSafe();

                            if (!string.IsNullOrWhiteSpace(val))
                            {
                                DateTime date;

                                if (DateTime.TryParse(val, UsCulture, DateTimeStyles.None, out date))
                                {
                                    info.PremiereDate = date.ToUniversalTime();
                                    info.ProductionYear = date.Year;
                                }
                            }

                            break;
                        }
                    case "director":
                        {
                            var directors = reader.ReadStringSafe() ?? string.Empty;

                            foreach (var director in Split(directors, ',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                var name = director.Trim();

                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    info.People.Add(new PersonInfo { Name = name, Type = PersonType.Director });
                                }
                            }
                            break;
                        }
                    case "description":
                        info.Overview = reader.ReadStringSafe();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        /// <summary>
        /// Provides an additional overload for string.split
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="options">The options.</param>
        /// <returns>System.String[][].</returns>
        private static string[] Split(string val, char separator, StringSplitOptions options)
        {
            return val.Split(new[] { separator }, options);
        }

        /// <summary>
        /// Fetches from the genre node
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="info">The info.</param>
        private static void FetchGenres(XmlReader reader, TrailerInfo info)
        {
            reader.MoveToContent();
            reader.Read();

            while (reader.IsStartElement())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "name":
                            {
                                var val = reader.ReadStringSafe();

                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    info.Genres.Add(val);
                                }
                                break;
                            }
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Fetches from the cast node
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="info">The info.</param>
        private static void FetchCast(XmlReader reader, TrailerInfo info)
        {
            reader.MoveToContent();
            reader.Read();

            while (reader.IsStartElement())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "name":
                            {
                                var name = reader.ReadStringSafe();

                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    info.People.Add(new PersonInfo { Name = name, Type = PersonType.Actor });
                                }
                                break;
                            }
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Fetches from the preview node
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="info">The info.</param>
        private static void FetchTrailerUrl(XmlReader reader, TrailerInfo info)
        {
            reader.MoveToContent();
            reader.Read();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "large":
                        info.TrailerUrl = reader.ReadStringSafe();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

        }

        /// <summary>
        /// Fetches from the poster node
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="info">The info.</param>
        private static void FetchPosterUrl(XmlReader reader, TrailerInfo info)
        {
            reader.MoveToContent();
            reader.Read();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "location":
                        info.ImageUrl = reader.ReadStringSafe();
                        break;
                    case "xlarge":
                        info.HdImageUrl = reader.ReadStringSafe();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

        }

    }
}

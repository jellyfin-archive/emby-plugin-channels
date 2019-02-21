using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Plugins.ITV
{
    public class Channel : IChannel, IRequiresMediaInfoCallback
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public Channel(IHttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger(GetType().Name);
        }

        public string DataVersion
        {
            get
            {
                // Increment as needed to invalidate all caches
                return "6";
            }
        }

        public string Description
        {
            get { return string.Empty; }
        }

        public string HomePageUrl
        {
            get { return "http://www.itv.com"; }
        }

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            _logger.LogDebug("cat ID : {Id}", query.FolderId);

            if (query.FolderId == null)
            {
                return await GetMainMenu(cancellationToken).ConfigureAwait(false);
            }

            var folderID = query.FolderId.Split('_');
            query.FolderId = folderID[1];

            if (folderID[0] == "programs")
            {
                return await GetProgramList(query, cancellationToken).ConfigureAwait(false);
            }
            if (folderID[0] == "episodes")
            {
                query.SortDescending = true;
                query.SortBy = ChannelItemSortField.PremiereDate;
                return await GetEpisodeList(query, cancellationToken).ConfigureAwait(false);
            }
            if (folderID[0] == "genres")
            {
                return await GetGenreList(query, cancellationToken).ConfigureAwait(false);
            }
            if (folderID[0] == "tvchannels")
            {
                return await GetTVChannelList(query, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        private async Task<ChannelItemResult> GetMainMenu(CancellationToken cancellationToken)
        {
            // Add more items here.
            var items = new List<ChannelItemInfo>
            {
                new ChannelItemInfo
                {
                    Name = "Most Popular Programmes",
                    Id = "programs_" + "https://www.itv.com/itvplayer/categories/browse/popular/catch-up",
                    Type = ChannelItemType.Folder
                },
                new ChannelItemInfo
                {
                    Name = "Genres",
                    Id = "genres_",
                    Type = ChannelItemType.Folder
                },
                new ChannelItemInfo
                {
                    Name = "TV Channels",
                    Id = "tvchannels_",
                    Type = ChannelItemType.Folder
                }
            };

            return new ChannelItemResult
            {
                Items = items.ToList()
            };
        }

        private async Task<ChannelItemResult> GetProgramList(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            var page = new HtmlDocument();
            var items = new List<ChannelItemInfo>();

            using (var site = await _httpClient.Get(query.FolderId, CancellationToken.None).ConfigureAwait(false))
            {
                page.Load(site, Encoding.UTF8);

                foreach (var node in page.DocumentNode.SelectNodes("//div[@id='categories-content']/div[@class='item-list']/ul/li"))
                {
                    var thumb = node.SelectSingleNode(".//div[@class='min-container']//img");
                    var title = node.SelectSingleNode(".//div[@class='programme-title cell-title']/a").InnerText.Replace("&#039;", "'");
                    var url = "http://www.itv.com" + node.SelectSingleNode(".//div[@class='programme-title cell-title']/a").Attributes["href"].Value;

                    items.Add(new ChannelItemInfo
                    {
                        Name = title,
                        ImageUrl = thumb != null ? thumb.Attributes["src"].Value.Replace("player_image_thumb_standard", "posterframe") : "",
                        Id = "episodes_" + url,
                        Type = ChannelItemType.Folder
                    });
                }
            }

            return new ChannelItemResult
            {
                Items = items.ToList()
            };
        }

        private async Task<ChannelItemResult> GetEpisodeList(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            var page = new HtmlDocument();
            var items = new List<ChannelItemInfo>();

            using (var site = await _httpClient.Get(query.FolderId, CancellationToken.None).ConfigureAwait(false))
            {
                page.Load(site);

                var nodes = page.DocumentNode.SelectNodes("//div[@class='view-content']/div[@class='views-row']");

                // Cant find multiple episodes so means only one episode in program so look at main video on page
                if (nodes == null)
                    nodes = page.DocumentNode.SelectNodes("//div[@class='hero']");

                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var id = query.FolderId;
                        if (node.SelectSingleNode(".//div[contains(@class, 'node-episode')]/a[1]") != null)
                            id = node.SelectSingleNode(".//div[contains(@class, 'node-episode')]/a[1]").Attributes["href"].Value;
                        else if (node.SelectSingleNode("./a") != null)
                            id = node.SelectSingleNode("./a[1]").Attributes["href"].Value;

                        var title = "Unknown";
                        if (node.SelectSingleNode("//h2[@class='title episode-title']") != null)
                            title = node.SelectSingleNode("//h2[@class='title episode-title']").InnerText.Replace("&#039;", "'");
                        else if (node.SelectSingleNode(".//div[@class='programme-title']//text()") != null)
                            title = node.SelectSingleNode(".//div[@class='programme-title']//text()").InnerText.Replace("&#039;", "'");

                        var seasonNumber = "";
                        if (node.SelectSingleNode(".//div[contains(@class, 'field-name-field-season-number')]//text()") != null)
                            seasonNumber = node.SelectSingleNode(".//div[contains(@class, 'field-name-field-season-number')]//text()").InnerText;

                        var episodeNumber = "";
                        if (node.SelectSingleNode(".//div[contains(@class, 'field-name-field-episode-number')]//text()") != null)
                            episodeNumber = node.SelectSingleNode(".//div[contains(@class, 'field-name-field-episode-number')]//text()").InnerText;

                        var overview = "";
                        if (node.SelectSingleNode(".//div[contains(@class,'field-name-field-short-synopsis')]//text()") != null)
                            overview = node.SelectSingleNode(".//div[contains(@class,'field-name-field-short-synopsis')]//text()").InnerText;

                        var thumb = "";
                        if (node.SelectSingleNode(".//div[contains(@class,'field-name-field-image')]//img") != null)
                            thumb = node.SelectSingleNode(".//div[contains(@class,'field-name-field-image')]//img").Attributes["src"].Value;
                        else if (node.SelectSingleNode(".//param[@name='poster']") != null)
                            thumb = node.SelectSingleNode(".//param[@name='poster']").Attributes["value"].Value;

                        var date = "";
                        if (node.SelectSingleNode(".//span[contains(@class,'date-display-single')]") != null)
                            date = node.SelectSingleNode(".//span[contains(@class,'date-display-single')]").Attributes["content"].Value;

                        // TODO : FIX ME !
                        var durText = node.SelectSingleNode(".//div[contains(@class,'field-name-field-duration')]//div[contains(@class, 'field-item')]").InnerText;

                        var hour = 0;
                        var hourNode = Regex.Match(durText, "([1-9][0-9]*) hour");
                        if (hourNode.Success)
                        {
                            hour = Convert.ToInt16(hourNode.Groups[0].Value.Replace(" hour", "")) * 60;
                        }

                        var minute = 0;
                        var minuteNode = Regex.Match(durText, "([1-9][0-9]*) minute");
                        if (minuteNode.Success)
                        {
                            minute = Convert.ToInt16(minuteNode.Groups[0].Value.Replace(" minute", ""));
                        }

                        items.Add(new ChannelItemInfo
                        {
                            Name = title + " (Season: " + seasonNumber + ", Ep: " + episodeNumber + ")",

                            ImageUrl = thumb != "" ? thumb.Replace("player_image_thumb_standard", "posterframe") : "",
                            Id = "http://www.itv.com" + id,
                            Overview = overview,
                            Type = ChannelItemType.Media,
                            ContentType = ChannelMediaContentType.Episode,
                            IsInfiniteStream = false,
                            MediaType = ChannelMediaType.Video,
                            PremiereDate = date != "" ? DateTime.Parse(date) : DateTime.MinValue,
                            RunTimeTicks = TimeSpan.FromMinutes(hour + minute).Ticks
                        });
                    }

                    items = items.OrderByDescending(i => i.PremiereDate).ToList();
                }
                else
                {
                    // No Episodes Found! Return Error
                }

            }

            return new ChannelItemResult
            {
                Items = items.ToList()
            };
        }

        private async Task<ChannelItemResult> GetGenreList(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            var data = new Data();
            var items = new List<ChannelItemInfo>();

            foreach (var genre in data.Genres)
            {
                items.Add(new ChannelItemInfo
                {
                    Name = genre.name,
                    Id = "programs_" + "https://www.itv.com/itvplayer/categories/" + genre.fname + "/popular/catch-up",
                    Type = ChannelItemType.Folder,
                    ImageUrl = genre.thumb
                });
            }

            return new ChannelItemResult
            {
                Items = items.ToList()
            };
        }

        private async Task<ChannelItemResult> GetDateList(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            var items = new List<ChannelItemInfo>
            {
                new ChannelItemInfo
                {
                    Name = "Today",
                    Id = "date_" + "https://www.itv.com/itvplayer/by-day/today",
                    Type = ChannelItemType.Folder
                },
                new ChannelItemInfo
                {
                    Name = "Yesturday",
                    Id = "date_" + "https://www.itv.com/itvplayer/by-day/yesturday",
                    Type = ChannelItemType.Folder
                }
            };

            var today = DateTime.Now;

            for (int i = 0; i < 31; i++)
            {
                var d = today.AddDays(-i);
                items.Add(new ChannelItemInfo
                {
                    Name = d.ToString("ddd, d MMM yyyy"),
                    Id = "date_" + "https://www.itv.com/itvplayer/by-day/" + d.ToString("d-MMM-yyyy"),
                    Type = ChannelItemType.Folder
                });
            }

            return new ChannelItemResult
            {
                Items = items.ToList()
            };
        }

        private async Task<ChannelItemResult> GetTVChannelList(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            var data = new Data();
            var items = new List<ChannelItemInfo>();

            foreach (var channel in data.TVChannel)
            {
                items.Add(new ChannelItemInfo
                {
                    Name = channel.name,
                    ImageUrl = channel.thumb,
                    Id = "programs_" + "https://www.itv.com/itvplayer/categories/" + channel.fname + "/popular/catch-up",
                    Type = ChannelItemType.Folder
                });
            }

            return new ChannelItemResult
            {
                Items = items.ToList()
            };
        }

        public async Task<IEnumerable<ChannelMediaInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
        {
            var page = new HtmlDocument();
            var items = new List<ChannelMediaInfo>();
            using (var site = await _httpClient.Get(id, CancellationToken.None).ConfigureAwait(false))
            {
                using (var reader = new StreamReader(site))
                {
                    var html = await reader.ReadToEndAsync().ConfigureAwait(false);
                    html = html.Replace("&#039", "'");

                    var productionNode = Regex.Match(html, "\"productionId\":\"(.*?)\"", RegexOptions.IgnoreCase);
                    var productionID = productionNode.Groups[0].Value;

                    productionID = productionID.Replace(@"\", "");
                    productionID = productionID.Replace("\"productionId\":\"", "");
                    productionID = productionID.Replace("\"", "");

                    _logger.LogDebug("Production ID : {Id}", productionID);

                    var SM_TEMPLATE =
                        String.Format(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"" xmlns:itv=""http://schemas.datacontract.org/2004/07/Itv.BB.Mercury.Common.Types"" xmlns:com=""http://schemas.itv.com/2009/05/Common"">
	                  <soapenv:Header/>
	                  <soapenv:Body>
		                <tem:GetPlaylist>
		                  <tem:request>
		                <itv:ProductionId>{0}</itv:ProductionId>
		                <itv:RequestGuid>FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF</itv:RequestGuid>
		                <itv:Vodcrid>
		                  <com:Id/>
		                  <com:Partition>itv.com</com:Partition>
		                </itv:Vodcrid>
		                  </tem:request>
		                  <tem:userInfo>
		                <itv:Broadcaster>Itv</itv:Broadcaster>
		                <itv:GeoLocationToken>
		                  <itv:Token/>
		                </itv:GeoLocationToken>
		                <itv:RevenueScienceValue>ITVPLAYER.12.18.4</itv:RevenueScienceValue>
		                <itv:SessionId/>
		                <itv:SsoToken/>
		                <itv:UserToken/>
		                  </tem:userInfo>
		                  <tem:siteInfo>
		                <itv:AdvertisingRestriction>None</itv:AdvertisingRestriction>
		                <itv:AdvertisingSite>ITV</itv:AdvertisingSite>
		                <itv:AdvertisingType>Any</itv:AdvertisingType>
		                <itv:Area>ITVPLAYER.VIDEO</itv:Area>
		                <itv:Category/>
		                <itv:Platform>DotCom</itv:Platform>
		                <itv:Site>ItvCom</itv:Site>
	                  </tem:siteInfo>
	                  <tem:deviceInfo>
		                <itv:ScreenSize>Big</itv:ScreenSize>
	                  </tem:deviceInfo>
	                  <tem:playerInfo>
		                <itv:Version>2</itv:Version>
	                  </tem:playerInfo>
		                </tem:GetPlaylist>
	                  </soapenv:Body>
	                </soapenv:Envelope>
	                ", productionID);

                    // TODO: Need to convert this to httpclient for compatibility

                    var request = (HttpWebRequest)WebRequest.Create("http://mercury.itv.com/PlaylistService.svc");
                    request.ContentType = "text/xml; charset=utf-8";
                    request.ContentLength = SM_TEMPLATE.Length;
                    request.Referer = "http://www.itv.com/mercury/Mercury_VideoPlayer.swf?v=1.6.479/[[DYNAMIC]]/2";
                    request.Headers.Add("SOAPAction", "http://tempuri.org/PlaylistService/GetPlaylist");
                    request.Host = "mercury.itv.com";
                    request.Method = "POST";

                    var requestWriter = new StreamWriter(request.GetRequestStream());

                    try
                    {
                        requestWriter.Write(SM_TEMPLATE);
                    }
                    finally
                    {
                        requestWriter.Close();
                        requestWriter = null;
                    }

                    var response = (HttpWebResponse)request.GetResponse();
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        var videoNode = Regex.Match(sr.ReadToEnd(), "<VideoEntries>(.*?)</VideoEntries>",
                            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
                        var video = videoNode.Groups[0].Value;

                        page.LoadHtml(video);

                        var videoPageNode = page.DocumentNode.SelectSingleNode("/videoentries/video/mediafiles");
                        var rtmp = videoPageNode.Attributes["base"].Value;
                        _logger.LogDebug(rtmp);

                        foreach (var node in videoPageNode.SelectNodes(".//mediafile"))
                        {
                            var bitrate = node.Attributes["bitrate"].Value;
                            var url = node.SelectSingleNode(".//url").InnerText;
                            var strippedURL = url.Replace("<![CDATA[", "").Replace("]]>", "");

                            var playURL = rtmp + " swfurl=http://www.itv.com/mercury/Mercury_VideoPlayer.swf playpath=" +
                                          strippedURL + " swfvfy=true";

                            items.Add(new ChannelMediaInfo
                            {
                                Path = playURL,
                                VideoBitrate = Convert.ToInt32(bitrate),
                                Protocol = MediaProtocol.Rtmp,
                                ReadAtNativeFramerate = true
                            });
                            _logger.LogDebug(strippedURL);
                        }
                    }
                }

                return items.OrderByDescending(i => i.VideoBitrate ?? 0);
            }
        }

        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Thumb:
                case ImageType.Backdrop:
                    {
                        var path = GetType().Namespace + ".Images." + type.ToString().ToLower() + ".png";

                        return Task.FromResult(new DynamicImageResponse
                        {
                            Format = ImageFormat.Png,
                            HasImage = true,
                            Stream = GetType().Assembly.GetManifestResourceStream(path)
                        });
                    }
                default:
                    throw new ArgumentException("Unsupported image type: " + type);
            }
        }


        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType>
            {
                ImageType.Thumb,
                ImageType.Backdrop
            };
        }

        public string Name
        {
            get { return "ITV UK"; }
        }

        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Clip
                },

                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                }
            };
        }

        public bool IsEnabledFor(string userId)
        {
            return true;
        }

        public ChannelParentalRating ParentalRating
        {
            get { return ChannelParentalRating.GeneralAudience; }
        }
    }
}

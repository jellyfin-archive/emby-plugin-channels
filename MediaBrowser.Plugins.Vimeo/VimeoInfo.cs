using System.Collections.Generic;

namespace MediaBrowser.Plugins.Vimeo
{
    public class Mobile
    {
        public int profile { get; set; }
        public string origin { get; set; }
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public int id { get; set; }
        public int bitrate { get; set; }
        public int availability { get; set; }
    }

    public class Hd
    {
        public int profile { get; set; }
        public string origin { get; set; }
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public int id { get; set; }
        public int bitrate { get; set; }
        public int availability { get; set; }
    }

    public class Sd
    {
        public int profile { get; set; }
        public string origin { get; set; }
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public int id { get; set; }
        public int bitrate { get; set; }
        public int availability { get; set; }
    }

    public class H264
    {
        public Mobile mobile { get; set; }
        public Hd hd { get; set; }
        public Sd sd { get; set; }

        public H264()
        {
            mobile = new Mobile();
            hd = new Hd();
            sd = new Sd();
        }
    }

    public class Vp6
    {
        public Sd sd { get; set; }
    }

    public class Hls
    {
        public string all { get; set; }
        public string hd { get; set; }
    }

    public class Files
    {
        public H264 h264 { get; set; }
        public Hls hls { get; set; }
        public Vp6 vp6 { get; set; }
        public List<string> codecs { get; set; }

        public Files()
        {
            h264 = new H264();
            hls = new Hls();
            codecs = new List<string>();
        }
    }

    public class Cookie
    {
        public int scaling { get; set; }
        public double volume { get; set; }
        public object hd { get; set; }
        public object captions { get; set; }
    }

    public class Flags
    {
        public int login { get; set; }
        public int preload_video { get; set; }
        public int plays { get; set; }
        public int partials { get; set; }
        public int conviva { get; set; }
    }

    public class Build
    {
        public string player { get; set; }
        public string js { get; set; }
    }

    public class Urls
    {
        public string zeroclip_swf { get; set; }
        public string js { get; set; }
        public string proxy { get; set; }
        public string conviva { get; set; }
        public string flideo { get; set; }
        public string canvas_js { get; set; }
        public string moog { get; set; }
        public string conviva_service { get; set; }
        public string moog_js { get; set; }
        public string zeroclip_js { get; set; }
        public string css { get; set; }
    }

    public class Request
    {
        public Files files { get; set; }
        public string ga_account { get; set; }
        public int timestamp { get; set; }
        public int expires { get; set; }
        public string prefix { get; set; }
        public string session { get; set; }
        public Cookie cookie { get; set; }
        public string cookie_domain { get; set; }
        public object referrer { get; set; }
        public string conviva_account { get; set; }
        public Flags flags { get; set; }
        public Build build { get; set; }
        public Urls urls { get; set; }
        public string signature { get; set; }

        public Request()
        {
            files = new Files();
            flags = new Flags();
            cookie = new Cookie();
            build = new Build();
            urls = new Urls();

        }
    }

    public class Owner
    {
        public string account_type { get; set; }
        public string name { get; set; }
        public string img { get; set; }
        public string url { get; set; }
        public string img_2x { get; set; }
        public int id { get; set; }
    }

    public class Thumbs
    {
        public string __invalid_name__1280 { get; set; }
        public string __invalid_name__960 { get; set; }
        public string __invalid_name__640 { get; set; }
    }

    public class Video
    {
        public int allow_hd { get; set; }
        public int height { get; set; }
        public Owner owner { get; set; }
        public Thumbs thumbs { get; set; }
        public int duration { get; set; }
        public int id { get; set; }
        public int hd { get; set; }
        public string embed_code { get; set; }
        public int default_to_hd { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string privacy { get; set; }
        public string share_url { get; set; }
        public int width { get; set; }
        public string embed_permission { get; set; }
        public double fps { get; set; }

        public Video()
        {
            owner = new Owner();
            thumbs = new Thumbs();
        }
    }

    public class Build2
    {
        public string player { get; set; }
        public string rpc { get; set; }
    }

    public class Badge
    {
        public string name { get; set; }
        public string img { get; set; }
        public string svg { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string link { get; set; }
        public string img_2x { get; set; }
    }

    public class Settings
    {
        public int fullscreen { get; set; }
        public int byline { get; set; }
        public int like { get; set; }
        public int playbar { get; set; }
        public int title { get; set; }
        public int color { get; set; }
        public int branding { get; set; }
        public int share { get; set; }
        public int scaling { get; set; }
        public int logo { get; set; }
        public int info_on_pause { get; set; }
        public int watch_later { get; set; }
        public int portrait { get; set; }
        public int embed { get; set; }
        public Badge badge { get; set; }
        public int volume { get; set; }

        public Settings()
        {
            badge = new Badge();
        }
    }

    public class Embed
    {
        public object player_id { get; set; }
        public string outro { get; set; }
        public int api { get; set; }
        public string context { get; set; }
        public int time { get; set; }
        public string color { get; set; }
        public Settings settings { get; set; }
        public int on_site { get; set; }
        public int loop { get; set; }
        public int autoplay { get; set; }

        public Embed()
        {
            settings = new Settings();
        }
    }

    public class User
    {
        public int liked { get; set; }
        public string account_type { get; set; }
        public int logged_in { get; set; }
        public int owner { get; set; }
        public int watch_later { get; set; }
        public int id { get; set; }
        public bool mod { get; set; }
    }

    public class RootObject
    {
        public string cdn_url { get; set; }
        public int view { get; set; }
        public Request request { get; set; }
        public string player_url { get; set; }
        public Video video { get; set; }
        public Build2 build { get; set; }
        public Embed embed { get; set; }
        public string vimeo_url { get; set; }
        public User user { get; set; }

        public RootObject()
        {
            request = new Request();
            video = new Video();
            build = new Build2();
            embed = new Embed();
            user = new User();
        }
    }
}

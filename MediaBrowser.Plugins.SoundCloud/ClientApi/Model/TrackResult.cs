using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.SoundCloud.ClientApi.Model
{
    public class TrackResult : BaseResult
    {
        public string future_href { get; set; }
        public Track[] collection { get; set; }
        public string next_href { get; set; }

        public override string GetNextUrl()
        {
            return this.next_href;
        }
    }
}


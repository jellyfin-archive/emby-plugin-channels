using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Plugins.Trailers.Providers.AP
{
    public class ArchiveProvider : TrailerProvider
    {
        public ArchiveProvider(ILogger logger) : base(logger)
        {
        }

        public override TrailerType TrailerType
        {
            get
            {
                return TrailerType.Archive;
            }
        }
    }
}

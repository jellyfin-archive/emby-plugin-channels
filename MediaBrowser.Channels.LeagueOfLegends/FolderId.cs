using System;
using System.Text.RegularExpressions;

namespace MediaBrowser.Channels.LeagueOfLegends
{
    internal class FolderId
    {
        private const string EventString = "event";
        private const string DayString = "day";
        private const string GameString = "game";

        public FolderIdType FolderIdType { get; private set; }
        public string EventId { get; private set; }
        public string DayId { get; private set; }
        public string GameId { get; private set; }

        public static FolderId ParseFolderId(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
            {
                return new FolderId(FolderIdType.None);
            }
            if (folderId.StartsWith(GameString))
            {
                var match = Helpers.RegexMatch(folderId, "{Game}-(?<gameId>.*)-{Day}-(?<dayId>.*)-{Event}-(?<eventId>.*)", GameString, DayString, EventString);
                return CreateGameFolderId(match.Groups["eventId"].Value, match.Groups["dayId"].Value, match.Groups["gameId"].Value);
            }
            if (folderId.StartsWith(DayString))
            {
                var match = Helpers.RegexMatch(folderId, "{Day}-(?<dayId>.*)-{Event}-(?<eventId>.*)", DayString, EventString);
                return CreateDayFolderId(match.Groups["eventId"].Value, match.Groups["dayId"].Value);
            }
            if (folderId.StartsWith(EventString))
            {
                var match = Helpers.RegexMatch(folderId, "{Event}-(?<eventId>.*)", EventString);
                return CreateEventFolderId(match.Groups["eventId"].Value);
            }
            throw new ArgumentException("Argument format is not recognized. Only pass string which have been returned by FolderId.ToString().", "folderId");
        }

        public static FolderId CreateGameFolderId(string eventId, string dayId, string gameId)
        {
            return new FolderId(FolderIdType.Game)
            {
                EventId = eventId,
                DayId = dayId,
                GameId = gameId
            };
        }

        public static FolderId CreateDayFolderId(string eventId, string dayId)
        {
            return new FolderId(FolderIdType.Day)
            {
                EventId = eventId,
                DayId = dayId
            };
        }

        public static FolderId CreateEventFolderId(string eventId)
        {
            return new FolderId(FolderIdType.Event)
            {
                EventId = eventId
            };
        }

        private FolderId(FolderIdType folderIdType)
        {
            FolderIdType = folderIdType;
        }

        public override string ToString()
        {
            switch (FolderIdType)
            {
                case FolderIdType.Event:
                    return string.Format("{Event}-{EventId}", EventString, EventId);
                case FolderIdType.Day:
                    return string.Format("{Day}-{DayId}-{Event}-{EventId}",
                        DayString, DayId, EventString, EventId);
                case FolderIdType.Game:
                    return string.Format("{Game}-{GameId}-{Day}-{DayId}-{Event}-{EventId}",
                        GameString, GameId, DayString, DayId, EventString, EventId);
                default:
                    throw new NotSupportedException("Unknown FolderIdType: " + FolderIdType);
            }
        }
    }
}

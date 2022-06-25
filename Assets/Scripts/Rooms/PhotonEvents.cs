namespace Rooms
{
    public static class PhotonEvents
    {
        public const byte NewPlayerJoinedRoom = 0;
        public const byte PlayersInRoom = 1;
        public const byte ToggleReady = 2;
        public const byte InitializeGame = 3;
        public const byte GameSubmitted = 4;
        public const byte ExistingGamesSubmitted = 5;
        public const byte RoomChancesSliderChanged = 6;
    }
}
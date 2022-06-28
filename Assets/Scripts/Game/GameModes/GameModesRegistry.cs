using System.Collections.Generic;

namespace Game.GameModes
{
    public static class GameModesRegistry
    {
        public static readonly List<BaseGameMode> GameModes;

        public static readonly BaseGameMode ClassicIndividuals;
        public static readonly BaseGameMode PartyIndividuals;

        static GameModesRegistry()
        {
            GameModes = new List<BaseGameMode>();
            
            ClassicIndividuals = new BaseGameMode("Classic Mode (Individual)");
            // TODO  PartyIndividuals
            
            GameModes.Add(ClassicIndividuals);
        }
    }
}
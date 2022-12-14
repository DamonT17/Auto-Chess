namespace UmbraProjects.AutoChess
{
    // Enum for number of teams in game
    public enum Team {
        Team1,
        Team2,
        Team3,
        Team4,
        Team5,
        Team6,
        Team7,
        Team8
    };

    // Enum for game's various states
    public enum GameState {
        Carousel = 0,
        Prep = 1,
        Fight = 2,
        Buffer = 3
    };
    
    // Enum for Agent's status types
    public enum AgentStatusType {
        Shield,
        Damage,
        Health,
        Mana
    };

    // Enum  for Tile status types
    public enum TileStatusType {
        Default,
        Valid,
        Invalid,
        Origin
    };
    
    
}
using System;
using UmbraProjects.AutoChess.Agents;
using UmbraProjects.Managers;

namespace UmbraProjects.AutoChess
{
    // Inherited manager class to handle all overhead game events
    public static class EventManager {
        public static Action OnRefresh;


        public static Action<Agent> OnAgentDeath;
        public static Action OnRoundStart;
        public static Action OnRoundEnd;
    }
}
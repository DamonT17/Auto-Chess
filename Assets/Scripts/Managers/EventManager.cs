using System;
using UmbraProjects.AutoChess.Agents;
using UmbraProjects.Managers;

namespace UmbraProjects.AutoChess
{
    // Inherited manager class to handle all overhead game events
    public static class EventManager {
        public static Action<int> OnCardClick;
        public static Action<int> OnExperienceClick;
        public static Action<int> OnRefreshClick;
        
        public static Action<int> OnGainXp;
        public static Action<int> OnSpendCoins;
        public static Action<int> OnLevelUp;

        public static Action<Agent> OnAgentDeath;
        public static Action OnRoundStart;
        public static Action OnRoundEnd;
    }
}
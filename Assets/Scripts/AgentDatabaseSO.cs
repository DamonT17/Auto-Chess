using System.Collections.Generic;
using UnityEngine;

// Creation of Agent database for usage in game
[CreateAssetMenu(fileName = "Agent Database", menuName = "CustomSO/Agent Database")]
public class AgentDatabaseSO : ScriptableObject {
    [System.Serializable]
    public struct AgentData {
        public Agent AgentPrefab;
        public Sprite AgentIcon;
        public string AgentName;

        public Sprite OriginIcon;
        public string AgentOrigin;

        public Sprite ClassIcon;
        public string AgentClass;

        public int AgentCost;
        public int AgentNumber;

        public Color BackgroundColor;
    }

    public List<AgentData> AllAgents;
}

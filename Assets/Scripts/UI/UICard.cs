using System.Collections;
using System.Collections.Generic;
using TMPro;
using UmbraProjects.AutoChess;
using UnityEngine;
using UnityEngine.UI;

namespace UmbraProjects.AutoChess.UI {
    // UI class for all Agent Card interactions
    public class UICard : MonoBehaviour {
        // Assignables
        public Image AgentIcon;
        public TextMeshProUGUI AgentName;
        public TextMeshProUGUI AgentCost;

        public Image OriginIcon;
        public TextMeshProUGUI OriginName;

        public Image ClassIcon;
        public TextMeshProUGUI ClassName;

        public Image Background;

        private UIShop _agentShop;
        private AgentDatabaseSO.AgentData _agentData;

        // ABSTRACTION
        // Initialization of each Agent card that is currently shown in the shop
        public void Setup(AgentDatabaseSO.AgentData agentData, UIShop agentShop) {
            AgentIcon.sprite = agentData.AgentIcon;
            AgentName.text = agentData.AgentName;
            AgentCost.text = agentData.AgentCost.ToString();

            OriginIcon.sprite = agentData.OriginIcon;
            OriginName.text = agentData.AgentOrigin;

            ClassIcon.sprite = agentData.ClassIcon;
            ClassName.text = agentData.AgentClass;

            Background.color = agentData.BackgroundColor;

            this._agentShop = agentShop;
            this._agentData = agentData;
        }

        // ABSTRACTION
        // Notify shop that an Agent card has been clicked (button event on card background)
        public void OnClick() {
            _agentShop.OnCardClick(this, _agentData);
        }
    }
}
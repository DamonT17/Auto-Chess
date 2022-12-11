using UnityEngine;

namespace UmbraProjects.AutoChess {
// Class for altering Tile characteristics during game
    public class Tile : MonoBehaviour {
        public SpriteRenderer HighlightSprite;
        public Color[] Colors = new Color[4];
        public int ColorIndex;

        // Set Tile color based on the given index value
        public void SetHighlightColor(int index) {
            HighlightSprite.color = Colors[index];
            ColorIndex = index;
        }

        // Set alpha of tile per the given value
        public void SetAlpha(float a) {
            Color color = HighlightSprite.color;
            color.a = a;

            HighlightSprite.color = color;
        }
    }
}
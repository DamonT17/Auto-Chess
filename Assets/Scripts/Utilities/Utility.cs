using UnityEngine;

namespace UmbraProjects.Utilities {
    public class Utility : MonoBehaviour {
        public static Vector3 GetMousePositionInWorldCoordinates(Camera gameCamera) {
            // Get world coordinates of mouse position
            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);

            var worldPosition = new Vector3(0, 0, 0);

            if (Physics.Raycast(ray, out var hit)) {
                worldPosition = hit.point;
            }

            return worldPosition;
        }

        // Gets tag of game object collided with
        public static string GetColliderTag(Collider objectCollider) {
            return objectCollider.tag;
        }
    }
}

using UnityEngine;

namespace UmbraProjects.Utilities {
    public class Utility : MonoBehaviour {
        public Vector3 GetMousePositionInWorldCoordinates(Camera gameCamera) {
            // Get world coordinates of mouse position
            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);

            var worldPosition = new Vector3(0, 0, 0);

            if (Physics.Raycast(ray, out var hit)) {
                worldPosition = hit.point;
            }

            return worldPosition;
        }   


    }
}

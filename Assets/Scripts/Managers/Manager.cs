using UnityEngine;

// INHERITANCE
// Generic manager class where "T" inherits from Manager<T> class
public class Manager<T> : MonoBehaviour where T : Manager<T> {
    public static T Instance;

    protected void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = (T) this;
        DontDestroyOnLoad(gameObject);
    }
}

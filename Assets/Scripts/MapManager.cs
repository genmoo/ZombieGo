using UnityEngine;

    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance;
        public Grid grid;

        void Awake()
        {
            Instance = this;
        }
    }


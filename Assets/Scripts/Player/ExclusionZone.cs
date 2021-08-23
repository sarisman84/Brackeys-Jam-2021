using UnityEngine;

namespace BrackeysJam2021.Assets.Scripts.Player {
    public struct ExclusionZone {

        public Vector2Int zoneSize;
        public float lifetime;

        public ExclusionZone (Vector2Int zoneSize, float lifetime) {
            this.zoneSize = zoneSize;
            this.lifetime = lifetime;
        }

    }
}
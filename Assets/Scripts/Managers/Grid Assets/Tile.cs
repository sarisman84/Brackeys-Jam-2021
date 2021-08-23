using System;

using BrackeysJam2021.Assets.Scripts.Managers;

using UnityEngine;
using Object = UnityEngine.Object;

namespace BrackeysJam2021.Assets.Scripts.Managers.GridAssets {
    public class Tile {

        public Vector3 position;
        public Vector2Int coordinate;

        private TileType type;

        public Tile (Vector2Int coordinate, Vector3 position, TileType type) {
            this.position = position;
            this.type = type;
            this.coordinate = coordinate;

        }

        public enum TileType {
            Unwalkable,
            Walkable,
            Pickup
        }

        public TileType Type => type;

        public void SetTileType (TileType type) {
            this.type = type;
            switch (type) {
                case TileType.Walkable:
                case TileType.Pickup:
                    PlaneFieldRenderer.SetVisualTileColor (this, PlaneFieldRenderer.GetDefaultVisualTileColor (this));
                    break;

                case TileType.Unwalkable:
                    PlaneFieldRenderer.SetVisualTileColor (this, Color.black);
                    break;
            }
        }
        public PalletObject assignedPallet;

    }

}
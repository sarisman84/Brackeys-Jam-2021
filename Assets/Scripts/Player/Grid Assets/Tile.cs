using UnityEngine;

namespace BrackeysJam2021.Assets.Scripts.Player {
    public class Tile {

        public Vector3 position;
        public Vector2Int coordinate;
        private SpriteRenderer tileRenderer;
        private TileType type;
        private Color defaultTileColor;

        public Tile (Vector2Int coordinate, Vector3 position, TileType type, SpriteRenderer gameObject) {
            this.position = position;
            this.type = type;
            this.coordinate = coordinate;
            gameObject.transform.position = position;
            tileRenderer = gameObject;
            defaultTileColor = tileRenderer.color;
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
                    tileRenderer.color = defaultTileColor;
                    break;

                case TileType.Unwalkable:
                    tileRenderer.color = Color.black;
                    break;
            }
        }
        public PalletObject assignedPallet;

    }

}
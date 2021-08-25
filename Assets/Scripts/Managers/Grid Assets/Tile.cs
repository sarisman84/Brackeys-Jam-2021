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
                    PlaneFieldRenderer.DOTileScale (this, PlaneFieldRenderer.GetVisualTile (this) is { } existing && !existing.Equals (new VisualTile ()) ? existing.renderer.transform.localScale : Vector3.zero, new Vector3 (0.42f, 0.42f, 1), 0.5f);
                    break;
                case TileType.Pickup:
                    // PlaneFieldRenderer.SetVisualTileColor (this, PlaneFieldRenderer.GetDefaultVisualTileColor (this));
                    AudioManager.Play ("Pickup_Spawn");
                    break;

                case TileType.Unwalkable:
                    // PlaneFieldRenderer.SetVisualTileColor (this, PlaneFieldRenderer.TransparentColor);
                    PlaneFieldRenderer.DOTileScale (this, new Vector3 (0.42f, 0.42f, 1), Vector3.zero, 0.5f);
                    break;
            }
        }
        public PalletObject assignedPallet;

    }

}
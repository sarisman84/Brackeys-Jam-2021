using System.Collections.Generic;

using BrackeysJam2021.Assets.Scripts.Managers.GridAssets;

using UnityEngine;

namespace BrackeysJam2021.Assets.Scripts.Managers {
    public static class PlaneFieldRenderer {
        static Dictionary<Vector2Int, VisualTile> registeredTilesToRender = new Dictionary<Vector2Int, VisualTile> ();

        public static void CreateVisualTile (Tile tile, SpriteRenderer tilePrefab) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate)) {
                registeredTilesToRender[tile.coordinate].renderer.gameObject.SetActive (true);
                SetVisualTileColor (tile, GetDefaultVisualTileColor (tile));
            } else {
                SpriteRenderer tileVisual = Object.Instantiate (tilePrefab, GameObject.FindGameObjectWithTag ("Grid").transform);
                tileVisual.transform.position = tile.position;
                registeredTilesToRender.Add (tile.coordinate, new VisualTile (tileVisual));
            }

        }

        public static void RemoveVisualTile (Tile tile) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate)) {
                registeredTilesToRender[tile.coordinate].renderer.gameObject.SetActive (false);
                return;
            }
            Debug.LogWarning ($"Couldnt find visual tile to reset at:({tile.coordinate}). Skipping.");
        }

        public static Color GetDefaultVisualTileColor (Tile tile) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate)) {
                return registeredTilesToRender[tile.coordinate].defaultTileColor;
            }
            Debug.LogWarning ($"Couldnt find corresponding visual tile to get its default color to at:({tile.coordinate}). Skipping.");
            return new Color ();
        }

        public static void SetVisualTileColor (Tile tile, Color newColor) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate)) {
                registeredTilesToRender[tile.coordinate].renderer.color = newColor;
                return;
            }

            Debug.LogWarning ($"Couldnt find corresponding visual tile to set the color to at:({tile.coordinate}). Skipping.");
        }

        public static Color GetVisualTileColor (Tile tile) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate)) {
                return registeredTilesToRender[tile.coordinate].renderer.color;
            }
            Debug.LogWarning ($"Couldnt find corresponding visual tile to get its color to at:({tile.coordinate}). Skipping.");
            return new Color ();
        }

    }

    public struct VisualTile {
        public SpriteRenderer renderer;
        public Color defaultTileColor;

        public VisualTile (SpriteRenderer renderer) {
            this.renderer = renderer;
            defaultTileColor = renderer.color;
        }
    }
}
using System;
using System.Collections.Generic;

using BrackeysJam2021.Assets.Scripts.Managers.GridAssets;

using DG.Tweening;

using UnityEngine;

namespace BrackeysJam2021.Assets.Scripts.Managers {
    public static class PlaneFieldRenderer {
        static Dictionary<Vector2Int, VisualTile> registeredTilesToRender = new Dictionary<Vector2Int, VisualTile> ();

        public static Color TransparentColor => new Color (0, 0, 0, 0);

        public static void CreateVisualTile (Tile tile, SpriteRenderer tilePrefab, Action<VisualTile> onTileDraw = null) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate)) {
                registeredTilesToRender[tile.coordinate].renderer.gameObject.SetActive (true);
                SetVisualTileColor (tile, GetDefaultVisualTileColor (tile));
                onTileDraw?.Invoke (registeredTilesToRender[tile.coordinate]);
            } else {
                SpriteRenderer tileVisual = UnityEngine.Object.Instantiate<SpriteRenderer> (tilePrefab, GameObject.FindGameObjectWithTag ("Grid").transform);
                tileVisual.transform.position = tile.position;
                registeredTilesToRender.Add (tile.coordinate, new VisualTile (tileVisual));
                onTileDraw?.Invoke (registeredTilesToRender[tile.coordinate]);
            }

        }

        public static VisualTile GetVisualTile (Tile tile) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate))
                return registeredTilesToRender[tile.coordinate];
            return new VisualTile ();
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

        public static bool IsTileAlreadyDrawn (Tile origin) {
            return registeredTilesToRender.ContainsKey (origin.coordinate) ? registeredTilesToRender[origin.coordinate].renderer.gameObject.activeSelf : false;
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

        public static void DOTileScale (VisualTile tile, Vector3 initialScale, Vector3 targetScale, float duration) {
            tile.renderer.transform.localScale = initialScale;
            tile.renderer.transform.DOScale (targetScale, duration);
        }

        public static void DOTileScale (Tile tile, Vector3 initialScale, Vector3 targetScale, float duration) {
            if (registeredTilesToRender.ContainsKey (tile.coordinate)) {
                VisualTile vTile = registeredTilesToRender[tile.coordinate];
                vTile.renderer.transform.localScale = initialScale;
                vTile.renderer.transform.DOScale (targetScale, duration);
                return;
            }

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
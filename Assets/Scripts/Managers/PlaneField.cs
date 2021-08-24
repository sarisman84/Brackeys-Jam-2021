using System;
using System.Collections;
using System.Collections.Generic;

using BrackeysJam2021.Assets.Scripts.Managers.GridAssets;

using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BrackeysJam2021.Assets.Scripts.Managers {
    public class PlaneField {

        static Tile[, ] grid;

        public Tile this [Vector2Int coordinates] {
            get => grid[coordinates.x, coordinates.y];
            set => grid[coordinates.x, coordinates.y] = value;
        }
        static List<Pallet> registeredPallets = new List<Pallet> ();

        public static IEnumerator GenerateGrid (Vector3 center, Vector2Int gridSize, SpriteRenderer tilePrefab, Action onGridGenerationCompletion) {
            grid = new Tile[gridSize.x, gridSize.y];

            for (int x = 0; x < grid.GetLength (0); x++) {
                for (int y = 0; y < grid.GetLength (1); y++) {

                    grid[x, y] = new Tile (new Vector2Int (x, y),
                        center + new Vector3 (x - (grid.GetLength (0) / 2f), 0, y - (grid.GetLength (1) / 2f)),
                        Tile.TileType.Walkable);

                }
            }
            yield return DrawSelfAndAdjacentTiles (onGridGenerationCompletion, tilePrefab);

        }

        private static IEnumerator DrawSelfAndAdjacentTiles (Action onGridGenerationCompletion, SpriteRenderer tilePrefab) {

            for (int x = 0; x < grid.GetLength (0); x++) {
                PlaneFieldRenderer.CreateVisualTile (grid[x, 0], tilePrefab);
                for (int y = 1; y <= x; y++) {
                    PlaneFieldRenderer.CreateVisualTile (grid[x - y, y], tilePrefab);
                }

                yield return new WaitForSecondsRealtime (0.05f);
            }

            for (int x = 0; x < grid.GetLength (0); x++) {
                for (int y = grid.GetLength (1) - 1; y >= 0; y--) {
                    if(x + grid.GetLength(0) - y<grid.GetLength(0))
                    PlaneFieldRenderer.CreateVisualTile (grid[x+ grid.GetLength(0)-y, y ], tilePrefab);
                }

                yield return new WaitForSecondsRealtime (0.05f);
            }

            onGridGenerationCompletion?.Invoke ();
        }

        public static void RegisterPallet (float baseSpawnRate, float spawnRateIncrement, GameObject palletPrefab, Action<SnakeController> onPalletPickup) {
            registeredPallets.Add (new Pallet (baseSpawnRate, spawnRateIncrement, palletPrefab, onPalletPickup));
        }

        public static IEnumerator StartGeneratingExclusionZones () {

            int iterations = 1;
            List<Tile> tilesToReset = new List<Tile> ();
            while (true) {
                yield return new WaitForSeconds (10f);

                int zoneCount = iterations;

                for (int count = 0; count < zoneCount; count++) {

                    bool resetTiles = Random.Range (0, 2) == 1;

                    List<Tile> resultingZone = GenerateExclusionZone (new Vector2Int (3, 3), new Vector2Int (11, 11));

                    if (resetTiles)
                        tilesToReset.AddRange (resultingZone);

                }
                iterations++;

                if (tilesToReset.Count > 0) {
                    yield return new WaitForSeconds (5f);
                    foreach (var tile in tilesToReset) {
                        tile.SetTileType (Tile.TileType.Walkable);
                    }
                    tilesToReset.Clear ();
                }

            }
        }

        private static Tile GetTileAtRandomCoordinatesAwayFromThePlayer (Vector2Int zoneSize) {
            Tile foundTile = GetTileAtRandomCoordinates ();

            if (TileIsInsideTheBoundaryOf (foundTile, SnakeController.PlayerCoordinates, zoneSize + new Vector2Int (2, 2))) {
                return GetTileAtRandomCoordinatesAwayFromThePlayer (zoneSize);
            }
            return foundTile;
        }

        private static bool TileIsInsideTheBoundaryOf (Tile tileToCompare, Vector2Int boundaryPosition, Vector2Int boundarySize) {
            Vector2Int posHalfExtends = new Vector2Int (boundaryPosition.x + Mathf.RoundToInt (boundarySize.x / 2f), boundaryPosition.y + Mathf.RoundToInt (boundarySize.y / 2f));
            Vector2Int negHalfExtends = new Vector2Int (boundaryPosition.x - Mathf.RoundToInt (boundarySize.x / 2f), boundaryPosition.y - Mathf.RoundToInt (boundarySize.y / 2f));
            return
            tileToCompare.coordinate.x >= posHalfExtends.x && tileToCompare.coordinate.x <= negHalfExtends.x &&
                tileToCompare.coordinate.y >= posHalfExtends.y && tileToCompare.coordinate.y <= negHalfExtends.y;
        }

        private static List<Tile> GenerateExclusionZone (Vector2Int minSize, Vector2Int maxSize) {

            Vector2Int zoneSize = new Vector2Int (Random.Range (minSize.x, maxSize.x), Random.Range (minSize.y, maxSize.y));

            List<Tile> resultingZone = new List<Tile> ();

            Tile tile = GetTileAtRandomCoordinatesAwayFromThePlayer (zoneSize);

            for (int x = -Mathf.RoundToInt (zoneSize.x / 2f); x < Mathf.RoundToInt (zoneSize.x / 2f) / 2f; x++) {
                for (int y = -Mathf.RoundToInt (zoneSize.y / 2f); y < Mathf.RoundToInt (zoneSize.y / 2f) / 2f; y++) {
                    Vector2Int localCoordinates = tile.coordinate + new Vector2Int (x, y);
                    if (CoordinatesAreOutofBounds (localCoordinates)) continue;
                    Tile foundTile = grid[localCoordinates.x, localCoordinates.y];
                    if (foundTile.Type == Tile.TileType.Pickup) {
                        foundTile.assignedPallet?.RemovePallet ();
                        foundTile.assignedPallet = null;
                    }

                    foundTile.SetTileType (Tile.TileType.Unwalkable);

                    resultingZone.Add (foundTile);
                }
            }

            return resultingZone;
        }

        public static IEnumerator StartGeneratingPallets () {
            for (int i = 0; i < registeredPallets.Count; i++) {
                registeredPallets[i].ModifiedSpawnRate = 0;
            }
            while (true) {

                yield return new WaitForEndOfFrame ();

                for (int i = 0; i < registeredPallets.Count; i++) {
                    Pallet registedPallet = registeredPallets[i];
                    registedPallet.CurrentSpawnRate += Time.deltaTime;

                    if (registedPallet.CurrentSpawnRate >= Mathf.Max (registedPallet.BaseSpawnRate - registedPallet.ModifiedSpawnRate, 0.85f)) {

                        Tile randomTile = GetTileAtRandomCoordinates ();

                        if (randomTile != null) {
                            randomTile.assignedPallet = new PalletObject (registedPallet.PalletPrefab, randomTile.position, registedPallet.OnPalletPickup);
                            randomTile.SetTileType (Tile.TileType.Pickup);
                        }
                        registedPallet.CurrentSpawnRate = 0;
                        registedPallet.ModifiedSpawnRate += registedPallet.SpawnRateIncrement;

                    }
                }
            }

        }

        public static void ResetGrid () {
            foreach (var tile in grid) {

                PlaneFieldRenderer.RemoveVisualTile (tile);
                tile.assignedPallet?.RemovePallet ();
                tile.assignedPallet = null;
            }

        }

        public static Tile[, ] Grid => grid;

        public static Vector2Int Center => new Vector2Int (Mathf.RoundToInt (grid.GetLength (0) / 2f), Mathf.RoundToInt (grid.GetLength (1) / 2f));

        public static Tile GetTileAtCoordinates (Vector2Int coordinates) {
            if (CoordinatesAreOutofBounds (coordinates))
                return null;

            return grid[coordinates.x, coordinates.y];
        }

        public static Tile GetTileAtRandomCoordinates (int depth = 3) {
            if (depth <= 0) return null;

            Tile tile = GetTileAtCoordinates (new Vector2Int (Random.Range (0, grid.GetLength (0)), Random.Range (0, grid.GetLength (1))));

            if (tile == null || tile.Type == Tile.TileType.Unwalkable || tile.Type == Tile.TileType.Pickup)
                return GetTileAtRandomCoordinates (depth - 1);

            return tile;
        }

        private static bool CoordinatesAreOutofBounds (Vector2Int coordinates) {
            return coordinates.x < 0 || coordinates.x >= grid.GetLength (0) || coordinates.y < 0 || coordinates.y >= grid.GetLength (1);
        }
    }

}
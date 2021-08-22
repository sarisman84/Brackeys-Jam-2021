using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BrackeysJam2021.Assets.Scripts.Player {
    public static class PlaneField {

        static Tile[, ] grid;
        static List<Pallet> registeredPallets = new List<Pallet> ();
        public static void GenerateGrid (Vector3 center, Vector2Int gridSize) {
            grid = new Tile[gridSize.x, gridSize.y];

            for (int x = 0; x < grid.GetLength (0); x++) {
                for (int y = 0; y < grid.GetLength (1); y++) {

                    grid[x, y] = new Tile (center + new Vector3 (x - (grid.GetLength (0) / 2f), 0, y - (grid.GetLength (1) / 2f)), Tile.TileType.Walkable);
                }
            }
        }

        public static void RegisterPallet (float baseSpawnRate, float spawnRateIncrement, GameObject palletPrefab, Action<SnakeController> onPalletPickup) {
            registeredPallets.Add (new Pallet (baseSpawnRate, spawnRateIncrement, palletPrefab, onPalletPickup));
        }

        public static IEnumerator StartGeneratingPallets () {

            while (true) {

                yield return new WaitForEndOfFrame ();

                for (int i = 0; i < registeredPallets.Count; i++) {
                    Pallet registedPallet = registeredPallets[i];
                    registedPallet.CurrentSpawnRate += Time.deltaTime;

                    if (registedPallet.CurrentSpawnRate >= Mathf.Max (registedPallet.BaseSpawnRate - registedPallet.ModifiedSpawnRate, 0.85f)) {

                        Tile randomTile = GetTileAtRandomCoordinates ();

                        if (randomTile != null) {
                            randomTile.assignedPallet = new PalletObject (registedPallet.PalletPrefab, randomTile.position, registedPallet.OnPalletPickup);
                            randomTile.type = Tile.TileType.Pickup;
                        }
                        registedPallet.CurrentSpawnRate = 0;
                        registedPallet.ModifiedSpawnRate += registedPallet.SpawnRateIncrement;

                    }
                }
            }

        }

        public static void ResetGrid () {
            foreach (var tile in grid) {

                tile.type = Tile.TileType.Walkable;
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

            if (tile == null || tile.type == Tile.TileType.Unwalkable || tile.type == Tile.TileType.Pickup)
                return GetTileAtRandomCoordinates (depth - 1);

            return tile;
        }

        private static bool CoordinatesAreOutofBounds (Vector2Int coordinates) {
            return coordinates.x < 0 || coordinates.x >= grid.GetLength (0) || coordinates.y < 0 || coordinates.y >= grid.GetLength (1);
        }
    }

    public class Tile {

        public Vector3 position;

        public Tile (Vector3 position, TileType type) {
            this.position = position;
            this.type = type;
        }

        public enum TileType {
            Unwalkable,
            Walkable,
            Pickup
        }

        public TileType type;
        public PalletObject assignedPallet;

    }

}
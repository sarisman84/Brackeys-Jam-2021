using System;

using UnityEngine;
using Random = UnityEngine.Random;

namespace BrackeysJam2021.Assets.Scripts.Player {
    public static class MovementPlane {

        static Tile[, ] grid;

        public static void GenerateGrid (Vector3 center, Vector2Int gridSize) {
            grid = new Tile[gridSize.x, gridSize.y];

            for (int x = 0; x < grid.GetLength (0); x++) {
                for (int y = 0; y < grid.GetLength (1); y++) {

                    grid[x, y] = new Tile (center + new Vector3 (x - (grid.GetLength (0) / 2f), 0, y - (grid.GetLength (1) / 2f)), Tile.TileType.Walkable);
                }
            }
        }

        public static Tile[, ] Grid => grid;

        public static Vector2Int Center => new Vector2Int (Mathf.RoundToInt (grid.GetLength (0) / 2f), Mathf.RoundToInt (grid.GetLength (1) / 2f));

        public static Tile GetTileAtCoordinates (Vector2Int coordinates) {
            if (CoordinatesAreOutofBounds (coordinates))
                return null;

            return grid[coordinates.x, coordinates.y];
        }

        public static Tile GetTileAtRandomCoordinates () {
            Tile tile = GetTileAtCoordinates (new Vector2Int (Random.Range (0, grid.GetLength (0)), Random.Range (0, grid.GetLength (1))));

            if (tile == null)
                return GetTileAtRandomCoordinates ();

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
            Default,
            Accelerate
        }

        public TileType type;
        public GameObject indicator;
    }
}
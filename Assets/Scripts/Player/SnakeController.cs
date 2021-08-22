using System;
using System.Collections;
using System.Collections.Generic;

using BrackeysJam2021.Assets.Scripts.Player;

using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour {

    public float movementSpeed;
    public InputActionReference movementInput;
    public GameObject tailPrefab;

    Vector2Int currentPosition;
    Vector2Int currentDirection;

    List<Tail> currentTail = new List<Tail> ();

    Coroutine movementCoroutine;

    private void OnEnable () {
        movementInput.action.Enable ();
    }

    private void OnDisable () {
        movementInput.action.Disable ();
    }

    private void Awake () {
        MovementPlane.GenerateGrid (transform.position, new Vector2Int (30, 30));
        StartGame ();
    }

    public void StartGame () {
        PlaneManager.Get.SpawnPallet = true;
        currentPosition = MovementPlane.Center;
        currentDirection = Vector2Int.up;
        movementCoroutine = StartCoroutine (MovePlayer ());
    }

    private IEnumerator MovePlayer () {
        yield return null;

        int initialTailCount = 0;

        while (true) {

            yield return new WaitForSeconds (movementSpeed);

            Vector2Int oldPosition = currentPosition;
            currentPosition += currentDirection;
            Tile resultingTile = MovementPlane.GetTileAtCoordinates (currentPosition);

            if (resultingTile == null) {
                Die ();
                yield break;
            }

            switch (resultingTile.type) {
                case Tile.TileType.Walkable:
                    transform.position = resultingTile.position;
                    UpdateTailPosition (oldPosition);
                    break;

                case Tile.TileType.Default:
                    transform.position = resultingTile.position;
                    currentTail.Add (new Tail (tailPrefab, currentTail));
                    UpdateTailPosition (oldPosition);
                    if (resultingTile.indicator)
                        Destroy (resultingTile.indicator);

                    resultingTile.type = Tile.TileType.Walkable;
                    break;

                case Tile.TileType.Unwalkable:
                    Die ();
                    break;

                default:
                    break;
            }

            if (initialTailCount <= 3) {

                currentTail.Add (new Tail (tailPrefab, currentTail));
                UpdateTailPosition (oldPosition);
                initialTailCount++;
            }

        }

    }

    private void UpdateTailPosition (Vector2Int coordinate) {
        for (int tailIndex = currentTail.Count - 1; tailIndex >= 0; tailIndex--) {
            if (currentTail[tailIndex].currentCoordinate == currentPosition) {
                Die ();
                break;
            }

            if (tailIndex == 0) {

                currentTail[tailIndex].MoveTail (coordinate);
                continue;
            }

            currentTail[tailIndex].MoveTail (currentTail[tailIndex - 1].currentCoordinate);

        }
    }

    public void Die () {
        gameObject.SetActive (false);
        foreach (var tailPart in currentTail) {
            Destroy (tailPart.tailModel);
        }

        StopCoroutine (movementCoroutine);
        currentTail.Clear ();
        PlaneManager.Get.SpawnPallet = false;
        PlaneManager.Get.ClearPallets ();
    }

    private void Update () {
        currentDirection = movementInput.action.ReadValue<Vector2> () is { } input && input != Vector2.zero ?
            GetLockedDirection (new Vector2Int (Mathf.CeilToInt (input.x), Mathf.CeilToInt (input.y))) : currentDirection;
    }

    private Vector2Int GetLockedDirection (Vector2Int input) {
        if (currentDirection == Vector2Int.up) {
            return input == Vector2Int.down ? currentDirection : input;
        }

        if (currentDirection == Vector2Int.down) {
            return input == Vector2Int.up ? currentDirection : input;
        }

        if (currentDirection == Vector2Int.left) {
            return input == Vector2Int.right ? currentDirection : input;
        }

        if (currentDirection == Vector2Int.right) {
            return input == Vector2Int.left ? currentDirection : input;
        }

        return input;
    }

    private void OnDrawGizmos () {
        if (MovementPlane.Grid is { } createdGrid) {

            foreach (var tile in createdGrid) {
                Gizmos.color = (tile.type == Tile.TileType.Walkable ? Color.cyan : tile.type == Tile.TileType.Unwalkable ? Color.red : Color.yellow) - new Color (0, 0, 0, 0.5f);
                Gizmos.DrawCube (tile.position, Vector3.one * 0.8f);
            }
        }
    }

    [Serializable]
    public class Tail {

        public string tailID;
        public Vector2Int currentCoordinate;
        public GameObject tailModel;

        public Tail (GameObject tailModel, List<Tail> currentTail) {
            this.tailModel = UnityEngine.Object.Instantiate (tailModel);
        }

        public void MoveTail (Vector2Int newCoordinate) {

            Tile resultingTile = MovementPlane.GetTileAtCoordinates (newCoordinate);

            switch (resultingTile.type) {

                case Tile.TileType.Walkable:
                case Tile.TileType.Default:
                case Tile.TileType.Accelerate:
                    tailModel.transform.position = resultingTile.position;
                    break;

                case Tile.TileType.Unwalkable:
                    tailModel.SetActive (false);
                    break;
            }

            currentCoordinate = newCoordinate;

        }

    }
}
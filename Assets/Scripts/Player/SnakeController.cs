using System;
using System.Collections;
using System.Collections.Generic;

using BrackeysJam2021.Assets.Manager;
using BrackeysJam2021.Assets.Scripts.Player;

using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour {

    public float movementSpeed;
    public InputActionReference movementInput;
    public GameObject tailPrefab;

    internal Vector2Int currentPosition;
    internal Vector2Int oldPosition;
    internal Vector2Int currentDirection;

    internal List<Tail> currentTail = new List<Tail> ();

    private void OnEnable () {
        movementInput.action.Enable ();
    }

    private void OnDisable () {
        movementInput.action.Disable ();
    }

    public IEnumerator MovePlayer () {
        yield return null;

        int initialTailCount = 0;

        while (true) {

            yield return new WaitForSeconds (movementSpeed);

            oldPosition = currentPosition;
            currentPosition += currentDirection;
            Tile resultingTile = PlaneField.GetTileAtCoordinates (currentPosition);

            if (resultingTile == null) {
                GameManager.Do.EndGame ();
                yield break;
            }

            switch (resultingTile.type) {
                case Tile.TileType.Walkable:
                    transform.position = resultingTile.position;
                    UpdateTailPosition (oldPosition);
                    break;

                case Tile.TileType.Pickup:
                    transform.position = resultingTile.position;

                    if (resultingTile.assignedPallet != null) {
                        resultingTile.assignedPallet.OnPalletPickup?.Invoke (this);
                        resultingTile.assignedPallet.RemovePallet ();
                    }
                    resultingTile.assignedPallet = null;

                    resultingTile.type = Tile.TileType.Walkable;
                    break;

                case Tile.TileType.Unwalkable:
                    GameManager.Do.EndGame ();
                    break;

                default:
                    break;
            }

            if (initialTailCount <= 3) {

                currentTail.Add (new Tail (tailPrefab));
                UpdateTailPosition (oldPosition);
                initialTailCount++;
            }

        }

    }

    public void UpdateTailPosition (Vector2Int coordinate) {
        for (int tailIndex = currentTail.Count - 1; tailIndex >= 0; tailIndex--) {
            if (currentTail[tailIndex].currentCoordinate == currentPosition) {
                GameManager.Do.EndGame ();
                break;
            }

            if (tailIndex == 0) {

                currentTail[tailIndex].MoveTail (coordinate, currentTail);
                continue;
            }

            currentTail[tailIndex].MoveTail (currentTail[tailIndex - 1].currentCoordinate, currentTail);

        }
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

    [Serializable]
    public class Tail {

        public string tailID;
        public Vector2Int currentCoordinate;
        public GameObject tailModel;

        public Tail (GameObject tailModel) {
            this.tailModel = UnityEngine.Object.Instantiate (tailModel);
        }

        public void MoveTail (Vector2Int newCoordinate, List<Tail> currentTail) {

            Tile resultingTile = PlaneField.GetTileAtCoordinates (newCoordinate);

            switch (resultingTile.type) {

                case Tile.TileType.Walkable:
                case Tile.TileType.Pickup:
                    tailModel.transform.position = resultingTile.position;
                    break;

                case Tile.TileType.Unwalkable:
                    Destroy (tailModel);
                    currentTail.Remove (this);
                    break;
            }

            currentCoordinate = newCoordinate;

        }

    }
}
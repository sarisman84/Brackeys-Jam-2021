using System;
using System.Collections;
using System.Collections.Generic;

using BrackeysJam2021.Assets.Manager;
using BrackeysJam2021.Assets.Scripts.Managers;
using BrackeysJam2021.Assets.Scripts.Managers.GridAssets;

using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class SnakeController : MonoBehaviour {

    public float movementSpeed;
    public InputActionReference movementInput;
    public InputActionReference pauseGame;
    public GameObject tailPrefab;

    internal Vector2Int currentDirection;

    public static List<Snake> SnakeEntities { get; private set; } = new List<Snake> ();

    public void ResetSnakes () {
        foreach (var snake in SnakeEntities) {
            snake.RemoveSnakeObject (this);
        }
        SnakeEntities.Clear ();
    }

    public Snake CreateSnake (GameObject snakePrefab, Vector2Int spawnCoordinate, Vector2Int spawnDirection, int initialTailCount = 3, Action onSnakeSpawn = (null)) {
        Snake newSnake = new Snake (snakePrefab, tailPrefab, spawnCoordinate, movementSpeed, (snake) => {
            snake.RemoveSnakeObject (this);
            SnakeEntities.Remove (snake);
            if (SnakeEntities.Count == 0) {
                GameManager.Do.EndGame ();
            }
        }, initialTailCount);
        currentDirection = spawnDirection;

        SnakeEntities.Add (newSnake);
        onSnakeSpawn?.Invoke ();
        return newSnake;
    }

    public List<Snake> SplitSnake (GameObject snakePrefab, int splitThreshold = 5) {
        List<Snake> newSnakes = new List<Snake> ();

        for (int i = 0; i < SnakeEntities.Count; i++) {
            Snake snake = i >= SnakeEntities.Count ? null : SnakeEntities[i];
            if (snake != null)
                if (snake.currentTail.Count > splitThreshold) {

                    Vector2Int curPos = snake.currentTail[5].currentCoordinate;

                    Snake newSnake = CreateSnake (snakePrefab, curPos, currentDirection, Mathf.CeilToInt (splitThreshold / 2f), () => {
                        AudioManager.Play ("Split_Player");
                    });
                    snake.SetTailSize (Mathf.FloorToInt (splitThreshold / 2f));

                    newSnake.movementCoroutine = StartCoroutine (newSnake.Move (this));

                }
        }

        SnakeEntities.AddRange (newSnakes);
        return newSnakes;
    }

    private void OnEnable () {
        movementInput.action.Enable ();
        pauseGame.action.Enable ();
    }

    private void OnDisable () {
        movementInput.action.Disable ();
        pauseGame.action.Disable ();
    }
    private void Update () {

        if (pauseGame.action.ReadValue<float> () > 0 && pauseGame.action.triggered) {
            GameManager.Do.SetPauseState (!PlaneField.isGamePaused);
        }

        if (!PlaneField.isGamePaused)
            currentDirection = movementInput.action.ReadValue<Vector2> () is { } input && input != Vector2.zero ?
            GetLockedDirection (new Vector2Int (Mathf.CeilToInt (input.x), Mathf.CeilToInt (input.y))) : currentDirection;

        print (currentDirection);

    }

    private Vector2Int GetLockedDirection (Vector2Int input) {
        bool isInputDiagonal = input == Vector2Int.one || input == -Vector2Int.one || input == new Vector2Int (1, -1) || input == new Vector2Int (-1, 1);
        if (currentDirection == Vector2Int.up) {
            return input == Vector2Int.down || isInputDiagonal ? currentDirection : input;
        }

        if (currentDirection == Vector2Int.down) {
            return input == Vector2Int.up || isInputDiagonal ? currentDirection : input;
        }

        if (currentDirection == Vector2Int.left) {
            return input == Vector2Int.right || isInputDiagonal ? currentDirection : input;
        }

        if (currentDirection == Vector2Int.right) {
            return input == Vector2Int.left || isInputDiagonal ? currentDirection : input;
        }

        return input;
    }

}

[Serializable]
public class Tail {

    public string tailID;
    public Vector2Int oldCoordinate;
    public Vector2Int currentCoordinate;
    public GameObject tailModel;

    public Tail (GameObject tailModel) {
        this.tailModel = UnityEngine.Object.Instantiate (tailModel, GameObject.FindGameObjectWithTag ("Player").transform);

    }

    public void MoveTail (Vector2Int newCoordinate, List<Tail> currentTail) {
        if (!tailModel) return;
        oldCoordinate = currentCoordinate;
        currentCoordinate = newCoordinate;
        Tile resultingTile = PlaneField.GetTileAtCoordinates (newCoordinate);

        switch (resultingTile.Type) {

            case Tile.TileType.Walkable:
            case Tile.TileType.Pickup:
                tailModel.transform.position = resultingTile.position;
                break;

            case Tile.TileType.Unwalkable:
                Object.Destroy (tailModel);
                currentTail.Remove (this);
                break;
        }

        currentCoordinate = newCoordinate;

    }

}

public class Snake {
    public List<Tail> currentTail;
    public float moddedMovementSpeed;
    public Coroutine movementCoroutine;

    private Vector2Int currentPosition;
    private Vector2Int oldPosition;

    private float movementSpeed;

    private Transform transform;

    public Vector2Int PlayerCoordinate => currentPosition;
    public Vector2Int PlayerOldCoordinate => oldPosition;
    public float PlayerMovementSpeed => movementSpeed;

    private Action<Snake> onSnakeDeath;

    public Snake (GameObject snakePrefab, GameObject tailPrefab, Vector2Int spawningCoordinate, float baseMovementSpeed, Action<Snake> onSnakeDeath, int initialTailSize) {
        transform = Object.Instantiate (snakePrefab).transform;
        currentPosition = spawningCoordinate;
        this.onSnakeDeath = onSnakeDeath;
        movementSpeed = baseMovementSpeed;
        moddedMovementSpeed = 0;
        currentTail = new List<Tail> ();
        int iteration = 0;

        while (iteration < initialTailSize) {
            AddTailPart (tailPrefab);
            iteration++;
        }

    }

    public IEnumerator Move (SnakeController controller) {

        while (true) {
            yield return new WaitUntil (() => !PlaneField.isGamePaused);
            yield return new WaitForSeconds (Mathf.Max (movementSpeed - moddedMovementSpeed, 0.05f));

            oldPosition = currentPosition;
            currentPosition += controller.currentDirection;
            Tile resultingTile = PlaneField.GetTileAtCoordinates (currentPosition);

            if (resultingTile == null) {
                onSnakeDeath?.Invoke (this);
                yield break;
            }

            switch (resultingTile.Type) {
                case Tile.TileType.Walkable:
                    transform.position = resultingTile.position;
                    UpdateTailPosition (oldPosition);
                    break;

                case Tile.TileType.Pickup:
                    transform.position = resultingTile.position;

                    if (resultingTile.assignedPallet != null) {
                        resultingTile.assignedPallet.OnPalletPickupAlt?.Invoke (this);
                        resultingTile.assignedPallet.RemovePallet ();
                    }
                    resultingTile.assignedPallet = null;

                    resultingTile.SetTileType (Tile.TileType.Walkable);
                    break;

                case Tile.TileType.Unwalkable:
                    onSnakeDeath?.Invoke (this);
                    break;

                default:
                    break;
            }
        }

    }

    public void AddTailPart (GameObject tailPrefab) {
        currentTail.Add (new Tail (tailPrefab));
        UpdateTailPosition (oldPosition);
    }

    public void UpdateTailPosition (Vector2Int coordinate) {
        for (int tailIndex = currentTail.Count - 1; tailIndex >= 0; tailIndex--) {
            if (currentTail[tailIndex].currentCoordinate == currentPosition) {
                onSnakeDeath?.Invoke (this);
                break;
            }

            if (tailIndex == 0) {

                currentTail[tailIndex].MoveTail (coordinate, currentTail);
                continue;
            }

            currentTail[tailIndex].MoveTail (currentTail[tailIndex - 1].currentCoordinate, currentTail);

        }
    }

    public void RemoveSnakeObject (SnakeController controller) {
        controller.StopCoroutine (movementCoroutine);
        foreach (var tail in currentTail) {
            if (tail.tailModel)
                Object.Destroy (tail.tailModel);
        }
        currentTail.Clear ();
        if (transform)
            Object.Destroy (transform.gameObject);
    }

    public void SetTailSize (int tailSize) {
        for (int i = 0; i < tailSize; i++) {
            Object.Destroy (currentTail[i].tailModel);
        }
        currentTail.RemoveRange (0, tailSize);
    }
}
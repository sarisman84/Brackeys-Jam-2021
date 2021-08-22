using System;

using BrackeysJam2021.Assets.Scripts.Player;

using UnityEngine;
namespace BrackeysJam2021.Assets.Manager {

    public class GameManager : MonoBehaviour {

        public SnakeController player;
        public Vector2Int playAreaSize = new Vector2Int (30, 30);
        public bool startGameOnAwake;

        [Header ("Default Pallet Settings")]
        public GameObject defaultPalletPrefab;
        public float startingDefaultPalletSpawningRate, defaultPalletSpawningRateIncrement;

        Coroutine movementCoroutine, pelletSpawnerCoroutine, playerInputCoroutine;

        public static GameManager Do { get; private set; }

        public void Awake () {

            if (Do != null) {
                Destroy (gameObject);
                return;
            }

            Do = this;

            try {
                player.gameObject.SetActive (false);
                player.transform.position = Vector3.zero;
            } catch {

                Debug.LogError ($"Player missing! (Have you forgotten to assign the player in {gameObject.name})");
            }

            RegisterPallets ();

            if (startGameOnAwake)
                StartGame ();

        }

        private void RegisterPallets () {
            PlaneField.RegisterPallet (startingDefaultPalletSpawningRate, defaultPalletSpawningRateIncrement, defaultPalletPrefab, (player) => {
                player.currentTail.Add (new SnakeController.Tail (player.tailPrefab));
                player.UpdateTailPosition (player.oldPosition);
            });
        }

        public void StartGame () {
            player.gameObject.SetActive (true);
            PlaneField.GenerateGrid (transform.position, playAreaSize);
            player.currentPosition = PlaneField.Center;
            player.currentDirection = Vector2Int.up;
            movementCoroutine = StartCoroutine (player.MovePlayer ());
            pelletSpawnerCoroutine = StartCoroutine (PlaneField.StartGeneratingPallets ());
        }

        public void EndGame () {
            player.transform.position = Vector3.zero;
            player.gameObject.SetActive (false);
            foreach (var tailPart in player.currentTail) {
                Destroy (tailPart.tailModel);
            }

            StopCoroutine (movementCoroutine);
            StopCoroutine (pelletSpawnerCoroutine);
            player.currentTail.Clear ();
            PlaneField.ResetGrid ();

        }

        private void OnDrawGizmos () {
            if (PlaneField.Grid is { } createdGrid) {

                foreach (var tile in createdGrid) {
                    Gizmos.color = (tile.type == Tile.TileType.Walkable ? Color.cyan : tile.type == Tile.TileType.Unwalkable ? Color.red : Color.yellow) - new Color (0, 0, 0, 0.5f);
                    Gizmos.DrawCube (tile.position, Vector3.one * 0.8f);
                }
            }
        }

    }
}
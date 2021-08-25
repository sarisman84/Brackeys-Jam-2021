using BrackeysJam2021.Assets.Scripts.Managers;
using BrackeysJam2021.Assets.Scripts.Managers.GridAssets;

using UnityEditor;

using UnityEngine;
using UnityEngine.Events;

namespace BrackeysJam2021.Assets.Manager {

    public class GameManager : MonoBehaviour {

        public SnakeController player;
        public Vector2Int playAreaSize = new Vector2Int (30, 30);
        public SpriteRenderer tilePrefab;
        public bool startGameOnAwake;
        [Header ("Events")]
        public UnityEvent onGameEnd;
        public UnityEvent onGameTransitionToMainMenu, onPauseEvent, onResumeEvent;

        [Header ("Default Pallet Settings")]
        public GameObject defaultPalletPrefab;
        public float startingDefaultPalletSpawningRate, defaultPalletSpawningRateIncrement;

        Coroutine movementCoroutine, pelletSpawnerCoroutine, exclusionZoneCoroutine;

        public static GameManager Do { get; private set; }

        public void Start () {

            if (Do != null) {
                Destroy (gameObject);
                return;
            }

            Do = this;
            ScoreManager.Get.SetDisplayActive = false;
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
                ScoreManager.Get.Score += 10;
                AudioManager.Play ("Pallet_Pickup");
            });
        }

        public void StartGame () {

            StartCoroutine (PlaneField.GenerateGrid (transform.position, playAreaSize, tilePrefab, () => {
                ScoreManager.Get.SetDisplayActive = true;
                ScoreManager.Get.Score = 0;
            }, () => {

                player.gameObject.SetActive (true);
                player.currentPosition = PlaneField.Center;
                player.currentDirection = Vector2Int.up;
                movementCoroutine = StartCoroutine (player.MovePlayer ());
                pelletSpawnerCoroutine = StartCoroutine (PlaneField.StartGeneratingPallets ());
                exclusionZoneCoroutine = StartCoroutine (PlaneField.StartGeneratingExclusionZones ());

            }));

        }

        public void EndGame () {
            player.transform.position = Vector3.zero;
            player.gameObject.SetActive (false);
            foreach (var tailPart in player.currentTail) {
                Destroy (tailPart.tailModel);
            }

            StopCoroutine (movementCoroutine);
            StopCoroutine (pelletSpawnerCoroutine);
            StopCoroutine (exclusionZoneCoroutine);
            player.currentTail.Clear ();
            PlaneField.ResetGrid ();

            ScoreManager.Get.SetDisplayActive = false;
            onGameEnd?.Invoke ();
        }

        public void ResetGameToMainMenu () {

            onGameTransitionToMainMenu?.Invoke ();
        }

        public void SetPauseState (bool value) {
            PlaneField.isGamePaused = value;

            if (PlaneField.isGamePaused) {
                onPauseEvent?.Invoke ();
            } else {
                onResumeEvent?.Invoke ();
            }
        }

        public void Quit () {
#if UNITY_EDITOR
            if (Application.isEditor && Application.isPlaying) {

                EditorApplication.ExitPlaymode ();
                return;
            }
#endif
            Application.Quit ();
        }

        private void OnDrawGizmos () {
            if (PlaneField.Grid is { } createdGrid) {

                foreach (var tile in createdGrid) {
                    if (tile == null) continue;
                    Gizmos.color = (tile.coordinate == SnakeController.PlayerCoordinates ? Color.green : tile.Type == Tile.TileType.Walkable ? Color.cyan : tile.Type == Tile.TileType.Unwalkable ? Color.red : Color.yellow) - new Color (0, 0, 0, 0.5f);
                    Gizmos.DrawCube (tile.position, Vector3.one * 0.8f);
                }
            }
        }

    }
}
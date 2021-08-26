using BrackeysJam2021.Assets.Scripts.Managers;
using BrackeysJam2021.Assets.Scripts.Managers.GridAssets;

using UnityEditor;

using UnityEngine;
using UnityEngine.Events;

namespace BrackeysJam2021.Assets.Manager {

    public class GameManager : MonoBehaviour {

        public SnakeController playerController;
        public GameObject playerPrefabModel;
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

            RegisterPallets ();

            if (startGameOnAwake)
                StartGame ();

        }

        private void RegisterPallets () {
            PlaneField.RegisterPallet (startingDefaultPalletSpawningRate, defaultPalletSpawningRateIncrement, defaultPalletPrefab, (snake) => {
                snake.currentTail.Add (new Tail (playerController.tailPrefab));
                snake.UpdateTailPosition (snake.PlayerOldCoordinate);
                playerController.SplitSnake (playerPrefabModel);
                ScoreManager.Get.Score += 10;
                AudioManager.Play ("Pallet_Pickup");
            });
        }

        public void StartGame () {

            StartCoroutine (PlaneField.GenerateGrid (transform.position, playAreaSize, tilePrefab, () => {
                ScoreManager.Get.SetDisplayActive = true;
                ScoreManager.Get.Score = 0;
            }, () => {

                playerController.CreateSnake (playerPrefabModel, PlaneField.Center, Vector2Int.up);

                movementCoroutine = StartCoroutine (playerController.MovePlayer ());
                pelletSpawnerCoroutine = StartCoroutine (PlaneField.StartGeneratingPallets ());
                exclusionZoneCoroutine = StartCoroutine (PlaneField.StartGeneratingExclusionZones ());

            }));

        }

        public void EndGame () {
            playerController.ResetSnakes ();
            StopCoroutine (movementCoroutine);
            StopCoroutine (pelletSpawnerCoroutine);
            StopCoroutine (exclusionZoneCoroutine);

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
                    Gizmos.color = (tile.Type == Tile.TileType.Walkable ? Color.cyan : tile.Type == Tile.TileType.Unwalkable ? Color.red : Color.yellow) - new Color (0, 0, 0, 0.5f);
                    foreach (var snake in SnakeController.SnakeEntities) {
                        Gizmos.color = (tile.coordinate == snake.PlayerCoordinate) ? Color.green - new Color (0, 0, 0, 0.5f) : Gizmos.color;
                    }
                    Gizmos.DrawCube (tile.position, Vector3.one * 0.8f);
                }
            }
        }

    }
}
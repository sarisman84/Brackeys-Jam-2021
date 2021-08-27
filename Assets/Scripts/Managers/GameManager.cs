using System;
using System.Collections.Generic;

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
        public UnityEvent onGameStart;
        public UnityEvent onGameEnd;
        public UnityEvent onGameTransitionToMainMenu, onPauseEvent, onResumeEvent;

        [Header ("Pallet Settings")]
        public List<PalletInfo> palletPresets;

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
            foreach (var preset in palletPresets) {
                PlaneField.RegisterPallet (preset.initialSpawnRate, preset.spawnRateIncrement, preset.palletPrefab, (snake) => {

                    switch (preset.palletType) {
                        case PalletInfo.PalletType.Score:
                            AddScore (snake);
                            break;
                        case PalletInfo.PalletType.Speedboost:
                            AddScore (snake);
                            snake.moddedMovementSpeed += 0.005f;
                            break;
                    }

                    AudioManager.Play ("Pallet_Pickup");
                });
            }

        }

        private void AddScore (Snake snake) {
            int score = Mathf.RoundToInt (10 + (SnakeController.SnakeEntities.Count - 1) * 15f);
            print ($"Current Score earned: {score}");
            ScoreManager.Get.Score += score;
            snake.currentTail.Add (new Tail (playerController.tailPrefab));
            snake.UpdateTailPosition (snake.PlayerOldCoordinate);
            List<Snake> snakes = playerController.SplitSnake (playerPrefabModel, 10);

        }

        public void StartGame () {

            StartCoroutine (PlaneField.GenerateGrid (transform.position, playAreaSize, tilePrefab, () => {
                ScoreManager.Get.SetDisplayActive = true;
                ScoreManager.Get.Score = 0;
            }, () => {

                Snake player = playerController.CreateSnake (playerPrefabModel, PlaneField.Center, Vector2Int.up, 3, () => {
                    AudioManager.Play ("Spawn_Player");
                });

                player.movementCoroutine = StartCoroutine (player.Move (playerController));
                pelletSpawnerCoroutine = StartCoroutine (PlaneField.StartGeneratingPallets ());
                exclusionZoneCoroutine = StartCoroutine (PlaneField.StartGeneratingExclusionZones ());

            }));

            onGameStart?.Invoke ();

        }

        public void EndGame () {

            playerController.ResetSnakes ();

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
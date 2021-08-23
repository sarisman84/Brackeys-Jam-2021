using System;

using UnityEngine;
using Object = UnityEngine.Object;

namespace BrackeysJam2021.Assets.Scripts.Managers.GridAssets {
    public class Pallet {

        public Pallet (float baseSpawnRate, float spawnRateIncrement, GameObject palletPrefab, Action<SnakeController> onPalletPickup) {
            BaseSpawnRate = baseSpawnRate;
            SpawnRateIncrement = spawnRateIncrement;
            PalletPrefab = palletPrefab;
            OnPalletPickup = onPalletPickup;
        }

        public float BaseSpawnRate { get; }
        public float SpawnRateIncrement { get; set; }
        public GameObject PalletPrefab { get; }
        public Action<SnakeController> OnPalletPickup { get; }

        public float CurrentSpawnRate { get; internal set; }
        public float ModifiedSpawnRate { get; internal set; }
    }

    public class PalletObject {
        public Action<SnakeController> OnPalletPickup { get; }

        GameObject palletModel;

        public PalletObject (GameObject prefab, Vector3 position, Action<SnakeController> @event) {
            OnPalletPickup = @event;
            palletModel = Object.Instantiate (prefab);
            palletModel.transform.position = position;
            palletModel.transform.SetParent (GameObject.FindGameObjectWithTag ("Grid/Pallets").transform);
        }

        public void RemovePallet () {
            if (palletModel)
                Object.Destroy (palletModel);
        }

        public void TriggerPallet (SnakeController player) {
            OnPalletPickup?.Invoke (player);
        }
    }
}
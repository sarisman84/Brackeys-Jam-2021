using System;

using UnityEngine;
using Object = UnityEngine.Object;

namespace BrackeysJam2021.Assets.Scripts.Managers.GridAssets {
    public class Pallet {

        public Pallet (float baseSpawnRate, float spawnRateIncrement, GameObject palletPrefab, Action<Snake> onPalletPickup) {
            BaseSpawnRate = baseSpawnRate;
            SpawnRateIncrement = spawnRateIncrement;
            PalletPrefab = palletPrefab;
            OnPalletPickup = onPalletPickup;
        }

        public float BaseSpawnRate { get; }
        public float SpawnRateIncrement { get; set; }
        public GameObject PalletPrefab { get; }

        public Action<Snake> OnPalletPickup { get; }

        public float CurrentSpawnRate { get; internal set; }
        public float ModifiedSpawnRate { get; internal set; }
    }

    public class PalletObject {

        public Action<Snake> OnPalletPickupAlt { get; }

        GameObject palletModel;

        public PalletObject (GameObject prefab, Vector3 position, Action<Snake> @event) {
            OnPalletPickupAlt = @event;
            palletModel = Object.Instantiate (prefab);
            palletModel.transform.position = position;
            palletModel.transform.SetParent (GameObject.FindGameObjectWithTag ("Grid/Pallets").transform);
        }

        public void RemovePallet () {
            if (palletModel)
                Object.Destroy (palletModel);
        }

        public void TriggerPallet (Snake player) {
            OnPalletPickupAlt?.Invoke (player);
        }
    }
}
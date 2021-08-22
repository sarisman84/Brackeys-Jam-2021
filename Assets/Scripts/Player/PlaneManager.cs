using System;
using System.Threading;

using UnityEngine;
using Random = UnityEngine.Random;
namespace BrackeysJam2021.Assets.Scripts.Player {

    public class PlaneManager : MonoBehaviour {

        public static PlaneManager Get { get; private set; }
        public GameObject defaultTailIncreasePalletModel, accelerationPalletModel;

        public float palletMinSpawnRate, palletMaxSpawnRate;

        float curSpawnRate = 0;
        float curTime = 0;

        public bool SpawnPallet { private get; set; }

        void Awake () {

            if (Get != null) {
                Destroy (gameObject);
                return;
            }

            Get = this;

            curSpawnRate = palletMaxSpawnRate;
        }

        private void Update () {

            if (SpawnPallet) {
                curTime += Time.deltaTime;

                if (curTime >= curSpawnRate) {

                    curSpawnRate = Random.Range (palletMinSpawnRate, palletMaxSpawnRate);
                    curTime = 0;

                    SpawnARandomDefaultPallet ();

                }
            }

        }

        private void SpawnARandomDefaultPallet () {
            Tile randomTile = MovementPlane.GetTileAtRandomCoordinates ();

            randomTile.indicator = Instantiate (defaultTailIncreasePalletModel);
            randomTile.indicator.transform.position = randomTile.position;

            randomTile.type = Tile.TileType.Default;
        }

        public void ClearPallets () {
            foreach (var tile in MovementPlane.Grid) {
                Destroy (tile.indicator);
                tile.type = Tile.TileType.Walkable;
            }
        }
    }

}
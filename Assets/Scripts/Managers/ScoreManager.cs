namespace BrackeysJam2021.Assets.Scripts.Managers {
    using TMPro;

    using UnityEngine;
    public class ScoreManager : MonoBehaviour {
        public TMP_Text scoreDisplayer;
        public int Score { set; get; }

        public bool SetDisplayActive {
            set => scoreDisplayer.gameObject.SetActive (value);
        }

        public static ScoreManager Get { get; private set; }

        void Awake () {
            if (Get) {
                Destroy (gameObject);
                return;
            }

            Get = this;
        }

        private void Update () {
            if (scoreDisplayer)
                scoreDisplayer.text = Score.ToString ();
        }

    }
}
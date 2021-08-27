namespace BrackeysJam2021.Assets.Scripts.Managers {
    using System.Collections.Generic;

    using UnityEngine;

    public class InstructionsManager : MonoBehaviour {
        public List<DOTweener> labelsToTween;

        void Awake () {
            SetLabelsActive (false);
        }

        public void SetLabelsActive (bool value) {
            foreach (var label in labelsToTween) {
                label.gameObject.SetActive (value);
            }
        }
        public void ShowInstructions () {
            foreach (var label in labelsToTween) {
                label.SetAlpha (0);
                label.gameObject.SetActive (true);
                label.Fade (1, 0.2f, true);
            }
        }
    }
}
using System.Collections.Generic;

namespace BrackeysJam2021.Assets.Scripts.Managers {
    using System.Collections;
    using System;

    using TMPro;

    using UnityEngine.Networking;
    using UnityEngine.UI;
    using UnityEngine;

    public class ScoreManager : MonoBehaviour {

        const string privateCode = "as4Ycfj99UynN40vlv3B-AxRIv4A3f0EOmJ_loEuXKPw",
            publicCode = "61266bc58f40bb6e98b16d61",
            webURL = "http://dreamlo.com/lb/";

        public TMP_Text scoreDisplayer, gameOverScoreDisplayer;

        [Header ("Highscore Settings")]
        public TMP_InputField highscoreInputField;
        public Highscore[] highscoresList;
        public TMP_Text[] highscoreDisplayList;

        public int Score { set; get; }

        public bool SetDisplayActive {
            set => scoreDisplayer.gameObject.SetActive (value);
        }

        public static ScoreManager Get { get; private set; }

        public bool CanScoreBeSaved { get; set; } = false;

        void Awake () {
            if (Get) {
                Destroy (gameObject);
                return;
            }

            Get = this;

        }

        private void Update () {
            if (scoreDisplayer && scoreDisplayer.gameObject.activeSelf)
                scoreDisplayer.text = Score.ToString ();

        }

        public void UploadHighscore (string value) {
            print ($"Save Highscore. (Save Mode: {CanScoreBeSaved})");
            if (Score > 0 && CanScoreBeSaved) {
                AddNewHighscore (value, Score, () => DownloadHighscores (UpdateHighscoreList));
                highscoreInputField.text = "Score Uploaded.";
                CanScoreBeSaved = false;
            } else {
                highscoreInputField.text = Score == 0 ? "Score is 0." : "Cant save Score.";
            }

        }

        public void UpdateHighscoreList () {
            print ("Updating List in Game.");
            if (highscoreDisplayList != null)
                for (int i = 0; i < highscoreDisplayList.Length; i++) {

                    TMP_Text textDisplay = highscoreDisplayList[i];
                    if (highscoresList == null || highscoresList.Length == 0) {
                        textDisplay.text = $"[{i+1}]___: 000";
                        continue;
                    } else if (i >= highscoresList.Length) continue;

                    textDisplay.text = $"[{i+1}]{highscoresList[i].userName}: {highscoresList[i].score}";
                }

        }
        public void AddNewHighscore (string username, int score, Action onHighscoreUpload = null) {
            StartCoroutine (UploadNewHighscore (username, score, onHighscoreUpload));
        }

        public void DownloadHighscores (Action onDownloadEnd = null) {
            StartCoroutine (DownloadHighscoresFromDatabase (onDownloadEnd));
        }

        public void FetchHighscoresAndDisplayThem () {
            UpdateHighscoreList ();
            StartCoroutine (DownloadHighscoresFromDatabase (UpdateHighscoreList));
        }

        public void DisplayScoreOnGameOver () {
            if (gameOverScoreDisplayer)
                gameOverScoreDisplayer.text = $"Personal Score:{System.Environment.NewLine}{Score}";
        }

        IEnumerator UploadNewHighscore (string username, int score, Action onHighscoreUploaded) {
            UnityWebRequest www = new UnityWebRequest (webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL (username.Replace (' ', '_')) + "/" + score);
            yield return www.SendWebRequest ();;

            if (string.IsNullOrEmpty (www.error)) {
                print ("Upload Successfull");
                onHighscoreUploaded?.Invoke ();
            } else {
                print ("Upload failed: " + www.error);
            }

        }

        IEnumerator DownloadHighscoresFromDatabase (Action onDownloadEnd) {
            print ("Downloading Highscores!");
            UnityWebRequest www = new UnityWebRequest (webURL + publicCode + "/pipe/0/10");
            www.downloadHandler = new DownloadHandlerBuffer ();
            yield return www.SendWebRequest ();

            if (string.IsNullOrEmpty (www.error)) {
                FormatHighscores (www.downloadHandler.text);
                print ("Download Successfull");
                onDownloadEnd?.Invoke ();
            } else {
                print ("Download failed: " + www.error);
            }

        }

        void FormatHighscores (string textStream) {
            string[] entries = textStream.Split (new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            highscoresList = new Highscore[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                string[] entryInfo = entries[i].Split (new char[] { '|' });
                string userName = entryInfo[0];
                int score = int.Parse (entryInfo[1]);
                highscoresList[i] = new Highscore (userName, score);
            }
        }

    }

    public struct Highscore {
        public string userName;
        public int score;

        public Highscore (string userName, int score) {
            this.userName = userName;
            this.score = score;
        }

    }

}
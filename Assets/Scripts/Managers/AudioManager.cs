using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AudioManager : MonoBehaviour {

    private static AudioManager Get { get; set; }
    public List<CustomClip> customClips = new List<CustomClip> ();

    private void Awake () {
        if (Get) {
            Destroy (gameObject);
            return;
        }
        Get = this;

        SetupClips ();

    }

    private void SetupClips () {
        GameObject playerHolder = new GameObject ("Audio Players");
        for (int i = 0; i < customClips.Count; i++) {
            CustomClip clip = customClips[i];
            clip.player = playerHolder.AddComponent<AudioSource> ();
            clip.player.playOnAwake = clip.playOnAwake;
            clip.player.clip = clip.audioClip;
            clip.player.loop = clip.loop;
            clip.player.volume = clip.volume;
            clip.player.pitch = clip.pitch;

            if (clip.playOnAwake)
                clip.player.Play ();
        }
    }

    public static void Play (string clip, bool waitForCompletion) {
        if (waitForCompletion) {
            if (AudioManager.Get.customClips.Find (c => c.clipID == clip) is { } clipFound && !clipFound.player.isPlaying) {

                clipFound.player.Play ();
            }
        } else {
            if (AudioManager.Get.customClips.Find (c => c.clipID == clip) is { } clipFound) {

                clipFound.player.Play ();
            }
        }

    }

    public static void Play (string clip) {
        Play (clip, false);
    }

    public static void Stop (string clip) {
        if (AudioManager.Get.customClips.Find (c => c.clipID == clip) is { } clipFound) {
            clipFound.player.Stop ();
        }
    }

    [Serializable]
    public class CustomClip {
        public string clipID;
        public AudioClip audioClip;

        internal AudioSource player;

        [Range (0, 1)]
        public float volume = 1;
        [Range (-3, 3)]
        public float pitch = 1;
        public bool loop;
        public bool playOnAwake;
    }
}
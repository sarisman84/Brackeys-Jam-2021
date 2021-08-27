using System;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DOTweener : MonoBehaviour {

    List<TMP_Text> textElements = new List<TMP_Text> ();
    List<Image> imageElements = new List<Image> ();
    bool completedAlready = false;

    public UnityEvent onTweenCompletion;
    public float tweenDuration = 0;
    public bool fetchChildren;

    void Awake () {

        if (GetComponent<TextMeshProUGUI> () is { } text) {
            textElements.Add (text);
        }

        if (GetComponent<Image> () is { } image) {
            imageElements.Add (image);
        }

        if (fetchChildren)
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild (i).GetComponent<TextMeshProUGUI> () is { } newText) {
                    textElements.Add (newText);
                }

                if (transform.GetChild (i).GetComponent<Image> () is { } newImage) {
                    imageElements.Add (newImage);
                }
            }
    }
    public void Fade (float value) {

        Fade (value, 0.2f, true);

    }

    public void Fade (float value, float duration, bool useCoroutine = false) {

        YieldInstruction instruction = null;
        foreach (var text in textElements) {
            if (text.gameObject.activeSelf)
                instruction = text.DOFade (value, duration).WaitForCompletion ();
        }

        foreach (var image in imageElements) {
            if (image.gameObject.activeSelf)
                instruction = image.DOFade (value, duration).WaitForCompletion ();
        }

        if (!completedAlready && useCoroutine)
            StartCoroutine (OnTweenCompletion (instruction));

    }

    public void SetAlpha (float value) {
        Fade (1, 0);
    }

    private IEnumerator OnTweenCompletion (YieldInstruction instruction) {
        yield return instruction;
        yield return new WaitForSeconds (tweenDuration);
        onTweenCompletion?.Invoke ();
        completedAlready = true;
    }
}
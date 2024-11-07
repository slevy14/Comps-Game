// CODE FROM THIS TUTORIAL: https://www.youtube.com/watch?v=UR_Rh0c4gbY
// Christina Creates Games on youtube!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour {

    private TMP_Text textBox;

    // Basic Typewriter functionality
    private int currentVisibleCharacterIndex;
    private Coroutine typewriterCoroutine;
    private bool readyForNewText = true;

    private WaitForSeconds _simpleDelay;
    private WaitForSeconds _punctuationDelay;

    [Header("Typewriter Settings")]
    [SerializeField] private float charactersPerSecond = 20;
    [SerializeField] private float punctuationDelay = 0.5f;

    // Skipping Functionality
    public bool CurrentlySkipping {get; private set;}
    private WaitForSeconds _skipDelay;

    [Header("Skip Options")]
    [SerializeField] private bool quickSkip;
    [SerializeField][Min(1)] private int skipSpeedup = 5;

    // Event Functionality
    private WaitForSeconds _textboxFullEventDelay;
    [SerializeField] [Range(0.1f, 0.5f)] private float sendDoneDelay = 0.25f;
    public static event Action CompleteTextRevealed;
    public static event Action<char> CharacterRevealed;


    private void Awake() {
        textBox = GetComponent<TMP_Text>();

        _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
        _punctuationDelay = new WaitForSeconds(punctuationDelay);
        _skipDelay = new WaitForSeconds(1 / (charactersPerSecond * skipSpeedup));
        _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);
    }

    private void OnEnable() {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
    }

    private void OnDisable() {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (textBox.maxVisibleCharacters <= textBox.textInfo.characterCount - 1) {
                Debug.Log("called skip");
                Skip();
            } else { 
                if (TutorialController.Instance.inTutorial && !PauseMenuController.Instance.isPaused && TutorialController.Instance.CanAdvanceDialog() && readyForNewText) {
                    textBox.maxVisibleCharacters = 0;
                    TutorialController.Instance.NextStep();
                    // Debug.Log("advanced");
                }
            }
        }
    }

    public void PrepareForNewText(UnityEngine.Object obj) {

        if (!readyForNewText) {
            return;
        }

        CurrentlySkipping = false;
        readyForNewText = false;

        if (typewriterCoroutine != null) {
            // Debug.Log("stopping coroutine from prepare");
            StopCoroutine(typewriterCoroutine);
        }

        textBox.maxVisibleCharacters = 0;
        currentVisibleCharacterIndex = 0;

        // Debug.Log("starting coroutine");
        typewriterCoroutine = StartCoroutine(Typewriter());
    }

    private IEnumerator Typewriter() {
        TMP_TextInfo textInfo = textBox.textInfo;

        while (currentVisibleCharacterIndex < textInfo.characterCount + 1) {

            var lastCharacterIndex = textInfo.characterCount - 1;

            if (currentVisibleCharacterIndex == lastCharacterIndex) {
                textBox.maxVisibleCharacters++;
                yield return _textboxFullEventDelay;
                CompleteTextRevealed?.Invoke();
                readyForNewText = true;
                yield break;
            }


            char character = textInfo.characterInfo[currentVisibleCharacterIndex].character;

            textBox.maxVisibleCharacters++;

            if ((!CurrentlySkipping) && (character == '?' || character == '.' || character == ',' || character == ':' || character == ';' || character == '!' || character == '-') ) {
                yield return _punctuationDelay;
            } else {
                yield return CurrentlySkipping ? _skipDelay : _simpleDelay;
            }

            // play sound few
            if (currentVisibleCharacterIndex % 3 == 0 && !CurrentlySkipping) {
                AudioController.Instance.PlaySoundEffect("BearTalkSound");
            }

            CharacterRevealed?.Invoke(character);
            currentVisibleCharacterIndex++;
            // Debug.Log("progressing coroutine");
        }
    }

    void Skip() {
        if (CurrentlySkipping) {
            // Debug.Log("currently skipping, not doing another");
            return;
        }

        CurrentlySkipping = true;

        if (!quickSkip) {
            StartCoroutine(SkipSpeedupReset());
            AudioController.Instance.PlaySoundEffect("BearTalkSound");
            return;
        }

        //else, quickskip
        // Debug.Log("stopping coroutine from skip");
        StopCoroutine(typewriterCoroutine);
        textBox.maxVisibleCharacters = textBox.textInfo.characterCount;
        readyForNewText = true;
        CompleteTextRevealed?.Invoke();
    }

    private IEnumerator SkipSpeedupReset() {
        yield return new WaitUntil(() => textBox.maxVisibleCharacters == textBox.textInfo.characterCount - 1);
        CurrentlySkipping = false;
    }
}

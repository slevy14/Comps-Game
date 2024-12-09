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
    [SerializeField] private float punctuationDelay = 0.3f;

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

        // define delays
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
        // on click, check to skip or advance
        if (Input.GetMouseButtonDown(0)) {
            // if still chars to show, skip
            if (textBox.maxVisibleCharacters <= textBox.textInfo.characterCount - 1) {
                Debug.Log("called skip");
                Skip();
            } else { // else, advance to next step of tutorial
                if (TutorialController.Instance.inTutorial && !PauseMenuController.Instance.isPaused && TutorialController.Instance.CanAdvanceDialog() && readyForNewText) {
                    textBox.maxVisibleCharacters = 0;
                    TutorialController.Instance.NextStep();
                }
            }
        }
    }

    public void PrepareForNewText(UnityEngine.Object obj) {
        // don't reset if shouldn't
        if (!readyForNewText) {
            return;
        }

        // reset status
        CurrentlySkipping = false;
        readyForNewText = false;

        // prevent coroutine conflicts
        if (typewriterCoroutine != null) {
            StopCoroutine(typewriterCoroutine);
        }

        // continue resetting status
        textBox.maxVisibleCharacters = 0;
        currentVisibleCharacterIndex = 0;

        // restart typewriter effect
        typewriterCoroutine = StartCoroutine(Typewriter());
    }

    private IEnumerator Typewriter() {
        TMP_TextInfo textInfo = textBox.textInfo;

        // loop while still more chars:
        while (currentVisibleCharacterIndex < textInfo.characterCount + 1) {

            // find most current char index
            var lastCharacterIndex = textInfo.characterCount - 1;

            // quit if whole text revealed
            if (currentVisibleCharacterIndex == lastCharacterIndex) {
                textBox.maxVisibleCharacters++;
                yield return _textboxFullEventDelay;
                CompleteTextRevealed?.Invoke();
                readyForNewText = true;
                yield break;
            }


            char character = textInfo.characterInfo[currentVisibleCharacterIndex].character;

            // display character
            textBox.maxVisibleCharacters++;

            // wait for delay time, either skip, punctuation, or regular
            if ((!CurrentlySkipping) && (character == '?' || character == '.' || character == ',' || character == ':' || character == ';' || character == '!' || character == '-') ) {
                yield return _punctuationDelay;
            } else {
                yield return CurrentlySkipping ? _skipDelay : _simpleDelay;
            }

            // play sound effect
            if (currentVisibleCharacterIndex % 3 == 0 && !CurrentlySkipping) {
                AudioController.Instance.PlaySoundEffect("BearTalkSound");
            }

            CharacterRevealed?.Invoke(character);
            currentVisibleCharacterIndex++;
        }
    }

    void Skip() {
        // don't skip if already skipping
        if (CurrentlySkipping) {
            return;
        }

        CurrentlySkipping = true;

        // regular skip, speed up text
        if (!quickSkip) {
            StartCoroutine(SkipSpeedupReset());
            AudioController.Instance.PlaySoundEffect("BearTalkSound");
            return;
        }

        // else, quickskip
        // just jump to end
        StopCoroutine(typewriterCoroutine);
        textBox.maxVisibleCharacters = textBox.textInfo.characterCount;
        readyForNewText = true;
        CompleteTextRevealed?.Invoke();
    }

    private IEnumerator SkipSpeedupReset() {
        // don't let skip happen until this is finished
        yield return new WaitUntil(() => textBox.maxVisibleCharacters == textBox.textInfo.characterCount - 1);
        CurrentlySkipping = false;
    }
}

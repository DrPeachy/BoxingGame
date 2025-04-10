// QuestionGenerator.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening; // DOTween namespace

public class QuestionGenerator : MonoBehaviour
{
    public static QuestionGenerator Instance { get; private set; }

    public GameObject questionBoard;
    public TMP_Text questionText;
    public List<TMP_Text> buttonTexts; // Text components attached to the answer buttons
    public List<Button> buttons;         // Answer button components (index 0: left, 1: up, 2: right, 3: down)
    public float typeDuration = 2f;

    // Define the trivia question structure
    [Serializable]
    public class TriviaQuestion
    {
        public string question;       // Question text
        public string[] options;      // Array of four options (order: left, up, right, down)
        public int correctOption;     // Correct option index (0-3)
    }

    // Private question bank, always populated in code
    private List<TriviaQuestion> questionBank;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // Populate the question bank in code
        PopulateDefaultQuestions();

        // Optionally initialize buttonTexts list if not set manually
        if (buttonTexts == null || buttonTexts.Count == 0)
        {
            buttonTexts = new List<TMP_Text>();
            foreach (Button button in buttons)
            {
                TMP_Text txt = button.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                    buttonTexts.Add(txt);
            }
        }
    }

    // Populate a default trivia question bank entirely in code
    private void PopulateDefaultQuestions()
    {
        questionBank = new List<TriviaQuestion>()
        {
            new TriviaQuestion() {
                question = "What is the capital of France?",
                options = new string[] { "Paris", "London", "Berlin", "Madrid" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "Which planet is known as the Red Planet?",
                options = new string[] { "Earth", "Mars", "Jupiter", "Saturn" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "Who wrote 'Romeo and Juliet'?",
                options = new string[] { "Charles Dickens", "William Shakespeare", "Leo Tolstoy", "Mark Twain" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "What is the chemical symbol for water?",
                options = new string[] { "H2O", "CO2", "NaCl", "O2" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "What is the largest mammal in the world?",
                options = new string[] { "Elephant", "Blue Whale", "Giraffe", "Great White Shark" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "What is the speed of light?",
                options = new string[] { "300,000 km/s", "150,000 km/s", "1,000,000 km/s", "3,000 km/s" },
                correctOption = 0
            },
        };
    }

    // Generate a trivia question with DOTween typewriter effect for the question text.
    // Returns the Button which corresponds to the correct answer.
    public Button GenerateQuestion()
    {
        // Ensure the question board is active
        if (questionBoard == null || !questionBoard.activeSelf)
        {
            if (questionBoard != null)
                questionBoard.SetActive(true);
        }

        // Randomly select a trivia question from the bank
        if (questionBank == null || questionBank.Count == 0)
        {
            Debug.LogError("Question bank is empty!");
            return null;
        }
        int randomIndex = UnityEngine.Random.Range(0, questionBank.Count);
        TriviaQuestion selectedQuestion = questionBank[randomIndex];

        Debug.Log("Generating trivia question...");

        // Clear current question text and animate it with DOTween (typewriter effect)
        questionText.text = "";
        // Duration for the typewriter effect (in seconds)
        questionText.DOText(selectedQuestion.question, typeDuration).SetEase(Ease.Linear);

        // Set texts for all four answer buttons from the question options
        if (buttonTexts.Count < 4)
        {
            Debug.LogError("Not enough button texts assigned.");
            return null;
        }
        for (int i = 0; i < 4; i++)
        {
            if (selectedQuestion.options.Length > i)
                buttonTexts[i].text = selectedQuestion.options[i];
            else
                buttonTexts[i].text = "";
        }

        // Return the Button corresponding to the correct answer for later validation
        Button correctButton = buttons[selectedQuestion.correctOption];
        Debug.Log($"Correct answer is at index: {selectedQuestion.correctOption}");
        return correctButton;
    }
}

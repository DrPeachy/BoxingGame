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
                options = new string[] { "Jupiter", "Mars", "Earth", "Saturn" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "What is the largest mammal in the world?",
                options = new string[] { "Elephant", "Giraffe", "Blue Whale", "Great White Shark" },
                correctOption = 2
            },
            new TriviaQuestion() {
                question = "What is the chemical symbol for water?",
                options = new string[] { "CO2", "NaCl", "O2", "H2O" },
                correctOption = 3
            },
            new TriviaQuestion() {
                question = "Which language is primarily used for Android app development?",
                options = new string[] { "Java", "Swift", "C#", "Python" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "Which gas do plants absorb from the atmosphere?",
                options = new string[] { "Oxygen", "Carbon Dioxide", "Nitrogen", "Hydrogen" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "What is the smallest prime number?",
                options = new string[] { "3", "7", "2", "11" },
                correctOption = 2
            },
            new TriviaQuestion() {
                question = "Who painted the Mona Lisa?",
                options = new string[] { "Pablo Picasso", "Vincent van Gogh", "Claude Monet", "Leonardo da Vinci" },
                correctOption = 3
            },
            new TriviaQuestion() {
                question = "How many continents are there on Earth?",
                options = new string[] { "7", "5", "6", "4" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "Which element has the chemical symbol 'Fe'?",
                options = new string[] { "Gold", "Iron", "Silver", "Copper" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "Which ocean is the largest?",
                options = new string[] { "Atlantic", "Indian", "Arctic", "Pacific" },
                correctOption = 3
            },
            new TriviaQuestion() {
                question = "Who wrote '1984'?",
                options = new string[] { "George Orwell", "Aldous Huxley", "Mark Twain", "J.K. Rowling" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "What is the hottest planet in our solar system?",
                options = new string[] { "Mercury", "Venus", "Mars", "Jupiter" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "In computing, what does 'CPU' stand for?",
                options = new string[] { "Central Processing Unit", "Computer Personal Unit", "Central Performance Utility", "Control Processing Unit" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "Which country gifted the Statue of Liberty to the USA?",
                options = new string[] { "France", "England", "Germany", "Spain" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "What year did World War II end?",
                options = new string[] { "1945", "1939", "1918", "1963" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "Which artist is famous for the sculpture 'The Thinker'?",
                options = new string[] { "Auguste Rodin", "Michelangelo", "Donatello", "Leonardo da Vinci" },
                correctOption = 0
            },
            new TriviaQuestion() {
                question = "What is the hardest natural substance?",
                options = new string[] { "Gold", "Diamond", "Iron", "Quartz" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "Which device is used to measure atmospheric pressure?",
                options = new string[] { "Thermometer", "Barometer", "Hygrometer", "Anemometer" },
                correctOption = 1
            },
            new TriviaQuestion() {
                question = "Which country is known as the Land of the Rising Sun?",
                options = new string[] { "China", "Japan", "South Korea", "Thailand" },
                correctOption = 1
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

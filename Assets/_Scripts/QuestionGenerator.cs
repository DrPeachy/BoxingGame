using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public class QuestionGenerator : MonoBehaviour
{
    public static QuestionGenerator Instance { get; private set; }

    public GameObject questionBoard;
    public TMPro.TMP_Text questionText;
    public List<TMP_Text> buttonTexts;
    public List<Button> buttons;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        buttonTexts = new List<TMP_Text>();
        foreach(Button button in buttons){
            buttonTexts.Add(button.GetComponentInChildren<TMP_Text>());
        }
    }

    public Button GenerateQuestion(){
        if(questionBoard == null || !questionBoard.activeSelf) return null;

        // generate question
        int a = UnityEngine.Random.Range(1, 100);
        int b = UnityEngine.Random.Range(1, 100);

        // generate operation
        int operation = UnityEngine.Random.Range(0, 3); // 0, 1, 2 -> add, sub, mul

        int answer = 0;
        
        switch(operation){
            case 0:
                answer = a + b;
                break;
            case 1:
                answer = a - b;
                if(answer < 0){
                    answer = -answer;
                    int temp = a;
                    a = b;
                    b = temp;
                }
                break;
            case 2:
                answer = a * b;
                break;
            default:
                break;
        }
        

        // generate two wrong answers, one with same last digit as correct one, or both with same last digit
        int wrongAnswer1 = answer;
        int wrongAnswer2 = answer;
        int max = Math.Clamp(answer * 2, 3, 9801); // 99 * 99 = 9801
        while(wrongAnswer1 == answer){
            max = Math.Clamp(answer * 2, 3, 9801); // 99 * 99 = 9801
            wrongAnswer1 = UnityEngine.Random.Range(1, max);
        }

        while(wrongAnswer2 == answer || wrongAnswer2 == wrongAnswer1){
            max = Math.Clamp(answer * 2, 3, 9801); // 99 * 99 = 9801
            wrongAnswer2 = UnityEngine.Random.Range(1, max);
            // set wrong answer 2 to same last digit as correct answer or wrong answer 1
            if(wrongAnswer2 % 10 != answer % 10 && wrongAnswer2 % 10 != wrongAnswer1 % 10){
                int temp = UnityEngine.Random.Range(0, 2) == 0 ? answer : wrongAnswer1;
                wrongAnswer2 = wrongAnswer2 - wrongAnswer2 % 10 + temp % 10;
            }
        }

        questionText.text = $"{a} {(operation == 0 ? "+" : operation == 1 ? "-" : "*")} {b} = ?";
        int correctAnswerIndex = UnityEngine.Random.Range(0, 3); // 0, 1, 2
        buttonTexts[correctAnswerIndex].text = answer.ToString();

        int wrongAnswerIndex1 = -1;
        int wrongAnswerIndex2 = -1;
        for(int i = 0; i < 3; i++){
            if(i == correctAnswerIndex) continue;
            if(wrongAnswerIndex1 == -1){
                wrongAnswerIndex1 = i;
            }else{
                wrongAnswerIndex2 = i;
            }
        }
        // switch wrong answers to make it random
        if(UnityEngine.Random.Range(0, 2) == 0){
            int temp = wrongAnswer1;
            wrongAnswer1 = wrongAnswer2;
            wrongAnswer2 = temp;
        }

        buttonTexts[wrongAnswerIndex1].text = wrongAnswer1.ToString();
        buttonTexts[wrongAnswerIndex2].text = wrongAnswer2.ToString();
        
        Debug.Log($"Correct answer: {correctAnswerIndex + 1}");
        Debug.Log($"Wrong Answer 1: {wrongAnswerIndex1 + 1}");
        Debug.Log($"Wrong Answer 2: {wrongAnswerIndex2 + 1}");

        return buttons[correctAnswerIndex];
    }
}

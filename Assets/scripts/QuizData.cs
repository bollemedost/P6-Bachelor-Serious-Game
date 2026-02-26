using UnityEngine;

[CreateAssetMenu(fileName = "QuizData", menuName = "Word Quiz/Quiz Data")]
public class QuizData : ScriptableObject
{
    
[System.Serializable]
public class Quiz
{
    public string text;        // clue
    public string correctWord; // answer

    public int startX;         // column (0 = left)
    public int startY;         // row (0 = top)
    public bool isAcross;      // true = across, false = down
}

    public Quiz[] quizzes;
}
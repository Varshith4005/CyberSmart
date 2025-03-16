using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] options;
        public string correctAnswer;
    }

    public Question[] questions;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private float timeRemaining = 20f;
    private bool isAnswered = false;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI questionNumberText;
    public TextMeshProUGUI timerText;
    public Button[] answerButtons;
    public Image timerCircle;
    public GameObject resultsPopup;
    public TextMeshProUGUI honorText;
    public TextMeshProUGUI totalHonorText;
    public TextMeshProUGUI bonusText;
    public Image[] stars;

    private Color defaultTimerColor = Color.green;
    private Color warningColor = Color.red;
    private RectTransform timerTransform;
    private bool isShaking = false;

    void Start()
    {
        resultsPopup.SetActive(false);
        timerTransform = timerCircle.GetComponent<RectTransform>();
        timerText.color = defaultTimerColor;
        timerCircle.color = defaultTimerColor;

        ShuffleAndSelectQuestions();
        LoadQuestion();
        StartCoroutine(TimerCountdown());
    }

    void ShuffleAndSelectQuestions()
    {
        List<Question> questionPool = new List<Question>(questions);
        questions = new Question[Mathf.Min(15, questionPool.Count)]; // Select up to 15 questions

        for (int i = 0; i < questions.Length; i++)
        {
            int randomIndex = Random.Range(0, questionPool.Count);
            questions[i] = questionPool[randomIndex];
            questionPool.RemoveAt(randomIndex); // Remove selected question to avoid duplicates
        }
    }

    void LoadQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            EndQuiz();
            return;
        }

        isAnswered = false;
        timeRemaining = 20f;
        timerText.color = defaultTimerColor;
        timerCircle.color = defaultTimerColor;

        Question q = questions[currentQuestionIndex];

        List<string> shuffledOptions = new List<string>(q.options);
        shuffledOptions.Shuffle();

        questionText.text = q.questionText;
        questionNumberText.text = "Question " + (currentQuestionIndex + 1) + "/" + questions.Length;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            string answerText = shuffledOptions[i];
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answerText;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => AnswerQuestion(answerText, q.correctAnswer));
            answerButtons[i].image.color = Color.white;
        }
    }

    void AnswerQuestion(string selectedAnswer, string correctAnswer)
    {
        if (isAnswered) return;

        isAnswered = true;
        bool isCorrect = selectedAnswer.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase);

        foreach (Button btn in answerButtons)
        {
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText.text.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase))
            {
                btn.image.color = Color.green;
            }
            else if (btnText.text.Equals(selectedAnswer, System.StringComparison.OrdinalIgnoreCase))
            {
                btn.image.color = Color.red;
            }
        }

        if (isCorrect)
        {
            score += 10;
        }

        StartCoroutine(NextQuestion());
    }

    IEnumerator NextQuestion()
    {
        yield return new WaitForSeconds(1.5f);
        currentQuestionIndex++;
        LoadQuestion();
    }

    IEnumerator TimerCountdown()
    {
        while (true)
        {
            if (!isAnswered)
            {
                timeRemaining -= Time.deltaTime;
                timerCircle.fillAmount = timeRemaining / 20f;
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

                if (timeRemaining <= 5f && !isShaking)
                {
                    StartCoroutine(ShakeTimer());
                    timerText.color = warningColor;
                    timerCircle.color = warningColor;
                }

                if (timeRemaining <= 0)
                {
                    isAnswered = true;
                    StartCoroutine(NextQuestion());
                }
            }
            yield return null;
        }
    }

    IEnumerator ShakeTimer()
    {
        isShaking = true;
        Vector3 originalPosition = timerTransform.localPosition;

        for (int i = 0; i < 10; i++)
        {
            timerTransform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * 5f;
            yield return new WaitForSeconds(0.05f);
        }

        timerTransform.localPosition = originalPosition;
        isShaking = false;
    }

    void EndQuiz()
    {
        resultsPopup.SetActive(true);
        honorText.text = "Honor Gained: " + score;

        int totalPossibleScore = questions.Length * 10;
        int bonusHonor = (score == totalPossibleScore) ? 100 : 0;
        bonusText.text = "Bonus Honor: " + bonusHonor;

        int finalScore = score + bonusHonor;
        totalHonorText.text = "Total Honor: " + finalScore;

        int starsEarned = (score == totalPossibleScore) ? 3 : (score >= totalPossibleScore * 0.7f) ? 2 : 1;
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = i < starsEarned;
        }
    }

    public void OnContinueButtonPressed()
    {
        SceneManager.LoadScene("Easy(Category)");
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}

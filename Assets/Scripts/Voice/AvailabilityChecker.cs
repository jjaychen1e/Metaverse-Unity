using System.Linq;
using UnityEngine;

public class AvailabilityChecker : MonoBehaviour {
    public GameObject voiceRecognitionUI;

    private QuestionPoint[] questionPoints;

    private QuestionPoint currentQuestionPoint;

    private void Start() {
        questionPoints = GameObject.FindGameObjectsWithTag("QuestionPoint")
            .Select(x => x.GetComponent<QuestionPoint>())
            .Where(x => x != null)
            .ToArray();
        DebugHelper.Log(questionPoints.Length + " Question Point(s) found");
    }

    void Update() {
        QuestionPoint firstAvailableQuestionPoint =
            questionPoints.FirstOrDefault(x => {
                var currentPositionXZ = new Vector2(transform.position.x, transform.position.z);
                var questionPointPositionXZ = new Vector2(x.transform.position.x, x.transform.position.z);
                var distance = Vector2.Distance(currentPositionXZ, questionPointPositionXZ);
                return distance < x.Radius;
            });
        if (firstAvailableQuestionPoint != null) {
            if (voiceRecognitionUI.activeSelf == false) {
                DebugHelper.Log("First available question point: " + firstAvailableQuestionPoint.name);
                voiceRecognitionUI.SetActive(true);
            }

            if (currentQuestionPoint != firstAvailableQuestionPoint) {
                currentQuestionPoint = firstAvailableQuestionPoint;
                QANetwork.Shared.GetQuestionList(firstAvailableQuestionPoint.Keyword,
                    (questionListResponse) => {
                        if (questionListResponse != null) {
                            var questions = questionListResponse.candidateQuestions;
                            var questionsText = string.Join("\n", questions);
                            DebugHelper.Log("Got questions: " + questionsText);
                        }
                    });
            }
        }
        else {
            currentQuestionPoint = null;
            if (voiceRecognitionUI.activeSelf == true) {
                voiceRecognitionUI.SetActive(false);
                DebugHelper.Log("No available question point");
            }
        }
    }
}
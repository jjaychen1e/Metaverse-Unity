using Newtonsoft.Json;
using UnityEngine;

public class QuestionAnswerResponse {
    [JsonProperty("user_question")] public string userQuestion;

    [JsonProperty("answer_text")] public string answerText;

    [JsonProperty("context")] public string[] context;
}
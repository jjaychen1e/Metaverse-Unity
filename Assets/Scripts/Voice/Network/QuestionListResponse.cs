using Newtonsoft.Json;

public class QuestionListResponse {
    [JsonProperty("scene_keyword")] public string sceneKeyword;

    [JsonProperty("candidate_questions")] public string[] candidateQuestions;
}
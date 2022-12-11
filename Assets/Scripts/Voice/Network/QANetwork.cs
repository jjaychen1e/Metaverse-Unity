using System;
using System.Text;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class QANetwork : MonoBehaviour {
    private string host = "http://172.23.148.83:9201/api/";

    private static QANetwork _shared;

    private static object _lock = new object();

    public static QANetwork Shared {
        get {
            if (_shared == null) {
                lock (_lock) {
                    if (_shared == null) {
                        var qaNetork = GameObject.Find("QANetwork");
                        if (qaNetork != null) {
                            _shared = qaNetork.GetComponent<QANetwork>();
                        }

                        if (_shared == null) {
                            GameObject go = new GameObject("QANetwork");
                            _shared = go.AddComponent<QANetwork>();
                        }
                    }
                }
            }

            return _shared;
        }
    }

    private IEnumerator Post(string url, string bodyString, Action<UnityWebRequest> callback) {
        var request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyString));
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError(request.error);
        }
        else {
            Debug.Log("Status Code: " + request.responseCode);
            Debug.Log("Result: " + request.downloadHandler.text);
            callback(request);
        }
    }

    public void GetQuestionList(string keyword, Action<QuestionListResponse> callback) {
        var reqArg = new QuestionListArgs(keyword);
        var bodyString = JsonConvert.SerializeObject(reqArg);
        var url = host + "qlist";
        StartCoroutine(Post(url, bodyString, (request) => {
            var dataString = request.downloadHandler.text;
            QuestionListResponse response =
                JsonConvert.DeserializeObject<QuestionListResponse>(dataString);
            callback(response);
        }));
    }

    public void GetQuestionAnswer(string question, Action<QuestionAnswerResponse> callback) {
        var reqArg = new QuestionAnswerArgs(question);
        var bodyString = JsonConvert.SerializeObject(reqArg);
        var url = host + "qa";
        StartCoroutine(Post(url, bodyString, (request) => {
            var dataString = request.downloadHandler.text;
            QuestionAnswerResponse response =
                JsonConvert.DeserializeObject<QuestionAnswerResponse>(dataString);
            callback(response);
        }));
    }
}
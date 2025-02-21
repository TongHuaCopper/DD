using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DeepSeekAPI_Sample : MonoBehaviour
{
    private string apiKey = "sk-16be6c290cee4d4393ae78e76398169c";
    private string apiUrl = "https://api.deepseek.com/v1/chat/completions";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SendMessageToDeepSeek("你好啊",null);
    }

    public void SendMessageToDeepSeek(String message,UnityAction<string> callback)
    {
        StartCoroutine(PostRequest(message,callback));
    }

    IEnumerator PostRequest(string message,UnityAction<string> callback){
        //创建匿名类型请求体
        var requestBody = new 
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new {role = "user",content = message}
            }
        };
        //使用Newtonsoft.Json序列化
        string jsonBody = JsonConvert.SerializeObject(requestBody);
        Debug.Log(jsonBody);

        //创建UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(apiUrl,"POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type","application/json");    //设置上传处理器
        request.SetRequestHeader("Authorization","Bearer " + apiKey);    //设置上传处理器

        //发送请求
        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);    //打印详细错误信息
        }
        else 
        {
            //处理响应
            string responseJson = request.downloadHandler.text;
            Debug.Log("Response: " + responseJson);
        }
    }
}

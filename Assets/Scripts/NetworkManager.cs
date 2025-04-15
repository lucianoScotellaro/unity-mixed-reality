using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{   

    //Constants
    private const string BASE_URL = "http://med-quad.univaq.it/univaq/vr/script.php";
    private const int TIMEOUT_IN_SECONDS = 100;

    //Fields
    private readonly HttpClient httpClient = GetClient();

    [SerializeField]
    private QRCodeManager qrCodeManager;

    //Events
    public event EventHandler<OnDownloadBaseDataCompletedEventArgs> OnDownloadBaseDataCompleted;
    public event EventHandler<OnDownloadAudioClipCompletedEventArgs> OnDownloadAudioClipCompleted;
    public event Action OnDownload3DModelCompleted;

    private void Start(){

        qrCodeManager.OnQRCodeChanged += DownloadResources;
    }

    private async void DownloadResources(object sender, OnQRCodeChangedEventArgs args){

        Data data = await DownloadBaseDataAsync(args.id);

        if(data.Audio != null){
           DownloadAudioClipAsync(data.Audio);
        }

        if(data.Files != null){
            Download3DModelAsync(data.Files);
        }
    }

    private async Task<Data> DownloadBaseDataAsync(string id){
        Data data = null;

        HttpResponseMessage response = await httpClient.GetAsync($"?id={id}");
        if(response.IsSuccessStatusCode)
        {
            data = await response.Content.ReadAsAsync<Data>();
        }
        OnDownloadBaseDataCompleted?.Invoke(httpClient, new OnDownloadBaseDataCompletedEventArgs(data));

        return data;
    }

    private void DownloadAudioClipAsync(string audioURL){

        // UnityWebRequestMultimedia just skips the DownloadHandler configuration
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioURL, AudioType.MPEG);
        request.timeout = TIMEOUT_IN_SECONDS;

        //Request is handled asynchronously "under the hood"
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        //Operations on completed requests are implemented through events
        operation.completed += (AsyncOperation operation) => {
            if(request.responseCode == 200){
                
                AudioClip clip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                OnDownloadAudioClipCompleted?.Invoke(request, new OnDownloadAudioClipCompletedEventArgs(clip));   
            }
        };
    }

    private async Task Download3DModelAsync(string[] files){
        List<Task> tasks = new List<Task>();

        foreach(string fileURL in files){
            string fileName = fileURL.Substring(fileURL.LastIndexOf("/") + 1);
            string fileType = fileName.Substring(fileName.LastIndexOf(".") + 1);
            
            tasks.Add(DownloadFileAsync(fileURL, fileName, fileType));
        }
        
        await Task.WhenAll(tasks);

        OnDownload3DModelCompleted?.Invoke();
    }

    private async Task DownloadFileAsync(string fileURL, string fileName, string fileType){
        string filename = fileType != "obj" ? fileName : "model.obj";
        if(fileType != "obj" && File.Exists(Path.Combine(Constants.MODELS_TEMPORARY_FOLDER_PATH, filename))){
            return;
        }

        HttpResponseMessage response = await httpClient.GetAsync(fileURL);
        if(response.IsSuccessStatusCode){

            byte[] data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(Path.Combine(Constants.MODELS_TEMPORARY_FOLDER_PATH, filename), data);
        }
    }

    private static HttpClient GetClient(){
        HttpClient instance = new HttpClient();
        instance.BaseAddress = new Uri(BASE_URL);
        instance.DefaultRequestHeaders.Accept.Clear();
        instance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return instance;
    }
}

public class Data{
    public int Id{get; set;}
    public string Name{ get; set;}
    public string Description{get; set;}
    public string Audio{get; set;}
    public string Video{get; set;}
    public string[] Files{get; set;}
}

public class OnDownloadBaseDataCompletedEventArgs : EventArgs{
    public Data data;

    public OnDownloadBaseDataCompletedEventArgs(Data data){
        this.data = data;
    }
}

public class OnDownloadAudioClipCompletedEventArgs : EventArgs{
    public AudioClip clip;

    public OnDownloadAudioClipCompletedEventArgs(AudioClip clip){
        this.clip = clip;
    }
}


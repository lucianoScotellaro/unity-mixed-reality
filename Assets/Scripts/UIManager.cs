using UnityEngine;
using TMPro;
using UnityEngine.Video;
using MixedReality.Toolkit.UX;

public class UIManager : MonoBehaviour
{

    //Managers
    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private QRCodeManager qrCodeManager;

    [SerializeField]
    private ModelManager modelManager;

    //Canvas object
    [SerializeField]
    private GameObject mainCanvasObject;

    //UI elements
    [SerializeField]
    private TextMeshProUGUI title;

    [SerializeField]
    private TextMeshProUGUI body;

    [SerializeField]
    private VideoPlayer videoPlayer;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private PressableButton videoPlayButton;
    private FontIconSelector videoButtonIconSelector;

    [SerializeField]
    private PressableButton audioPlayButton;
    private FontIconSelector audioButtonIconSelector;

    [SerializeField]
    private PressableButton showModelButton;

    private void Start(){

        videoButtonIconSelector = videoPlayButton.GetComponentInChildren<FontIconSelector>();
        audioButtonIconSelector = audioSource.GetComponentInChildren<FontIconSelector>();

        //Subscribe to events

        //QR
        qrCodeManager.OnQRCodeChanged += OnQRCodeChanged;

        //Network
        networkManager.OnDownloadBaseDataCompleted += OnDownloadBaseDataCompleted;
        networkManager.OnDownloadAudioClipCompleted += OnDownloadAudioClipCompleted;

        //Model
        modelManager.OnModelLoaded += OnModelLoaded;

        //Buttons
        videoPlayButton.OnClicked.AddListener(OnVideoPlayButtonClicked);
        audioPlayButton.OnClicked.AddListener(OnAudioPlayButtonClicked);
        showModelButton.OnClicked.AddListener(OnShowModelButtonClicked);

        mainCanvasObject.SetActive(false);
    }

    private void OnQRCodeChanged(object sender, OnQRCodeChangedEventArgs args){

        mainCanvasObject.SetActive(false);
        transform.position = args.position;
    }

    private void OnDownloadBaseDataCompleted(object sender, OnDownloadBaseDataCompletedEventArgs args){

        if(args.data == null){
            title.text = "Connection error!";
            return;
        }

        //Text
        title.text = args.data.Name != null ? args.data.Name : "Name was not found.";
        body.text = args.data.Description != null ? args.data.Description : "Description was not found.";

        //Video
        if(args.data.Video != null){
            videoPlayer.url = args.data.Video;
        }
    }

    private void OnDownloadAudioClipCompleted(object sender, OnDownloadAudioClipCompletedEventArgs args){
        audioSource.clip = args.clip;
    }

    private void OnModelLoaded(){
        ShowCanvas();
    }

    public void ShowCanvas()
    {
        Vector3 userPosition = Camera.main.transform.position;
        mainCanvasObject.transform.LookAt(userPosition, Camera.main.transform.rotation * Vector3.up);
        mainCanvasObject.transform.Rotate(0, 180, 0);
        mainCanvasObject.SetActive(true);
        videoPlayer.Prepare(); 
    }

    private void OnShowModelButtonClicked(){
        mainCanvasObject.SetActive(false);
        modelManager.ShowModel();
    }

    private void OnVideoPlayButtonClicked(){

        if(videoPlayer.isPrepared && !videoPlayer.isPlaying){
            videoPlayer.Play();
            videoButtonIconSelector.CurrentIconName = "Icon 135";
        }else if(videoPlayer.isPrepared && videoPlayer.isPlaying){
            videoPlayer.Pause();
            videoButtonIconSelector.CurrentIconName = "Icon 122";
        }
    }

    private void OnAudioPlayButtonClicked(){

        if(audioSource.clip != null && !audioSource.isPlaying){
            audioSource.Play();
            audioButtonIconSelector.CurrentIconName = "Icon 135";
        }else if(audioSource.clip != null && audioSource.isPlaying){
            audioSource.Pause();
            audioButtonIconSelector.CurrentIconName = "Icon 122";
        }
    }
}

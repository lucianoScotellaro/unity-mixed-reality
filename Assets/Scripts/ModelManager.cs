using System;
using System.IO;
using Dummiesman;
using MixedReality.Toolkit;
using UnityEngine;

public class ModelManager : MonoBehaviour
{

    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private QRCodeManager qrCodeManager;

    [SerializeField]
    private UIManager uiManager;

    public event Action OnModelLoaded;

    private Vector3 qrCodePosition;

    private GameObject model;

    private void Start(){

        networkManager.OnDownload3DModelCompleted += LoadAndPosition;
        qrCodeManager.OnQRCodeChanged += OnQRCodeChanged;

        Directory.CreateDirectory(Constants.MODELS_TEMPORARY_FOLDER_PATH);
    }

    private void LoadAndPosition(){

        // Loads model
        model = new OBJLoader().Load(Path.Combine(Constants.MODELS_TEMPORARY_FOLDER_PATH, "model.obj"));
        model.SetActive(false);

        //Interaction setup
        Mesh mesh = model.GetComponentInChildren<MeshFilter>().mesh;
        MeshCollider collider = model.AddComponent<MeshCollider>();
        StatefulInteractable statefulInteractable = model.AddComponent<StatefulInteractable>();
        collider.sharedMesh = mesh;
        statefulInteractable.colliders.Add(collider);
        statefulInteractable.OnClicked.AddListener(OnModelClicked);
        
        //Positioning
        model.transform.position = - mesh.bounds.center + qrCodePosition;

        OnModelLoaded?.Invoke();
    }

    private void OnQRCodeChanged(object sender, OnQRCodeChangedEventArgs args){

        //Clear possible previous model
        if(model != null){
            GameObject.Destroy(model);
        }

        //Save QR Code position
        qrCodePosition = args.position;
    }

    private void OnModelClicked(){
        model.SetActive(false);
        uiManager.ShowCanvas();
    }

    public void ShowModel(){
        model.SetActive(true);
    }

    private void OnApplicationQuit(){

        //Clear files
        Directory.Delete(Constants.MODELS_TEMPORARY_FOLDER_PATH, true);
    }
}

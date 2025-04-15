using Microsoft.MixedReality.OpenXR;
using UnityEngine;
using System;

[RequireComponent(typeof(ARMarkerManager))]
public class QRCodeManager : MonoBehaviour
{
    //Fields
    [SerializeField]
    private ARMarkerManager markerManager;

    private FixedSizeQueue<string> idsHistory = new FixedSizeQueue<string>(3);

    private string currentDisplayedQRCodeId = null;

    private bool stable;

    //Events
    public event EventHandler<OnQRCodeChangedEventArgs> OnQRCodeChanged;
    
    private void Start(){

        markerManager.markersChanged += OnMarkersChanged;
    }

    private void OnMarkersChanged(ARMarkersChangedEventArgs args){

        foreach(ARMarker updatedMarker in args.updated){
            HandleUpdatedMarker(updatedMarker);
        }
    }

    private void HandleUpdatedMarker(ARMarker marker){

        string updatedQRId = marker.GetDecodedString();

        //Add ID to "history"
        idsHistory.Enqueue(updatedQRId);

        //Wait until at least 3 ids are available
        if(idsHistory.Count() < idsHistory.Size){
            return;
        }
        
        if(updatedQRId != currentDisplayedQRCodeId){

            //Check if updates on that QR are consistent
            stable = true;
            foreach(string previousId in idsHistory){
                if(updatedQRId != previousId){
                    stable = false;
                }
            }

            //If updates consistent proceeds to invoke event
            if(stable){
                currentDisplayedQRCodeId = updatedQRId;
                OnQRCodeChanged?.Invoke(this, new OnQRCodeChangedEventArgs(updatedQRId, marker.transform.position));
            }
        }
    }
}

public class OnQRCodeChangedEventArgs : EventArgs{

    public string id; 

    public Vector3 position; 

    public OnQRCodeChangedEventArgs(string id, Vector3 position){

        this.id = id;
        this.position = position;
    }
}

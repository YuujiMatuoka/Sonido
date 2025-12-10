using UnityEngine;

public interface IInspectable
{
    void OnFocus();             // Raycast activo sobre el objeto
    void OnUnfocus();           // Raycast sale del objeto
    void OnPopUp();
    void UnPopUp();
    string GetDescription();    

    AudioClip GetInspectionAudioClip();
    float GetInspectionDisplayDuration();
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeVisualPair
{
    public TimeState timeState;
    public GameObject visual;
}
public class TimeCube : MonoBehaviour
{
    /*[SerializeField] private List<TimeVisualPair> timeVisuals = new List<TimeVisualPair>();

    protected override void Start()
    {
        usePositionSaving = false; //Esta linea es para que el cambio de pocisiones no se calcule en este objeto
        base.Start();
        UpdateVisual(TimeTravelManager.Instance.CurrentTimeState);
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        UpdateVisual(newTimeState);
    }

    private void UpdateVisual(TimeState newTimeState)
    {
        foreach (var pair in timeVisuals)
        {
            // Activa el objeto que corresponde al estado de tiempo actual
            pair.visual.SetActive(pair.timeState == newTimeState);
        }
    }*/
}




using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimeTravel //Interface que se encarga de etiquetar los objetos afectados por el tiempo
{
    void PreTimeChange(TimeState newTimeState);
    void OnTimeChanged(TimeState newTimeState);
}
public enum TimeState // Los estados de tiempo que existen en el juego
{
    Origin,
    L1
}

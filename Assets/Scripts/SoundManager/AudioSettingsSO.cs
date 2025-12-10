using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "Settings/Audio Settings")]
public class AudioSettingsSO : ScriptableObject
{
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private const string SAVE_KEY = "AudioSettings";

    public void Save()
    {
        var data = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(SAVE_KEY, data);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SAVE_KEY), this);
        }
        else
        {
            Save();
        }
    }
}

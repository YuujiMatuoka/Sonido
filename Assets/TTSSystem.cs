using UnityEngine;
using System.Diagnostics;
using System.Collections;

public class TTSSystem : MonoBehaviour
{
    public static TTSSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpeakText(string text)
    {
        StartCoroutine(SpeakCoroutine(text));
    }

    private IEnumerator SpeakCoroutine(string text)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string cleanText = text.Replace("'", "''").Replace("\"", "\\\"");

        Process process = new Process();
        process.StartInfo.FileName = "powershell";
        process.StartInfo.Arguments = $"-Command \"Add-Type -AssemblyName System.Speech; (New-Object System.Speech.Synthesis.SpeechSynthesizer).Speak('{cleanText}');\"";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;

        bool started = false;
        try
        {
            started = process.Start();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning($"TTS no disponible: {e.Message}");
        }

        if (started)
        {
            // Esperar basado en la longitud del texto
            float waitTime = Mathf.Max(2f, text.Length * 0.08f);
            yield return new WaitForSeconds(waitTime);

            if (!process.HasExited)
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(500))
                    process.Kill();
            }
        }

        process.Dispose();
#else
        UnityEngine.Debug.Log($"[TTS Simulado]: {text}");
        yield return null;
#endif
    }

    public void StopAllSpeech()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Cerrar todos los procesos de PowerShell
        Process[] processes = Process.GetProcessesByName("powershell");
        foreach (Process process in processes)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    if (!process.WaitForExit(500))
                        process.Kill();
                }
            }
            catch { }
        }
#endif
    }
}
using UnityEngine;
using System.Diagnostics;
using System.Collections;

public class VoiceTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(TestTTS());
    }

    IEnumerator TestTTS()
    {
        UnityEngine.Debug.Log("🎯 Probando TTS via PowerShell...");

        string text = "Hola, soy una voz automática en Unity.";
        yield return StartCoroutine(SpeakWithPowerShell(text));

        UnityEngine.Debug.Log("✅ Prueba completada");
    }

    private IEnumerator SpeakWithPowerShell(string text)
    {
        // Escapar el texto para PowerShell
        string cleanText = text.Replace("'", "''").Replace("\"", "\\\"");

        string powerShellCommand = $@"
            Add-Type -AssemblyName System.Speech;
            $speaker = New-Object System.Speech.Synthesis.SpeechSynthesizer;
            $speaker.Speak('{cleanText}');
        ";

        Process process = new Process();
        process.StartInfo.FileName = "powershell";
        process.StartInfo.Arguments = $"-Command \"{powerShellCommand}\"";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;

        UnityEngine.Debug.Log("🔊 Iniciando PowerShell TTS...");

        bool success = false;
        try
        {
            process.Start();
            success = true;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"❌ Error al iniciar TTS: {e.Message}");
        }

        // Solo esperar si fue exitoso
        if (success)
        {
            // Esperar estimación basada en longitud del texto
            float waitTime = Mathf.Max(3f, text.Length * 0.1f);
            UnityEngine.Debug.Log($"⏳ Esperando {waitTime} segundos...");
            yield return new WaitForSeconds(waitTime);

            // Cerrar el proceso si aún está ejecutándose
            if (!process.HasExited)
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(1000))
                {
                    process.Kill();
                }
            }

            UnityEngine.Debug.Log("✅ TTS completado");
        }

        process.Dispose();
    }
}
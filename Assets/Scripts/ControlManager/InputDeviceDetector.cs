using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputDeviceDetector : MonoBehaviour
{
    public static bool IsGamepadActive { get; private set; }
    public static event Action<bool> OnInputDeviceChanged;

    private InputDevice lastActiveDevice;
    private float lastDeviceCheckTime;

    void Update()
    {
        // Verificar cada 0.1 segundos para optimizar
        if (Time.time - lastDeviceCheckTime < 0.1f) return;

        lastDeviceCheckTime = Time.time;
        bool wasGamepad = IsGamepadActive;
        InputDevice currentDevice = GetActiveInputDevice();

        if (currentDevice != lastActiveDevice)
        {
            lastActiveDevice = currentDevice;
            IsGamepadActive = currentDevice is Gamepad;

            if (IsGamepadActive != wasGamepad)
            {
                Debug.Log($"Input device changed to: {(IsGamepadActive ? "Gamepad" : "Keyboard/Mouse")}");
                OnInputDeviceChanged?.Invoke(IsGamepadActive);
            }
        }
    }

    private InputDevice GetActiveInputDevice()
    {
        if (Mouse.current != null && (Mouse.current.delta.ReadValue().magnitude > 0 || Mouse.current.leftButton.isPressed))
            return Mouse.current;

        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
            return Keyboard.current;

        if (Gamepad.current != null)
        {
            // Verificar si hay input significativo del gamepad
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            float triggerInput = Mathf.Max(Gamepad.current.leftTrigger.ReadValue(), Gamepad.current.rightTrigger.ReadValue());

            if (leftStick.magnitude > 0.1f || rightStick.magnitude > 0.1f || triggerInput > 0.1f)
                return Gamepad.current;
        }

        return lastActiveDevice ?? Keyboard.current; // Default to keyboard
    }
}

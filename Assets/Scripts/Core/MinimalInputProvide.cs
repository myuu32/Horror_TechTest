using System;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HorrorTech.Core
{
    [DisallowMultipleComponent]
    [AddComponentMenu("HorrorTech/Core/Minimal Input Provider")]
    public class MinimalInputProvider : MonoBehaviour, IInputProvider
    {
        [Header("Look (mouse)")]
        [Tooltip("Enable mouse look. If disabled, look delta is always zero.")]
        public bool enableLook = true;

        [Tooltip("Use raw mouse delta from the new Input System when available; otherwise fall back to old Input.")]
        public bool preferNewInputSystemForLook = true;

        [Header("Move (digital axes)")]
        [Tooltip("Enable digital movement axes (e.g., WASD). If disabled, movement is always zero.")]
        public bool enableMove = false;

        [Tooltip("Define digital axes (positive/negative keys). Only axes named 'MoveX' and 'MoveY' are consumed by Player logic.")]
        public List<DigitalAxis> digitalAxes = new List<DigitalAxis>
        {
            // Uncomment if you want defaults pre-added:
            // new DigitalAxis{ name = "MoveX", positiveOld = KeyCode.D, negativeOld = KeyCode.A, positiveNew = Key.D, negativeNew = Key.A },
            // new DigitalAxis{ name = "MoveY", positiveOld = KeyCode.W, negativeOld = KeyCode.S, positiveNew = Key.W, negativeNew = Key.S },
        };

        public Vector2 GetLookDelta()
        {
            if (!enableLook) return Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (preferNewInputSystemForLook)
            {
                var mouse = Mouse.current;
                if (mouse != null)
                    return mouse.delta.ReadValue();
            }
#endif
            float dx = Input.GetAxisRaw("Mouse X");
            float dy = Input.GetAxisRaw("Mouse Y");
            return new Vector2(dx, dy);
        }

        public Vector2 GetMoveAxis()
        {
            if (!enableMove) return Vector2.zero;

            float x = ReadDigital("MoveX");
            float y = ReadDigital("MoveY");
            return new Vector2(x, y);
        }

        public bool GetJumpDown() => false;
        public bool GetSprintHeld() => false;
        public bool GetCrouchHeld() => false;
        public bool GetInteractDown() => false;
        public bool GetCaptureDown() => false;

        float ReadDigital(string axisName)
        {
            var axis = digitalAxes.Find(a => string.Equals(a.name, axisName, StringComparison.Ordinal));
            if (axis == null) return 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                bool pos = axis.HasNewPositive && Keyboard.current[axis.positiveNew].isPressed;
                bool neg = axis.HasNewNegative && Keyboard.current[axis.negativeNew].isPressed;
                return (pos ? 1f : 0f) - (neg ? 1f : 0f);
            }
#endif
            bool posOld = axis.HasOldPositive && Input.GetKey(axis.positiveOld);
            bool negOld = axis.HasOldNegative && Input.GetKey(axis.negativeOld);
            return (posOld ? 1f : 0f) - (negOld ? 1f : 0f);
        }

        [Serializable]
        public class DigitalAxis
        {
            [Tooltip("Axis name. Player movement expects 'MoveX' and 'MoveY'.")]
            public string name = "MoveX";

            [Header("Old Input (fallback)")]
            [Tooltip("Positive direction key (e.g., D or W).")]
            public KeyCode positiveOld = KeyCode.None;
            [Tooltip("Negative direction key (e.g., A or S).")]
            public KeyCode negativeOld = KeyCode.None;

#if ENABLE_INPUT_SYSTEM
            [Header("New Input System")]
            [Tooltip("Positive direction key (e.g., Key.D or Key.W).")]
            public Key positiveNew = Key.None;
            [Tooltip("Negative direction key (e.g., Key.A or Key.S).")]
            public Key negativeNew = Key.None;
#endif

            public bool HasOldPositive => positiveOld != KeyCode.None;
            public bool HasOldNegative => negativeOld != KeyCode.None;

#if ENABLE_INPUT_SYSTEM
            public bool HasNewPositive => positiveNew != Key.None;
            public bool HasNewNegative => negativeNew != Key.None;
#endif
        }
    }
}
using UnityEngine;
using HorrorTech.Core;

namespace HorrorTech.CameraSystem
{
    [AddComponentMenu("HorrorTech/Camera/Player Camera Controller")]
    [DisallowMultipleComponent]
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("External Rig References (NOT parented under GameManager)")]
        [Tooltip("Yaw pivot (e.g., PlayerRig in the scene).")]
        public Transform yawPivot;

        [Tooltip("Pitch pivot (usually a child of yawPivot).")]
        public Transform pitchPivot;

        [Tooltip("Transform that holds the actual Camera component.")]
        public Transform cameraTransform;

        [Header("Input")]
        [Tooltip("MonoBehaviour that implements IInputProvider (e.g., HybridInputProvider on GameManager).")]
        public MonoBehaviour inputProviderBehaviour;
        IInputProvider _input;

        [Header("Look Sensitivity")]
        public float sensitivityX = 1.5f;
        public float sensitivityY = 1.5f;
        public bool invertY = false;
        public bool scaleByDeltaTime = false;

        [Header("Vertical Clamp (deg)")]
        public Vector2 verticalClamp = new Vector2(-80f, 80f);

        [Header("Cursor")]
        public bool lockCursorOnStart = true;

        [Header("Smoothing")]
        [Range(0f, 1f)] public float rotationLerp = 0.18f;
        [Range(0f, 1f)] public float positionLerp = 0.20f;

        [Header("Handheld/Bob (optional)")]
        public bool enableBob = true;
        public float bobFrequency = 1.7f;
        public float bobAmplitude = 0.02f;
        public Vector3 localOffset = Vector3.zero;
        public Vector3 localNoiseAmplitude = new Vector3(0.002f, 0.002f, 0.002f);
        public float noiseSpeed = 6f;

        [Header("Debug (read-only)")]
        [SerializeField] CameraState current;
        [SerializeField] CameraState target;

        bool _valid;
        bool _pitchIsChildOfYaw;
        float _noiseTime;

        void Awake()
        {
            _input = inputProviderBehaviour as IInputProvider;
            if (_input == null)
            {
                foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                {
                    if (mb is IInputProvider p) { _input = p; break; }
                }
            }

            _valid = yawPivot && pitchPivot && cameraTransform;
            if (!_valid)
            {
                Debug.LogError("[PlayerCameraController] Missing references. Assign yawPivot, pitchPivot, cameraTransform.", this);
                enabled = false;
                return;
            }

            _pitchIsChildOfYaw = (pitchPivot.parent == yawPivot);

            current.pos = target.pos = yawPivot.position;
            current.yaw = target.yaw = yawPivot.eulerAngles.y;
            current.pitch = target.pitch = pitchPivot.localEulerAngles.x;

            if (lockCursorOnStart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (!_pitchIsChildOfYaw)
            {
                Debug.LogWarning("[PlayerCameraController] pitchPivot is NOT a child of yawPivot. " +
                                 "Using math composition (yaw * pitch). This is fine, but parenting pitch under yaw is recommended.", this);
            }
        }

        void Update()
        {
            if (!_valid) return;

            if (_input != null)
            {
                Vector2 delta = _input.GetLookDelta();
                if (scaleByDeltaTime) delta *= Time.deltaTime;

                float sx = sensitivityX;
                float sy = sensitivityY * (invertY ? 1f : -1f);

                target.yaw += delta.x * sx;
                target.pitch += delta.y * sy;
                target.pitch = Mathf.Clamp(target.pitch, verticalClamp.x, verticalClamp.y);
            }

            Vector3 additive = Vector3.zero;
            if (enableBob)
            {
                _noiseTime += Time.deltaTime * bobFrequency;
                additive.y += Mathf.Sin(_noiseTime) * bobAmplitude;
            }
            if (noiseSpeed > 0f)
            {
                float t = Time.time * noiseSpeed;
                additive += Vector3.Scale(new Vector3(
                    Mathf.PerlinNoise(t, 0.37f) - 0.5f,
                    Mathf.PerlinNoise(0.81f, t) - 0.5f,
                    Mathf.PerlinNoise(t * 0.33f, t * 0.77f) - 0.5f
                ), localNoiseAmplitude);
            }

            Vector3 desiredPos = yawPivot.position + yawPivot.TransformVector(localOffset) + additive;
            target.pos = desiredPos;

            current.LerpTowards(target, positionLerp, rotationLerp);

            yawPivot.rotation = Quaternion.Euler(0f, current.yaw, 0f);

            if (_pitchIsChildOfYaw)
            {
                pitchPivot.localRotation = Quaternion.Euler(current.pitch, 0f, 0f);

                cameraTransform.position = pitchPivot.position;
                cameraTransform.rotation = pitchPivot.rotation;
            }
            else
            {
                Quaternion yawRot = Quaternion.Euler(0f, current.yaw, 0f);
                Quaternion pitchRot = Quaternion.Euler(current.pitch, 0f, 0f);
                Quaternion finalRot = yawRot * pitchRot;

                cameraTransform.position = current.pos;
                cameraTransform.rotation = finalRot;

                pitchPivot.localRotation = Quaternion.Euler(current.pitch, 0f, 0f);
            }
        }

        // Public API
        public float GetYaw() => current.yaw;
        public float GetPitch() => current.pitch;
        public Vector3 GetCameraPosition() => cameraTransform.position;
        public Vector3 GetCameraForward() => cameraTransform.forward;

        public void SetSensitivity(float x, float y)
        {
            sensitivityX = Mathf.Max(0f, x);
            sensitivityY = Mathf.Max(0f, y);
        }

        public void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
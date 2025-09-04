using UnityEngine;

namespace HorrorTech.CameraSystem
{
    [System.Serializable]
    public struct CameraState
    {
        public float yaw;
        public float pitch;
        public Vector3 pos;

        public void LerpTowards(CameraState target, float posLerp, float rotLerp)
        {
            yaw = Mathf.LerpAngle(yaw, target.yaw, rotLerp);
            pitch = Mathf.LerpAngle(pitch, target.pitch, rotLerp);
            pos = Vector3.Lerp(pos, target.pos, posLerp);
        }
    }
}
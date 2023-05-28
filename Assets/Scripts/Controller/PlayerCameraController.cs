using Cinemachine;
using UnityEngine;

    public class PlayerCameraController : MonoBehaviour
    {
        new private CinemachineFreeLook camera;
        private bool keepRecenteringDisable = false;

        private BiasOffset biasOffset;

    private void Awake()
    {
        camera = FindObjectOfType<CinemachineFreeLook>();
    }
    void Update()
        {
            if (biasOffset == null) return;
            if (biasOffset.startTime + biasOffset.duration >= Time.time)
            {
                camera.m_XAxis.Value += ((biasOffset.offset - biasOffset.startingBias) / biasOffset.duration) * Time.deltaTime;
                //camera.m_Heading.m_Bias =;
            }
        }
        public void UpdateCamera(float horizontalDamping, float cameraDistance = 4.2f, float cameraHeight = 2.6f)
        {
            //var composer = camera.GetCinemachineComponent<CinemachineComposer>();
            //var thirdPersonFollow = camera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            //composer.m_HorizontalDamping = Mathf.MoveTowards(composer.m_HorizontalDamping, horizontalDamping, 50f * Time.deltaTime);
            //thirdPersonFollow.CameraDistance = Mathf.MoveTowards(thirdPersonFollow.CameraDistance, cameraDistance, 4f * Time.deltaTime);

            camera.m_Orbits[1].m_Height = Mathf.MoveTowards(camera.m_Orbits[1].m_Height, cameraHeight, 1.5f * Time.deltaTime);
            camera.m_Orbits[1].m_Radius = Mathf.MoveTowards(camera.m_Orbits[1].m_Radius, cameraDistance, 4f * Time.deltaTime);

            /*for (int i = 0; i < 3; i++)
            {
                var orbit = camera.m_Orbits[i];

                orbit.m_Height = cameraHeight;
                orbit.m_Radius = cameraDistance;
            }*/
        }

        public void SetCinemachineNoiseIntensity(float magnitude, float maxRunningSpeed)
        {
            for (int i = 0; i < 3; i++)
            {
                var rig = camera.GetRig(i);
                //rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = .5f + magnitude / maxRunningSpeed;

                /*if (fsm.GetCurrentState().ID == MovementStatus.Walking)
                    rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = .5f;
                else*/
                rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = .5f + magnitude / maxRunningSpeed;
            }
        }
        public void SetRecenteringCamera(bool recenter)
        {
            if (recenter && !camera.m_RecenterToTargetHeading.m_enabled && keepRecenteringDisable)
            {
                keepRecenteringDisable = false;
                return;
            }
            camera.m_RecenterToTargetHeading.m_enabled = recenter;
        }
        public bool GetRecenteringCamera()
        {
            return camera.m_RecenterToTargetHeading.m_enabled;
        }
        public void KeepRecenteringCameraDisable()
        {
            keepRecenteringDisable = true;
        }
        internal void SetBiasOffset(float finalBias, float duration)
        {
            biasOffset = new BiasOffset(finalBias, duration, camera.m_Heading.m_Bias);
        }
    }

class BiasOffset
{
    public float offset = 0f;
    public float duration = 0f;
    public float startTime = 0f;
    public float startingBias = 0f;

    public BiasOffset(float offset, float duration, float currentBias)
    {
        this.offset = offset;
        this.duration = duration;
        this.startTime = Time.time;
        this.startingBias = currentBias;
    }
}

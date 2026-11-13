using UnityEngine;
using UnityEngine.UI;

namespace FullCount
{
    public sealed class BaseballPrototype : MonoBehaviour
    {
        #region References and settings
        [SerializeField] private Rigidbody ball;
        [SerializeField] private SphereCollider ballCollider;
        [SerializeField] private Transform mound;
        [SerializeField] private Rigidbody bat;
        [SerializeField] private Button pitchButton;
        [SerializeField] private Button swingButton;
        [SerializeField] private Button resetButton;
        [SerializeField, Min(0.1f)] private float pitchSpeed = 5f;
        [SerializeField] private float readyAngle = 55f;
        [SerializeField] private float finishAngle = -75f;
        [SerializeField, Min(0.02f)] private float swingDuration = 0.26f;

        private bool pitchInProgress;
        private bool swingUsed;
        private bool swinging;
        private float swingElapsed;
        #endregion

        private void Awake()
        {
            if (Application.isMobilePlatform)
                Screen.orientation = ScreenOrientation.LandscapeLeft;

            pitchButton.onClick.AddListener(Pitch);
            swingButton.onClick.AddListener(Swing);
            resetButton.onClick.AddListener(ResetPlay);
            ResetPlay();
        }

        private void OnDestroy()
        {
            pitchButton.onClick.RemoveListener(Pitch);
            swingButton.onClick.RemoveListener(Swing);
            resetButton.onClick.RemoveListener(ResetPlay);
        }

        private void FixedUpdate()
        {
            if (!swinging)
                return;

            swingElapsed += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(swingElapsed / swingDuration);
            float angle = Mathf.Lerp(readyAngle, finishAngle, progress);
            bat.MoveRotation(Quaternion.Euler(0f, angle, 0f));
            swinging = progress < 1f;
        }

        public void Pitch()
        {
            if (pitchInProgress)
                return;

            pitchInProgress = true;
            ball.isKinematic = false;
            Vector3 velocity = Vector3.back * pitchSpeed;
            float radius = ballCollider.radius * ballCollider.transform.lossyScale.y;
            ball.maxAngularVelocity = Mathf.Max(50f, pitchSpeed / radius);
            ball.linearVelocity = velocity;
            ball.angularVelocity = Vector3.Cross(Vector3.up, velocity) / radius;
            UpdateButtons();
        }

        public void Swing()
        {
            if (!pitchInProgress || swingUsed)
                return;

            swingUsed = true;
            swinging = true;
            swingElapsed = 0f;
            UpdateButtons();
        }

        public void ResetPlay()
        {
            pitchInProgress = false;
            swingUsed = false;
            swinging = false;
            swingElapsed = 0f;

            if (!ball.isKinematic)
            {
                ball.linearVelocity = Vector3.zero;
                ball.angularVelocity = Vector3.zero;
            }

            ball.isKinematic = true;
            ball.position = mound.position;
            ball.rotation = Quaternion.identity;
            bat.rotation = Quaternion.Euler(0f, readyAngle, 0f);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            pitchButton.interactable = !pitchInProgress;
            swingButton.interactable = pitchInProgress && !swingUsed;
        }
    }
}

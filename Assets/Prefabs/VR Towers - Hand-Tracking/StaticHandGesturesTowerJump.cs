using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Hands.Gestures;

namespace UnityEngine.XR.Hands.Samples.GestureSample
{
    /// <summary>
    /// A gesture that detects when a hand is held in a static shape and orientation for a minimum amount of time.
    /// </summary>
    public class StaticHandGesturesTowerJump : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The hand tracking events component to subscribe to receive updated joint data to be used for gesture detection.")]
        XRHandTrackingEvents m_HandTrackingEvents;

        [SerializeField]
        [Tooltip("The hand shape or pose that must be detected for the gesture to be performed.")]
        ScriptableObject m_HandShapeOrPose;

        [SerializeField]
        [Tooltip("The event fired when the gesture is performed.")]
        UnityEvent m_GesturePerformed;

        [SerializeField]
        [Tooltip("The event fired when the gesture is ended.")]
        UnityEvent m_GestureEnded;

        [SerializeField]
        [Tooltip("The minimum amount of time the hand must be held in the required shape and orientation for the gesture to be performed.")]
        float m_MinimumHoldTime = 0.2f;

        [SerializeField]
        [Tooltip("The interval at which the gesture detection is performed.")]
        float m_GestureDetectionInterval = 0.1f;

        [SerializeField]
        [Tooltip("The image displayed when this gesture is performed.")]
        Image m_Highlight;

        XRHandShape m_HandShape;
        XRHandPose m_HandPose;
        bool m_WasDetected;
        bool m_PerformedTriggered;
        float m_TimeOfLastConditionCheck;
        float m_HoldStartTime;

        /// <summary>
        /// The hand tracking events component to subscribe to receive updated joint data to be used for gesture detection.
        /// </summary>
        public XRHandTrackingEvents handTrackingEvents
        {
            get => m_HandTrackingEvents;
            set => m_HandTrackingEvents = value;
        }

        /// <summary>
        /// The hand shape or pose that must be detected for the gesture to be performed.
        /// </summary>
        public ScriptableObject handShapeOrPose
        {
            get => m_HandShapeOrPose;
            set => m_HandShapeOrPose = value;
        }

        /// <summary>
        /// The event fired when the gesture is performed.
        /// </summary>
        public UnityEvent gesturePerformed
        {
            get => m_GesturePerformed;
            set => m_GesturePerformed = value;
        }

        /// <summary>
        /// The event fired when the gesture is ended.
        /// </summary>
        public UnityEvent gestureEnded
        {
            get => m_GestureEnded;
            set => m_GestureEnded = value;
        }

        /// <summary>
        /// The minimum amount of time the hand must be held in the required shape and orientation for the gesture to be performed.
        /// </summary>
        public float minimumHoldTime
        {
            get => m_MinimumHoldTime;
            set => m_MinimumHoldTime = value;
        }

        /// <summary>
        /// The interval at which the gesture detection is performed.
        /// </summary>
        public float gestureDetectionInterval
        {
            get => m_GestureDetectionInterval;
            set => m_GestureDetectionInterval = value;
        }

        /// <summary>
        /// The image component that draws the highlight state visuals for gesture icons.
        /// </summary>
        public Image highlight
        {
            get => m_Highlight;
            set => m_Highlight = value;
        }

        /// <summary>
        ///  Show or hide any highlight-state related visual UI elements
        /// </summary>
        public bool highlighted
        {
            set
            {
                m_Highlight.enabled = value;
            }
        }

        void Awake()
        {
            if (m_HandTrackingEvents == null)
            {
                m_HandTrackingEvents = GameObject.FindGameObjectWithTag("RGestures").GetComponent<XRHandTrackingEvents>();
            }
        }

        void OnEnable()
        {
            m_HandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);

            m_HandShape = m_HandShapeOrPose as XRHandShape;
            m_HandPose = m_HandShapeOrPose as XRHandPose;
        }

        void OnDisable() => m_HandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

        void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
        {
            if (!isActiveAndEnabled || Time.timeSinceLevelLoad < m_TimeOfLastConditionCheck + m_GestureDetectionInterval)
                return;

            var detected =
                m_HandTrackingEvents.handIsTracked &&
                m_HandShape != null && m_HandShape.CheckConditions(eventArgs) ||
                m_HandPose != null && m_HandPose.CheckConditions(eventArgs);

            if (!m_WasDetected && detected)
            {
                m_HoldStartTime = Time.timeSinceLevelLoad;
            }
            else if (m_WasDetected && !detected)
            {
                m_PerformedTriggered = false;
                m_GestureEnded?.Invoke();
            }

            m_WasDetected = detected;

            if (!m_PerformedTriggered && detected)
            {
                var holdTimer = Time.timeSinceLevelLoad - m_HoldStartTime;
                if (holdTimer > m_MinimumHoldTime)
                {
                    m_GesturePerformed?.Invoke();
                    m_PerformedTriggered = true;
                    m_Highlight.enabled = true;
                }
            }

            m_TimeOfLastConditionCheck = Time.timeSinceLevelLoad;
        }
    }
}

using UnityEngine;

namespace KartGame.KartSystems
{
    /// <summary>
    /// A basic gamepad implementation of the IInput interface for all the input information a kart needs.
    /// </summary>
    public class CameraInput : MonoBehaviour, IInput
    {
        public CvSystem cvSystem;
        public float Acceleration
        {
            get { return m_Acceleration; }
        }
        public float Steering
        {
            get { return m_Steering; }
        }
        public bool BoostPressed
        {
            get { return m_BoostPressed; }
        }
        public bool FirePressed
        {
            get { return m_FirePressed; }
        }
        public bool HopPressed
        {
            get { return m_HopPressed; }
        }
        public bool HopHeld
        {
            get { return m_HopHeld; }
        }

        float m_Acceleration;
        float m_Steering;
        bool m_HopPressed;
        bool m_HopHeld;
        bool m_BoostPressed;
        bool m_FirePressed;

        bool m_FixedUpdateHappened;

        void Update ()
        {
            if (cvSystem.Brake)
                m_Acceleration = -1f;
            else if (cvSystem.Accelerate)
                m_Acceleration = 1f;
            else
                m_Acceleration = 0f;

            m_Steering = cvSystem.SteeringAngle;

            m_HopHeld = cvSystem.HopHeld;

            if (m_FixedUpdateHappened)
            {
                m_FixedUpdateHappened = false;

                m_HopPressed = false;
                m_BoostPressed = false;
                m_FirePressed = false;
            }

            m_HopPressed |= cvSystem.HopPressed;
            m_BoostPressed |= cvSystem.Boost;
            m_FirePressed |= cvSystem.Fire;
        }

        void FixedUpdate ()
        {
            m_FixedUpdateHappened = true;
        }
    }
}
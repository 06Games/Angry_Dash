using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
    public class FPSCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;
        const string display = "{0} FPS";
        private Text m_Text;

        public bool TextMode = true;

        private void Start()
        {
            if (string.IsNullOrEmpty(ConfigAPI.GetString("FPS.show")))
                ConfigAPI.SetBool("FPS.show", true);

            if (TextMode)
            {
                m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
                m_Text = GetComponent<Text>();
                m_Text.text = "";
            }
            else GetComponent<Toggle>().isOn = ConfigAPI.GetBool("FPS.show");
        }


        private void Update()
        {
            if (TextMode)
            {
                // measure average frames per second
                m_FpsAccumulator++;
                if (Time.realtimeSinceStartup > m_FpsNextPeriod)
                {
                    m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                    m_FpsAccumulator = 0;
                    m_FpsNextPeriod += fpsMeasurePeriod;

                    if (ConfigAPI.GetBool("FPS.show"))
                        m_Text.text = string.Format(display, m_CurrentFps);
                    else m_Text.text = "";
                }
            }
            else ConfigAPI.SetBool("FPS.show", GetComponent<Toggle>().isOn);
        }
    }
}

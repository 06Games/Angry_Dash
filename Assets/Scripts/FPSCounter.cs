﻿using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
    public class FPSCounter : MonoBehaviour
    {
        private const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator;
        private float m_FpsNextPeriod;
        private int m_CurrentFps;
        private const string display = "{0} FPS";
        private Text m_Text;

        public bool TextMode = true;

        private void Start()
        {
            if (string.IsNullOrEmpty(ConfigAPI.GetString("video.showFPS")))
                ConfigAPI.SetBool("video.showFPS", true);

            if (TextMode)
            {
                m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
                m_Text = GetComponent<Text>();
                m_Text.text = "";
            }
            else GetComponent<Toggle>().isOn = ConfigAPI.GetBool("video.showFPS");
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

                    m_Text.text = string.Format(display, m_CurrentFps);

                    GetComponent<Text>().enabled = ConfigAPI.GetBool("video.showFPS");
                }
            }
            else ConfigAPI.SetBool("video.showFPS", GetComponent<Toggle>().isOn);
        }
    }
}

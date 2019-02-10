﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class MockupCanvas : MonoBehaviour
{
    public Image mockupImage;
    const float proColor = 40 / 255f;
    const float personalColor = 49 / 255f;
    public void SetMockupSprite(Sprite sprite, ScreenOrientation orientation, bool simulate, bool flipped)
    {
        if (!simulate)
        {
            mockupImage.enabled = false;
            Destroy(gameObject);
        }
        else
        {
            mockupImage.enabled = true;

#if UNITY_EDITOR
            if (EditorGUIUtility.isProSkin)
            {
                mockupImage.color = new Color(proColor, proColor, proColor, 1);
            }
            else
            {
                mockupImage.color = new Color(personalColor, personalColor, personalColor, 1);
            }
#endif

            mockupImage.sprite = sprite;
            mockupImage.transform.localScale = new Vector3(
                flipped ? -1 : 1,
                flipped ? -1 : 1,
                1
            );

            //Force editor to refresh
            mockupImage.enabled = false;
            mockupImage.enabled = true;
        }
    }

    public void OnEnable()
    {
#if !UNITY_EDITOR
        Destroy(gameObject);
#else
        string enableSimulationKey = E7.NotchSolution.NotchSolutionUtility.prefix + "enableSimulation";
        if (!EditorPrefs.GetBool(enableSimulationKey)) DestroyImmediate(gameObject);
#endif
    }
}

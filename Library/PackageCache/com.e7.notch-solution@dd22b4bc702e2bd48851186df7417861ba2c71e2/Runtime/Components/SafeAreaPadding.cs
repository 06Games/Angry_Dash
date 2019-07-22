﻿//#define DEBUG_NOTCH_SOLUTION

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace E7.NotchSolution
{
    /// <summary>
    /// Make the panel into full stretch and apply padding to the panel according to reported <see cref="Screen.safeArea">
    /// The <see cref="Screen.safeArea"> will be interpolated into top level <see cref="RectTransform">'s size.
    /// 
    /// It should be a direct child of top canvas, or deeper child of some similarly full stretch rect in order to look right,
    /// although in reality it just pad in the shape of <see cref="Screen.safeArea"> regardless of its rectangle size.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [HelpURL("https://github.com/5argon/NotchSolution/blob/master/.Documentation/Components/SafeAreaPadding.md")]
    public class SafeAreaPadding : UIBehaviour, ILayoutSelfController, INotchSimulatorTarget
    {
        private Rect GetTopLevelRect()
        {
            var topLevelCanvas = GetTopLevelCanvas();
            Vector2 topRectSize = topLevelCanvas.GetComponent<RectTransform>().sizeDelta;
            return new Rect(Vector2.zero, topRectSize);

            Canvas GetTopLevelCanvas()
            {
                var canvas = this.GetComponentInParent<Canvas>();
                var rootCanvas = canvas.rootCanvas;
                return rootCanvas;
            }
        }

#pragma warning disable 0649
        [SerializeField] SupportedOrientations orientationType;
        [SerializeField] PerEdgeEvaluationModes portraitOrDefaultPaddings;
        [SerializeField] PerEdgeEvaluationModes landscapePaddings;
        [SerializeField] [Range(0f, 1f)] float influence = 1;
#pragma warning restore 0649

        [System.NonSerialized]
        private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            DelayedUpdate();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }


#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            influence = 1;
            orientationType = SupportedOrientations.Single;
            portraitOrDefaultPaddings = new PerEdgeEvaluationModes();
            landscapePaddings = new PerEdgeEvaluationModes();
        }
        
        protected override void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                DelayedUpdate();
            }
        }
#endif

        //INotchSimulatorTarget
        public void SimulatorUpdate(Rect simulatedSafeArea, Rect[] simulatedCutouts)
        {
            UpdateRect();
        }

        //ILayoutController
        public void SetLayoutHorizontal()
        {
            //Simulator is already calling SimulatorUpdate but this could be useful in some edge cases?
            UpdateRect();
        }

        //ILayoutController
        public void SetLayoutVertical()
        {
        }

        private void UpdateRect()
        {
            if (!IsActive()) return;

            PerEdgeEvaluationModes selectedOrientation =
            orientationType == SupportedOrientations.Dual ?
            NotchSolutionUtility.GetCurrentOrientation() == ScreenOrientation.Landscape ?
            landscapePaddings : portraitOrDefaultPaddings
            : portraitOrDefaultPaddings;

            m_Tracker.Clear();
            m_Tracker.Add(this, rectTransform,
                (LockSide(selectedOrientation.left) ? DrivenTransformProperties.AnchorMinX : 0) |
                (LockSide(selectedOrientation.right) ? DrivenTransformProperties.AnchorMaxX : 0) |
                (LockSide(selectedOrientation.bottom) ? DrivenTransformProperties.AnchorMinY : 0) |
                (LockSide(selectedOrientation.top) ? DrivenTransformProperties.AnchorMaxY : 0) |
                (LockSide(selectedOrientation.left) && LockSide(selectedOrientation.right) ? (DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.AnchoredPositionX) : 0) |
                (LockSide(selectedOrientation.top) && LockSide(selectedOrientation.bottom) ? (DrivenTransformProperties.SizeDeltaY | DrivenTransformProperties.AnchoredPositionY) : 0)
            );

            bool LockSide(SafeAreaEvaluationMode sapm)
            {
                switch (sapm)
                {
                    case SafeAreaEvaluationMode.Safe:
                    case SafeAreaEvaluationMode.SafeBalanced:
                    case SafeAreaEvaluationMode.Zero:
                        return true;
                    //When "Unlocked" is supported, it will be false.
                    default:
                        return false;
                }
            }

            //Lock the anchor mode to full stretch first.

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            var topRect = GetTopLevelRect();
            var safeAreaRelative = NotchSolutionUtility.SafeAreaRelative;

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"Top {topRect} safe {safeAreaRelative} min {safeAreaRelative.xMin} {safeAreaRelative.yMin}");
#endif

            var safeAreaPaddingsRelativeLDUR = new float[4]
            {
                safeAreaRelative.xMin,
                safeAreaRelative.yMin,
                1 - (safeAreaRelative.yMin + safeAreaRelative.height),
                1 - (safeAreaRelative.xMin + safeAreaRelative.width),
            };

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"SafeLDUR {string.Join(" ", safeAreaPaddingsRelativeLDUR.Select(x => x.ToString()))}");
#endif

            var currentRect = rectTransform.rect;

            //TODO : Calculate the current padding relative, to enable "Unlocked" mode. (Not forcing zero padding)
            var finalPaddingsLDUR = new float[4]
            {
                0,0,0,0
            };

            switch (selectedOrientation.left)
            {
                case SafeAreaEvaluationMode.Safe:
                    finalPaddingsLDUR[0] = topRect.width * safeAreaPaddingsRelativeLDUR[0];
                    break;
                case SafeAreaEvaluationMode.SafeBalanced:
                    finalPaddingsLDUR[0] = safeAreaPaddingsRelativeLDUR[3] > safeAreaPaddingsRelativeLDUR[0] ?
                        topRect.width * safeAreaPaddingsRelativeLDUR[3] :
                        topRect.width * safeAreaPaddingsRelativeLDUR[0];
                    break;
            }

            switch (selectedOrientation.right)
            {
                case SafeAreaEvaluationMode.Safe:
                    finalPaddingsLDUR[3] = topRect.width * safeAreaPaddingsRelativeLDUR[3];
                    break;
                case SafeAreaEvaluationMode.SafeBalanced:
                    finalPaddingsLDUR[3] = safeAreaPaddingsRelativeLDUR[0] > safeAreaPaddingsRelativeLDUR[3] ?
                        topRect.width * safeAreaPaddingsRelativeLDUR[0] :
                        topRect.width * safeAreaPaddingsRelativeLDUR[3];
                    break;
            }

            switch (selectedOrientation.bottom)
            {
                case SafeAreaEvaluationMode.Safe:
                    finalPaddingsLDUR[1] = topRect.height * safeAreaPaddingsRelativeLDUR[1];
                    break;
                case SafeAreaEvaluationMode.SafeBalanced:
                    finalPaddingsLDUR[1] = safeAreaPaddingsRelativeLDUR[2] > safeAreaPaddingsRelativeLDUR[1] ?
                        topRect.height * safeAreaPaddingsRelativeLDUR[2] :
                        topRect.height * safeAreaPaddingsRelativeLDUR[1];
                    break;
            }

            switch (selectedOrientation.top)
            {
                case SafeAreaEvaluationMode.Safe:
                    finalPaddingsLDUR[2] = topRect.height * safeAreaPaddingsRelativeLDUR[2];
                    break;
                case SafeAreaEvaluationMode.SafeBalanced:
                    finalPaddingsLDUR[2] = safeAreaPaddingsRelativeLDUR[1] > safeAreaPaddingsRelativeLDUR[2] ?
                        topRect.height * safeAreaPaddingsRelativeLDUR[1] :
                        topRect.height * safeAreaPaddingsRelativeLDUR[2];
                    break;
            }

            //Apply influence to the calculated padding
            finalPaddingsLDUR[0] *= influence;
            finalPaddingsLDUR[1] *= influence;
            finalPaddingsLDUR[2] *= influence;
            finalPaddingsLDUR[3] *= influence;

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"FinalLDUR {string.Join(" ", finalPaddingsLDUR.Select(x => x.ToString()))}");
#endif

            //Combined padding becomes size delta.
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = -(finalPaddingsLDUR[0] + finalPaddingsLDUR[3]);
            sizeDelta.y = -(finalPaddingsLDUR[1] + finalPaddingsLDUR[2]);
            rectTransform.sizeDelta = sizeDelta;

            //The rect remaining after subtracted the size delta.
            Vector2 rectWidthHeight = new Vector2(topRect.width + sizeDelta.x, topRect.height + sizeDelta.y);

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"RectWidthHeight {rectWidthHeight}");
#endif

            //Anchor position's answer is depending on pivot too. Where the pivot point is defines where 0 anchor point is.
            Vector2 zeroPosition = new Vector2(rectTransform.pivot.x * topRect.width, rectTransform.pivot.y * topRect.height);
            Vector2 pivotInRect = new Vector2(rectTransform.pivot.x * rectWidthHeight.x, rectTransform.pivot.y * rectWidthHeight.y);

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"zeroPosition {zeroPosition}");
#endif

            //Calculate like zero position is at bottom left first, then diff with the real zero position.
            rectTransform.anchoredPosition3D = new Vector3(
                finalPaddingsLDUR[0] + pivotInRect.x - zeroPosition.x,
                finalPaddingsLDUR[1] + pivotInRect.y - zeroPosition.y,
            rectTransform.anchoredPosition3D.z);
        }

        WaitForEndOfFrame eofWait = new WaitForEndOfFrame();

        private void DelayedUpdate() => StartCoroutine(DelayedUpdateRoutine());
        private IEnumerator DelayedUpdateRoutine()
        {
            yield return eofWait;
            UpdateRect();
        }
    }
}

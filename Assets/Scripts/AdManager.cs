using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdManager
{
    public class AdManager : MonoBehaviour
    {
        public static string UserId = "evan_g@orange.fr";
        public static string appKey = "6fb6333d";

        // Use this for initialization
        void Start()
        {
            /*Debug.Log("unity-script: MyAppStart Start called");

            IronSource.Agent.reportAppStarted();

            IronSourceConfig.Instance.setClientSideCallbacks(true);

            string id = IronSource.Agent.getAdvertiserId();
            Debug.Log("unity-script: IronSource.Agent.getAdvertiserId : " + id);

            Debug.Log("unity-script: IronSource.Agent.validateIntegration");
            IronSource.Agent.validateIntegration();

            Debug.Log("unity-script: unity version" + IronSource.unityVersion());

            Debug.Log("unity-script: IronSource.Agent.init");
            //IronSource.Agent.setUserId(uniqueUserId);
            IronSource.Agent.init(appKey);*/

            //For Rewarded Video
            IronSource.Agent.setUserId(UserId);
            IronSource.Agent.init(appKey);

            //IronSource.Agent.validateIntegration();
        }

        void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }
    }
    public class RewardAdd : MonoBehaviour
    {

        /*static int userTotalCredits = 0;
        public static System.String REWARDED_INSTANCE_ID = "0";

        void Start()
        {
            Debug.Log("unity-script: ShowRewardedVideoScript Start called");

            //Add Rewarded Video Events
            IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;

            //Add Rewarded Video DemandOnly Events
            IronSourceEvents.onRewardedVideoAdOpenedDemandOnlyEvent += RewardedVideoAdOpenedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdClosedDemandOnlyEvent += RewardedVideoAdClosedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedDemandOnlyEvent += RewardedVideoAvailabilityChangedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdRewardedDemandOnlyEvent += RewardedVideoAdRewardedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedDemandOnlyEvent += RewardedVideoAdShowFailedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdClickedDemandOnlyEvent += RewardedVideoAdClickedDemandOnlyEvent;
        }

        // RewardedVideo API //
        public static void ShowRewardedVideoButtonClicked()
        {
            Debug.Log("unity-script: ShowRewardedVideoButtonClicked");
            if (IronSource.Agent.isRewardedVideoAvailable())
            {
                IronSource.Agent.showRewardedVideo();
            }
            else
            {
                Debug.Log("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
            }

            // DemandOnly
            // ShowDemandOnlyRewardedVideo ();
        }

        static void ShowDemandOnlyRewardedVideo()
        {
            Debug.Log("unity-script: ShowDemandOnlyRewardedVideoButtonClicked");
            if (IronSource.Agent.isISDemandOnlyRewardedVideoAvailable(REWARDED_INSTANCE_ID))
            {
                IronSource.Agent.showISDemandOnlyRewardedVideo(REWARDED_INSTANCE_ID);
            }
            else
            {
                Debug.Log("unity-script: IronSource.Agent.isISDemandOnlyRewardedVideoAvailable - False");
            }
        }

        // RewardedVideo Delegates //
        static void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
        {
            Debug.Log("unity-script: I got RewardedVideoAvailabilityChangedEvent, value = " + canShowAd);
            if (canShowAd)
            {
                //ShowText.GetComponent<UnityEngine.UI.Text>().color = UnityEngine.Color.blue;
            }
            else
            {
                //ShowText.GetComponent<UnityEngine.UI.Text>().color = UnityEngine.Color.red;
            }
        }

        static void RewardedVideoAdOpenedEvent()
        {
            Debug.Log("unity-script: I got RewardedVideoAdOpenedEvent");
        }

        static void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
        {
            Debug.Log("unity-script: I got RewardedVideoAdRewardedEvent, amount = " + ssp.getRewardAmount() + " name = " + ssp.getRewardName());
            userTotalCredits = userTotalCredits + ssp.getRewardAmount();
            //AmountText.GetComponent<UnityEngine.UI.Text>().text = "" + userTotalCredits;
        }

        static void RewardedVideoAdClosedEvent()
        {
            Debug.Log("unity-script: I got RewardedVideoAdClosedEvent");
        }

        static void RewardedVideoAdStartedEvent()
        {
            Debug.Log("unity-script: I got RewardedVideoAdStartedEvent");
        }

        static void RewardedVideoAdEndedEvent()
        {
            Debug.Log("unity-script: I got RewardedVideoAdEndedEvent");
        }

        static void RewardedVideoAdShowFailedEvent(IronSourceError error)
        {
            Debug.Log("unity-script: I got RewardedVideoAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
        }

        static void RewardedVideoAdClickedEvent(IronSourcePlacement ssp)
        {
            Debug.Log("unity-script: I got RewardedVideoAdClickedEvent, name = " + ssp.getRewardName());
        }

        // RewardedVideo DemandOnly Delegates //

        static void RewardedVideoAvailabilityChangedDemandOnlyEvent(string instanceId, bool canShowAd)
        {
            Debug.Log("unity-script: I got RewardedVideoAvailabilityChangedDemandOnlyEvent for instance: " + instanceId + ", value = " + canShowAd);
            if (canShowAd)
            {
                //ShowText.GetComponent<UnityEngine.UI.Text>().color = UnityEngine.Color.blue;
            }
            else
            {
                //ShowText.GetComponent<UnityEngine.UI.Text>().color = UnityEngine.Color.red;
            }
        }

        static void RewardedVideoAdOpenedDemandOnlyEvent(string instanceId)
        {
            Debug.Log("unity-script: I got RewardedVideoAdOpenedDemandOnlyEvent for instance: " + instanceId);
        }

        static void RewardedVideoAdRewardedDemandOnlyEvent(string instanceId, IronSourcePlacement ssp)
        {
            Debug.Log("unity-script: I got RewardedVideoAdRewardedDemandOnlyEvent for instance: " + instanceId + ", amount = " + ssp.getRewardAmount() + " name = " + ssp.getRewardName());
            userTotalCredits = userTotalCredits + ssp.getRewardAmount();
            //AmountText.GetComponent<UnityEngine.UI.Text>().text = "" + userTotalCredits;
        }

        static void RewardedVideoAdClosedDemandOnlyEvent(string instanceId)
        {
            Debug.Log("unity-script: I got RewardedVideoAdClosedDemandOnlyEvent for instance: " + instanceId);
        }

        static void RewardedVideoAdStartedDemandOnlyEvent(string instanceId)
        {
            Debug.Log("unity-script: I got RewardedVideoAdStartedDemandOnlyEvent for instance: " + instanceId);
        }

        static void RewardedVideoAdEndedDemandOnlyEvent(string instanceId)
        {
            Debug.Log("unity-script: I got RewardedVideoAdEndedDemandOnlyEvent for instance: " + instanceId);
        }

        static void RewardedVideoAdShowFailedDemandOnlyEvent(string instanceId, IronSourceError error)
        {
            Debug.Log("unity-script: I got RewardedVideoAdShowFailedDemandOnlyEvent for instance: " + instanceId + ", code :  " + error.getCode() + ", description : " + error.getDescription());
        }

        static void RewardedVideoAdClickedDemandOnlyEvent(string instanceId, IronSourcePlacement ssp)
        {
            Debug.Log("unity-script: I got RewardedVideoAdClickedDemandOnlyEvent for instance: " + instanceId + ", name = " + ssp.getRewardName());
        }*/
    }
}

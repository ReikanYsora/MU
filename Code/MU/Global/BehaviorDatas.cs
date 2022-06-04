using MU.Managers;
using System.Collections.Generic;
using System.Linq;

namespace MU.Global
{
    public static class BehaviorDatas
    {
        public static List<MUManager> Managers { internal set; get; }
        public static Expression ActualExpression { internal set; get; }
        public static double LastDetectedDistance { internal set; get; }

        #region CALLBACKS
        public static void CBUltraSonicManager_OnDataCalculated(object sender, double data)
        {
            LastDetectedDistance = data;

            UltraSonicManager usm = Managers.Where(x => x is UltraSonicManager).SingleOrDefault() as UltraSonicManager;

            if (usm != null)
            {
                usm.AddDetectionTask();
            }
        }

        public static void CBSpeechReconManager_OnSpeechDetected(object sender, string data)
        {
            string text = data;
        }
        #endregion
    }
}

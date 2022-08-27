using UnityModManagerNet;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using PassengerJobsMod;
using DV.Logic.Job;


namespace Henry.Passenger_Train_Subsidization_Program
{
    public class PassengerTrainSubsidizationProgram
    {
        public enum ExtraBonusMode
        {
            Match,
            Fixed
        }
        public static bool ModState { get; private set; }
        public static UnityModManager.ModEntry Entry { get; private set; }
        public static Settings.Settings thisSetting;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.Logger.Log("PTSP Load Called");
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            thisSetting = UnityModManager.ModSettings.Load<Settings.Settings>(modEntry);
            Entry = modEntry;
            Entry.Logger.Log("PTSP Load Complete");
            return true;
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Entry.Logger.Log($"OnSaveGUI called on {modEntry.Info.DisplayName}");
            thisSetting.Save(modEntry);
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Entry.Logger.Log($"OnGUI called on {modEntry.Info.DisplayName}");
            thisSetting.Draw(modEntry);
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Entry.Logger.Log($"OnToggle called on {modEntry.Info.DisplayName} for {value}");
            ModState = value;
            if (value)
            {
                AddToJobComplete();
            }
            else
            {
                RemoveFromJobComplete();
            }
            return true;
        }

        public static bool IsPaxJobInUse()
        {
            Entry.Logger.Log("Looking for passenger job mod");
            UnityModManager.ModEntry PaxMod = UnityModManager.FindMod("PassengerJobs");
            if (PaxMod != null)
            {
                Entry.Logger.Log("Found it");
                return true;
            }
            else
            {
                Entry.Logger.Log("Did not locate it");
                return false;
            }

        }

        public static void AddToJobComplete()
        {
            Entry.Logger.Log("Adding to Job Complete");
            if (IsPaxJobInUse())
            {
                Entry.Logger.Log("Patching now");
                var harmony = new HarmonyLib.Harmony(Entry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Entry.Logger.Log("Not patching now");
                FoundNoPaxJob();
            }
        }

        public static void RemoveFromJobComplete()
        {
            Debug.Log("Removing From Job Complete");
            var harmony = new HarmonyLib.Harmony(Entry.Info.Id);
            harmony.UnpatchAll();
        }

        public static void FoundNoPaxJob()
        {
            Entry.Logger.Log("Pax job not in use, this mod will not activate");
        }

        public static float GetNewIncome(Job job)
        {
            float income = job.GetBasePaymentForTheJob() + job.GetBonusPaymentForTheJob();
            float newincome = IncomeModificator(thisSetting.BonusMode, income);

            Entry.Logger.Log($"you can get this much with the subsidization program: {newincome}");
            return newincome;
        }

        public static float IncomeModificator(ExtraBonusMode mode, float income)
        {
            Entry.Logger.Log($"Using {mode.GetType().Name} to give out subsidy with base income of: {income}");
            switch (mode)
            {
                case ExtraBonusMode.Match:
                    return income * (1f + thisSetting.MatchLevel);
                case ExtraBonusMode.Fixed:
                    return income + thisSetting.GiveLevel;
            }
            return income;
        }

        [HarmonyPatch(typeof(Job), "GetWageForTheJob")]
        public class JobGetWageForTheJobPatch
        {
            public static void Postfix(Job __instance, ref float __result)
            {
                Job job = __instance;
                Entry.Logger.Log($"Found job {__instance.ID}, its type code is: {__instance.jobType} | for refernce, passenger commuter job code is {PassJobType.Commuter}, passenger express job code is {PassJobType.Express}");
                if (job.jobType == PassJobType.Commuter | job.jobType == PassJobType.Express)
                {
                    __result = GetNewIncome(job);
                }
                Entry.Logger.Log("PTSP GetWageForTheJob Done");
            }
        }
    }
}

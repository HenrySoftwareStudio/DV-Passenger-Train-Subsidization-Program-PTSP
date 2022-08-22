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
        public static float MatchLevel{ get; private set; }
        public static float GiveLevel { get; private set; }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.Logger.Log("PTSP Load Called");
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            Entry = modEntry;
            Entry.Logger.Log("PTSP Load Complete");
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Entry.Logger.Log($"OnGUI called on {modEntry.Info.DisplayName}, but it is yet to be built");
            //To Be built
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
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

        static bool IsPaxJobInUse()
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

        static void AddToJobComplete()
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

        static void RemoveFromJobComplete()
        {
            Debug.Log("Removing From Job Complete");
            var harmony = new HarmonyLib.Harmony(Entry.Info.Id);
            harmony.UnpatchAll();
        }

        private static void FoundNoPaxJob()
        {
            Entry.Logger.Log("Pax job not in use, this mod will not activate");
        }

        static float GetNewIncome(Job job)
        {
            float income = job.GetBasePaymentForTheJob() + job.GetBonusPaymentForTheJob();
            float newincome = IncomeModificator(ExtraBonusMode.Match, income);

            Entry.Logger.Log($"you can get this much with the subsidization program: {newincome}");
            return newincome;
        }

        private static float IncomeModificator(ExtraBonusMode mode, float income)
        {
            Entry.Logger.Log($"Using {mode.GetType().Name} to give out subsidy with base income of: {income}");
            switch (mode)
            {
                case ExtraBonusMode.Match:
                    MatchLevel = 1f;
                    return income * (1f + MatchLevel);
                case ExtraBonusMode.Fixed:
                    GiveLevel = 1000f;
                    return income + GiveLevel;
            }
            return income;
        }

        [HarmonyPatch(typeof(Job), "GetWageForTheJob")]
        class JobGetWageForTheJobPatch
        {
            static void Postfix(Job __instance, ref float __result)
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

using UnityModManagerNet;
using UnityEngine;
using System;


namespace Henry.Passenger_Train_Subsidization_Program.Settings
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Matching Level - if Bonus Mode is set to match (new wages = original total * (1 + MatchLevel))")]
        public float MatchLevel = 1f;

        [Draw("Give Level - hand you money each time you complete a job (new wages = original total + Give Level)")]
        public float GiveLevel = 10000f;

        [Draw("Extra Bonus Mode - Give for giving out a fixed amount, Match for the bonus program to give based on based pay")]
        public PassengerTrainSubsidizationProgram.ExtraBonusMode BonusMode = PassengerTrainSubsidizationProgram.ExtraBonusMode.Match;

        public void OnChange()
        {
            Debug.LogWarning($"Update Aquired, will be in effect when payment is calculated again, new settings: Match Level = {MatchLevel};" +
                $" Give Level = {GiveLevel}; Bonus Mode = {Enum.GetName(typeof(PassengerTrainSubsidizationProgram.ExtraBonusMode), BonusMode)/*Displays Bonus Mode name*/}");
            PassengerTrainSubsidizationProgram.Entry.Logger.Log("Updated setting recevied");
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            PassengerTrainSubsidizationProgram.Entry.Logger.Log("Mod is saving its settings");
            Save(this, modEntry);
        }
    }
}

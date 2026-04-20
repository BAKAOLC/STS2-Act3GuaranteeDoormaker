using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace Act3GuaranteeDoormaker.Patches;

internal sealed class RunManagerGenerateRoomsDoormakerPatch : IPatchMethod
{
    public static string PatchId => "run_manager_generate_rooms_act3_doormaker";
    public static string Description => "Force Act 3 boss pool to include The Doormaker";

    public static ModPatchTarget[] GetTargets()
    {
        return [new ModPatchTarget(typeof(RunManager), nameof(RunManager.GenerateRooms))];
    }

    public static void Postfix(RunManager __instance)
    {
        var state = __instance.State;
        if (state == null || state.Acts.Count == 0)
            return;

        var act3 = state.Acts[^1];
        if (act3 is not Glory)
            return;

        var doormaker = ModelDb.Encounter<DoormakerBoss>();

        var hasDoubleBoss = state.AscensionLevel >= (int)AscensionLevel.DoubleBoss;
        if (!hasDoubleBoss)
        {
            act3.SetBossEncounter(doormaker);
            return;
        }

        if (act3.BossEncounter.Id == doormaker.Id
            || act3.SecondBossEncounter?.Id == doormaker.Id)
            return;

        act3.SetBossEncounter(doormaker);
    }
}
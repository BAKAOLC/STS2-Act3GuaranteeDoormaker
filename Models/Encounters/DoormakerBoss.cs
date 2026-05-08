using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using Act3GuaranteeDoormaker.Models.Afflictions;
using Act3GuaranteeDoormaker.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Act3GuaranteeDoormaker.Models.Encounters;

/// <summary>
/// Restored in-mod implementation of the removed 0.104 Doormaker boss encounter.
/// </summary>
[RegisterGlobalEncounter]
public sealed class DoormakerBoss : ModEncounterTemplate
{
    public override RoomType RoomType => RoomType.Boss;

    public override MegaSkeletonDataResource? BossNodeSpineResource => null;

    public override string BossNodePath => "res://Act3GuaranteeDoormaker/images/map/placeholder/doormaker_boss_icon";

    public override EncounterAssetProfile AssetProfile => new(
        RunHistoryIconPath: "res://Act3GuaranteeDoormaker/images/ui/run_history/doormaker_boss.png",
        RunHistoryIconOutlinePath: "res://Act3GuaranteeDoormaker/images/ui/run_history/doormaker_boss_outline.png");

    public override string CustomBgm => "event:/music/act3_boss_queen";

    protected override bool HasCustomBackground => false;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Doormaker>()];

    public override IEnumerable<string> ExtraAssetPaths => [ModelDb.Affliction<Devoured>().OverlayPath];

    public override float GetCameraScaling() => 0.9f;

    public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [(ModelDb.Monster<Doormaker>().ToMutable(), null)];
}


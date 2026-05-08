using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Act3GuaranteeDoormaker.Models.Powers;

/// <summary>
/// Restored for Doormaker phase logic (0.104 compatibility).
/// </summary>
[RegisterPower]
public sealed class ScrutinyPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Act3GuaranteeDoormaker/images/powers/scrutiny_power.png",
        BigIconPath: "res://Act3GuaranteeDoormaker/images/powers/scrutiny_power.png");

    public override bool ShouldDraw(Player player, bool fromHandDraw)
    {
        if (fromHandDraw)
            return true;

        Flash();
        return false;
    }
}


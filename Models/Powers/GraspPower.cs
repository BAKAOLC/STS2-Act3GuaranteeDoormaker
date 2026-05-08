using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Act3GuaranteeDoormaker.Models.Afflictions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Act3GuaranteeDoormaker.Models.Powers;

/// <summary>
/// Restored for Doormaker phase logic (0.104 compatibility).
/// In 0.105 the original Weighted affliction interaction was removed upstream,
/// so this power currently functions as a phase marker only.
/// </summary>
[RegisterPower]
public sealed class GraspPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Act3GuaranteeDoormaker/images/powers/grasp_power.png",
        BigIconPath: "res://Act3GuaranteeDoormaker/images/powers/grasp_power.png");

    protected override bool IncludeEnergyHoverTip => true;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var combatState = Owner.CombatState;
        if (combatState == null)
            return;

        foreach (Creature item in combatState.Allies.ToList())
        {
            if (!item.IsPlayer)
                continue;

            var player = item.Player;
            if (player == null)
                continue;

            var playerCombatState = player.PlayerCombatState;
            if (playerCombatState == null)
                continue;

            var list = playerCombatState.AllCards.ToList();
            foreach (var card in list)
                await Afflict(card);
        }
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Affliction != null)
            return;

        await Afflict(card);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (oldOwner.CombatState == null)
            return Task.CompletedTask;

        foreach (Creature item in oldOwner.CombatState.Allies.ToList())
        {
            if (!item.IsPlayer)
                continue;

            var player = item.Player;
            if (player == null)
                continue;

            var playerCombatState = player.PlayerCombatState;
            if (playerCombatState == null)
                continue;

            var list = playerCombatState.AllCards.Where(static c => c.Affliction is Weighted).ToList();
            foreach (var card in list)
                CardCmd.ClearAffliction(card);
        }

        return Task.CompletedTask;
    }

    private async Task Afflict(CardModel card)
    {
        if (card.Affliction != null)
            return;

        await CardCmd.Afflict<Weighted>(card, Amount);
    }
}


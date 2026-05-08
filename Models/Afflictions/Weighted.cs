using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Act3GuaranteeDoormaker.Models.Afflictions;

[RegisterAffliction]
public sealed class Weighted : ModAfflictionTemplate
{
    public override bool HasExtraCardText => true;

    public override AfflictionAssetProfile AssetProfile => new(
        OverlayScenePath:
        "res://Act3GuaranteeDoormaker/scenes/cards/overlays/afflictions/act3_guarantee_doormaker_affliction_weighted.tscn");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.ForEnergy(Card)];

    public override async Task OnPlay(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PlayerCmd.LoseEnergy(Amount, Card.Owner);
    }
}


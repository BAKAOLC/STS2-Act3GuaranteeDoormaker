using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Act3GuaranteeDoormaker.Models.Afflictions;

[RegisterAffliction]
public sealed class Devoured : ModAfflictionTemplate
{
    private bool _appliedExhaust;

    public bool AppliedExhaust
    {
        get => _appliedExhaust;
        set
        {
            AssertMutable();
            _appliedExhaust = value;
        }
    }

    public override AfflictionAssetProfile AssetProfile => new(
        OverlayScenePath:
        "res://Act3GuaranteeDoormaker/scenes/cards/overlays/afflictions/act3_guarantee_doormaker_affliction_devoured.tscn");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public override bool CanAfflictCardType(CardType cardType)
    {
        return (uint)(cardType - 1) <= 1u;
    }
}


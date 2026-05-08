using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Act3GuaranteeDoormaker.Models.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Act3GuaranteeDoormaker.Models.Monsters;

/// <summary>
/// Restored in-mod implementation of the removed 0.104 Doormaker monster.
/// </summary>
[RegisterMonster]
public sealed class Doormaker : ModMonsterTemplate
{
    private const string DoormakerTrackName = "queen_progress";

    private const string Phase1Visual = "res://Act3GuaranteeDoormaker/images/monsters/beta/door_maker_placeholder_2.png";
    private const string Phase2Visual = "res://Act3GuaranteeDoormaker/images/monsters/beta/door_maker_placeholder_3.png";
    private const string Phase3Visual = "res://Act3GuaranteeDoormaker/images/monsters/beta/door_maker_placeholder_4.png";

    private int _originalHp;
    private bool _isPortalOpen;
    public override MonsterAssetProfile AssetProfile =>
        new("res://Act3GuaranteeDoormaker/scenes/creature_visuals/doormaker.tscn");

    public override LocString Title =>
        IsPortalOpen
            ? MonsterModel.L10NMonsterLookup($"{Id.Entry}.name")
            : MonsterModel.L10NMonsterLookup("ACT3_GUARANTEE_DOORMAKER_DOOR.name");

    private bool IsPortalOpen
    {
        get => _isPortalOpen;
        set
        {
            AssertMutable();
            _isPortalOpen = value;
        }
    }

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 512, 489);

    public override int MaxInitialHp => MinInitialHp;

    private int HungerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 35, 30);

    private int ScrutinyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 24);

    private int GraspDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

    private int GraspStrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

    private int OriginalHp
    {
        get => _originalHp;
        set
        {
            AssertMutable();
            _originalHp = value;
        }
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OriginalHp = Creature.MaxHp;
        await CreatureCmd.SetMaxAndCurrentHp(Creature, 999999999m);
        Creature.HpDisplay = HpDisplay.InfiniteWithoutNumbers;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState dramaticOpen = new("DRAMATIC_OPEN_MOVE", DramaticOpenMove, new SummonIntent());
        MoveState hunger = new("HUNGER_MOVE", HungerMove, new SingleAttackIntent(HungerDamage));
        MoveState scrutiny = new("SCRUTINY_MOVE", ScrutinyMove, new SingleAttackIntent(ScrutinyDamage));
        MoveState grasp = new("GRASP_MOVE", GraspMove, new MultiAttackIntent(GraspDamage, 2), new BuffIntent());

        dramaticOpen.FollowUpState = hunger;
        hunger.FollowUpState = scrutiny;
        scrutiny.FollowUpState = grasp;
        grasp.FollowUpState = hunger;

        return new MonsterMoveStateMachine([dramaticOpen, hunger, scrutiny, grasp], dramaticOpen);
    }

    private async Task DramaticOpenMove(IReadOnlyList<Creature> targets)
    {
        IsPortalOpen = true;
        await CreatureCmd.SetMaxAndCurrentHp(Creature, OriginalHp);

        foreach (PowerModel power in Creature.Powers.ToList())
            await PowerCmd.Remove(power);

        Creature.HpDisplay = HpDisplay.Normal;

        await SwapPhasePower<HungerPower>();
        UpdateVisual(Phase2Visual);

        await Cmd.CustomScaledWait(0.2f, 0.6f);
        TalkCmd.Play(MonsterModel.L10NMonsterLookup($"{Id.Entry}.moves.DRAMATIC_OPEN_MOVE.speakLine"), Creature,
            VfxColor.Purple);
        await Cmd.CustomScaledWait(0.2f, 0.6f);
        NRunMusicController.Instance?.UpdateMusicParameter(DoormakerTrackName, 1f);
    }

    private async Task HungerMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(HungerDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);

        await SwapPhasePower<ScrutinyPower>();
        await Cmd.Wait(0.2f);
        UpdateVisual(Phase1Visual);
    }

    private async Task ScrutinyMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ScrutinyDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithHitFx("vfx/vfx_bite")
            .Execute(null);

        await SwapPhasePower<GraspPower>();
        await Cmd.Wait(0.2f);
        UpdateVisual(Phase3Visual);
    }

    private async Task GraspMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(GraspDamage)
            .WithHitCount(2)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);

        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, GraspStrengthGain, Creature, null);
        await SwapPhasePower<HungerPower>();
        await Cmd.Wait(0.2f);
        UpdateVisual(Phase2Visual);
    }

    private async Task SwapPhasePower<T>() where T : PowerModel
    {
        if (Creature.HasPower<HungerPower>())
            await PowerCmd.Remove<HungerPower>(Creature);
        if (Creature.HasPower<ScrutinyPower>())
            await PowerCmd.Remove<ScrutinyPower>(Creature);
        if (Creature.HasPower<GraspPower>())
            await PowerCmd.Remove<GraspPower>(Creature);

        await PowerCmd.Apply<T>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    private void UpdateVisual(string path)
    {
        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node == null)
            return;

        if (node.Visuals.GetCurrentBody() is not Sprite2D sprite)
            return;

        sprite.Texture = ResourceLoader.Load<Texture2D>(path);

        Vector2 scale = sprite.Scale;
        Tween tween = node.CreateTween();
        tween.TweenProperty(sprite, "scale", scale, 1.2f).From(scale * 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        tween.Parallel().TweenProperty(sprite, "modulate", Colors.White, 0.5).From(Colors.Black);
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Creature)
            return Task.CompletedTask;

        NRunMusicController.Instance?.UpdateMusicParameter(DoormakerTrackName, 5f);
        return Task.CompletedTask;
    }
}


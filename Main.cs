using Act3GuaranteeDoormaker.Patches;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Patching.Core;

namespace Act3GuaranteeDoormaker;

[ModInitializer(nameof(Initialize))]
public static class Main
{
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(Const.ModId);

    public static bool IsModActive { get; private set; }

    public static void Initialize()
    {
        Logger.Info($"Mod ID: {Const.ModId}");
        Logger.Info($"Version: {Const.Version}");

        try
        {
            var patcher = RitsuLibFramework.CreatePatcher(Const.ModId, "main");
            patcher.RegisterPatch<RunManagerGenerateRoomsDoormakerPatch>();

            if (!RitsuLibFramework.ApplyRequiredPatcher(patcher, () => IsModActive = false))
            {
                Logger.Error("补丁应用失败，模组未启用。");
                return;
            }

            IsModActive = true;
            Logger.Info("模组已启用：第三幕首领固定包含铸门者（Doormaker）。");
        }
        catch (Exception ex)
        {
            Logger.Error($"初始化异常: {ex.Message}");
            Logger.Error(ex.StackTrace ?? string.Empty);
            IsModActive = false;
        }
    }
}
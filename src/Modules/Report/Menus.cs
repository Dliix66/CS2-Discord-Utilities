using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Menu;

namespace Report
{
    public partial class Report
    {
        public static Report Instance { get; private set; } = new Report();
        public void OpenReportMenu_Players(CCSPlayerController player)
        {
            if (GetTargetsForReportCount(player) == 0)
            {
                player.PrintToChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.ReportNoTargetsFound"]}");
                return;
            }

            CenterHtmlMenu Menu = new CenterHtmlMenu($"{Localizer["Menu.ReportSelectPlayer"]}");

            //foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.SteamID.ToString().Length == 17))
            foreach (var p in Utilities.GetPlayers().Where(p => p != null && p.IsValid && p != player && DiscordUtilities!.IsPlayerDataLoaded(p) && p.Connected == PlayerConnectedState.PlayerConnected && p.SteamID.ToString().Length == 17 && !AdminManager.PlayerHasPermissions(p, Config.UnreportableFlag)))
                Menu.AddMenuOption(p.PlayerName, (player, target) => OnSelectPlayer_ReportMenu(player, p));

            MenuManager.OpenCenterHtmlMenu(Instance, player, Menu);
        }

        public void OpenReportMenu_Reason(CCSPlayerController player, CCSPlayerController target)
        {
            var selectedTarget = target;
            string[] Reasons = Config.ReportReasons.Split(',');
            var Menu = new CenterHtmlMenu($"{Localizer["Menu.ReportSelectReason"]}");
            foreach (var reason in Reasons)
            {
                if (reason.Contains("#CUSTOMREASON"))
                    Menu.AddMenuOption($"{Localizer["Menu.ReportCustomReason"]}", (player, target) => CustomReasonReport(player, selectedTarget));
                else
                    Menu.AddMenuOption(reason, (player, target) => SendReport(player, selectedTarget, reason));
            }
            MenuManager.OpenCenterHtmlMenu(Instance, player, Menu);
            Menu.PostSelectAction = PostSelectAction.Close;
        }

        private void OnSelectPlayer_ReportMenu(CCSPlayerController player, CCSPlayerController target)
        {
            OpenReportMenu_Reason(player, target);
        }
    }
}
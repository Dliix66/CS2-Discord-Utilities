using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace DiscordUtilities
{
    public partial class DiscordUtilities
    {
        [GameEventHandler(HookMode.Post)]
        public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player != null && player.IsValid && player.AuthorizedSteamID != null && !playerData.ContainsKey(player))
            {
                PlayerData newPlayer = new PlayerData
                {
                    Name = player.PlayerName,
                    NameWithoutEmoji = RemoveEmoji(player.PlayerName),
                    SteamId32 = player.AuthorizedSteamID.SteamId32.ToString(),
                    SteamId64 = player.AuthorizedSteamID.SteamId64.ToString(),
                    IpAddress = player.IpAddress != null ? player.IpAddress.ToString() : "Invalid",
                    CommunityUrl = player.AuthorizedSteamID.ToCommunityUrl().ToString(),
                    /*TeamShortName = GetTeamShortName(player.TeamNum),
                    TeamLongName = GetTeamLongName(player.TeamNum),
                    TeamNumber = player.TeamNum.ToString(),
                    Kills = 0.ToString(),
                    Deaths = 0.ToString(),
                    Assists = 0.ToString(),
                    Points = 0.ToString(),*/
                    CountryShort = "Undefined",
                    CountryLong = "Undefined",
                    CountryEmoji = ":flag_white:",
                    DiscordGlobalname = "",
                    DiscordDisplayName = "",
                    DiscordPing = "",
                    DiscordAvatar = "",
                    DiscordID = "",
                    IsLinked = false,
                };
                playerData.Add(player, newPlayer);
                PlayerDataLoaded(player);
                if (IsDbConnected && Config.Link.Enabled)
                {
                    if (linkedPlayers.ContainsKey(player.AuthorizedSteamID.SteamId64))
                    {
                        if (playerData.ContainsKey(player))
                            playerData[player].IsLinked = true;

                        _ = LoadPlayerData(player.AuthorizedSteamID.SteamId64.ToString(), linkedPlayers[player.AuthorizedSteamID.SteamId64]);
                    }
                }

                string IpAddress = player.IpAddress!.Split(":")[0];
                LoadPlayerCountry(IpAddress, player.AuthorizedSteamID.SteamId64);
            }
            return HookResult.Continue;
        }

        [GameEventHandler(HookMode.Post)]
        public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            var player = @event.Userid;

            if (player != null && player.IsValid && playerData.ContainsKey(player))
                playerData.Remove(player);

            return HookResult.Continue;
        }
    }
}
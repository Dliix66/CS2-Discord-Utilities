using CounterStrikeSharp.API;
using Discord;
using CounterStrikeSharp.API.Modules.Cvars;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;

namespace DiscordUtilities
{
    public partial class DiscordUtilities
    {
        private async Task LoadPlayerData(string steamid, ulong discordID)
        {
            var guild = BotClient!.GetGuild(ulong.Parse(ServerId));
            if (guild == null)
            {
                Perform_SendConsoleMessage($"[Discord Utilities] Guild with id '{ServerId}' was not found!", ConsoleColor.Red);
                return;
            }
            var user = guild.GetUser(discordID);
            if (user == null)
            {
                await RemovePlayerData(steamid);
                Perform_SendConsoleMessage($"[Discord Utilities] User with ID '{discordID}' was not found! Players has been removed from the Linked players.", ConsoleColor.DarkYellow);
                return;
            }

            await PerformLinkRole(discordID.ToString());
            LoadPlayerDiscordData(ulong.Parse(steamid), discordID);

            Server.NextFrame(() =>
            {
                PerformLinkPermission(ulong.Parse(steamid));
            });
        }

        private void LoadPlayerDiscordData(ulong steamid, ulong discordid)
        {
            var guild = BotClient!.GetGuild(ulong.Parse(ServerId));
            if (guild == null)
            {
                Perform_SendConsoleMessage($"[Discord Utilities] Guild with id '{ServerId}' was not found!", ConsoleColor.Red);
                return;
            }
            var user = guild.GetUser(discordid);
            if (user == null)
                return;

            var target = GetTargetBySteamID64(steamid);
            if (target != null)
            {
                var p = playerData[target];
                if (p != null)
                {
                    p.DiscordGlobalname = user.GlobalName;
                    p.DiscordDisplayName = user.DisplayName;
                    p.DiscordID = user.Id.ToString();
                    p.DiscordPing = $"<@{user.Id}>";
                    p.DiscordAvatar = user.GetAvatarUrl();
                }
                LinkedUserLoaded(user, target);
            }
        }
        private void LoadPlayerCountry(string IpAddress, ulong steamid)
        {
            try
            {
                using (var reader = new DatabaseReader(ModuleDirectory + "/GeoLite2-Country.mmdb"))
                {
                    var response = reader.Country(IpAddress);
                    string[] country = new string[2];
                    country[0] = response.Country.Name!;
                    country[1] = response.Country.IsoCode!;
                    UpdatePlayerCountry(steamid, country);
                }
            }
            catch (AddressNotFoundException ex)
            {
                Perform_SendConsoleMessage($"[Discord Utilities] IP Adress '{ex}' was not found.", ConsoleColor.Red);
            }
            catch (GeoIP2Exception ex)
            {
                Perform_SendConsoleMessage($"[Discord Utilities] An error occurred while searching IP adress: {ex.Message}.", ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                Perform_SendConsoleMessage($"[Discord Utilities] An error occurred while searching IP adress: {ex.Message}.", ConsoleColor.Red);
            }
        }

        private void UpdateServerData()
        {
            var timelimit = ConVar.Find("mp_timelimit")!.GetPrimitiveValue<float>() * 60;
            var gameStart = GameRules().GameStartTime;
            var currentTime = Server.CurrentTime;

            var timeleft = (int)timelimit - (int)(currentTime - gameStart);
            TimeSpan time = TimeSpan.FromSeconds(timeleft);

            serverData!.Timeleft = $"{time:mm\\:ss}";
            serverData.OnlinePlayers = GetPlayersCount().ToString();
            serverData.OnlinePlayersAndBots = GetPlayersCountWithBots().ToString();
            serverData.OnlineBots = GetBotsCounts().ToString();
        }
        private void UpdatePlayerCountry(ulong steamid, string[] country)
        {
            var target = GetTargetBySteamID64(steamid);
            if (target == null)
                return;

            var p = playerData[target];
            if (p != null)
            {
                if (!string.IsNullOrWhiteSpace(country[0]))
                    p.CountryLong = country[0];
                else
                    p.CountryLong = "Unknown";
                if (!string.IsNullOrWhiteSpace(country[1]))
                {
                    p.CountryShort = country[1].ToUpper();
                    string emoji = $":flag_{country[1].ToLower()}:";
                    p.CountryEmoji = IsValidFlag(emoji);
                }
                else
                {
                    p.CountryShort = "??";
                    p.CountryEmoji = ":flag_white:";
                }
            }
        }
        public async Task UpdateBotStatus()
        {
            if (BotClient == null || !IsBotConnected)
                return;

            int activityType = Config.BotStatus.ActivityType == 0 ? 1 : Config.BotStatus.ActivityType;
            string ActivityFormat = ReplaceServerDataVariables(Config.BotStatus.ActivityFormat);
            await BotClient.SetGameAsync(ActivityFormat, null, (ActivityType)activityType);
            await BotClient.SetStatusAsync((UserStatus)Config.BotStatus.Status);
        }
    }
}

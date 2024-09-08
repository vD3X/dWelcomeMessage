using CounterStrikeSharp.API; 
using CounterStrikeSharp.API.Core; 
using CounterStrikeSharp.API.Core.Attributes.Registration; 
using CounterStrikeSharp.API.Modules.Admin; 
using CounterStrikeSharp.API.Modules.Commands; 
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Core.Listeners;

namespace dWelcomeMessage; 
 
public class dWelcomeMessage : BasePlugin 
{ 
    public override string ModuleName => "[CS2] D3X - [ Welcome Message ]";
    public override string ModuleAuthor => "D3X";
    public override string ModuleDescription => "Plugin dodaje na serwer wiadomości powitalne dla graczy & vipów";
    public override string ModuleVersion => "1.0.0";

    public static dWelcomeMessage Instance { get; private set; }

    public override void Load(bool hotReload)
    {
        Instance = this;
        Config.Initialize();

        RegisterListener<OnClientPutInServer>(OnPlayerConnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    private void OnPlayerConnect(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return;

        var steamid = player.AuthorizedSteamID?.SteamId2 ?? "invalid steamid";
        var playerName = player.PlayerName;
        var group = GetPlayerGroup(player);
        var welcomeMessage = GetMessageForGroup(group, "Welcome_Message");

        if (!Config.config.Settings.Default_Message_Enabled && group == "Default") return;

        var formattedMessage = FormatMessage(welcomeMessage, playerName, steamid);
        if (!string.IsNullOrEmpty(formattedMessage))
        {
            Server.PrintToChatAll(formattedMessage);
        }
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;

        var steamid = player.AuthorizedSteamID?.SteamId2 ?? "invalid steamid";
        var playerName = player.PlayerName;
        var group = GetPlayerGroup(player);
        var goodbyeMessage = GetMessageForGroup(group, "Goodbye_Message");

        if (!Config.config.Settings.Default_Message_Enabled && group == "Default") return HookResult.Continue;

        var formattedMessage = FormatMessage(goodbyeMessage, playerName, steamid);
        if (!string.IsNullOrEmpty(formattedMessage))
        {
            Server.PrintToChatAll(formattedMessage);
        }

        return HookResult.Continue;
    }

    private string GetPlayerGroup(CCSPlayerController player)
    {
        foreach (var group in Config.config.Groups)
        {
            if (AdminManager.PlayerHasPermissions(player, group.Value.PermissionRequired))
            {
                return group.Key;
            }
        }
        return "Default";
    }

    private string GetMessageForGroup(string groupName, string messageType)
    {
        if (Config.config.Groups.TryGetValue(groupName, out var messages))
        {
            return messageType switch
            {
                "Welcome_Message" => messages.Welcome_Message,
                "Goodbye_Message" => messages.Goodbye_Message,
                _ => Config.config.Settings.Default_Welcome_Message
            };
        }

        return messageType switch
        {
            "Welcome_Message" => Config.config.Settings.Default_Welcome_Message,
            "Goodbye_Message" => Config.config.Settings.Default_Goodbye_Message,
            _ => string.Empty
        };
    }

    private string FormatMessage(string message, string playerName = "", string steamid = "")
    {
        var prefix = Config.config.Settings.Prefix;

        var colorMapping = new Dictionary<string, string>
        {
            { "DEFAULT", "\x01" },
            { "WHITE", "\x01" },
            { "DARKRED", "\x02" },
            { "GREEN", "\x04" },
            { "LIGHTYELLOW", "\x09" },
            { "LIGHTBLUE", "\x0B" },
            { "OLIVE", "\x05" },
            { "LIME", "\x06" },
            { "RED", "\x07" },
            { "LIGHTPURPLE", "\x03" },
            { "PURPLE", "\x0E" },
            { "GREY", "\x08" },
            { "YELLOW", "\x09" },
            { "GOLD", "\x10" },
            { "SILVER", "\x0A" },
            { "BLUE", "\x0B" },
            { "DARKBLUE", "\x0C" },
            { "BLUEGREY", "\x0A" },
            { "MAGENTA", "\x0E" },
            { "LIGHTRED", "\x0F" },
            { "ORANGE", "\x10" },
            { "PLAYERNAME", playerName },
            { "STEAMID", steamid }
        };

        foreach (var entry in colorMapping)
        {
            prefix = prefix.Replace($"{{{entry.Key}}}", entry.Value);
        }

        foreach (var entry in colorMapping)
        {
            message = message.Replace($"{{{entry.Key}}}", entry.Value);
        }

        return $"{prefix} {message}";
    }
} 

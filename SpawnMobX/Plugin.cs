using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SpawnMobX
{
  [ApiVersion(2, 1)]
  public class Plugin : TerrariaPlugin
  {
    public override string Name => "SpawnMobX";
    public override string Author => "Newy";
    public override string Description => "Spawn mobs with special properties!";
    public override Version Version => typeof(Plugin).Assembly.GetName().Version;

    public Plugin(Main game) : base(game)
    {
    }

    public override void Initialize()
    {
      Commands.ChatCommands.Add(new Command("spawnmobx.use", SpawnCommand, "spawnmobx", "smx"));
    }

    private const string _usage = "Invalid usage! Usage: /smx <mob name / id> <count> [parameter=value] ...";
    private const string _availableParameters = "Available parameters: {0}\n(/smx list to see details)";
    private const string _spawnSuccess = "Spawned {0} successfully.";
    private const string _spawnSuccessMultiple = "Spawned {0} instances of {1} successfully.";
    private const string _zeroNpcs = "Cannot spawn zero NPCs!";

    private static readonly Dictionary<string, string> _validParameters = new Dictionary<string, string>
    {
      ["name"] = "A nickname to give to the NPC. May not work client side.",

      ["health"] = "Maximum HP of the NPC. Limited to a certain multiple of the default HP.",
      ["regen"] = "HP regen rate of the NPC. May not work client side, or at all.",
      ["target"] = "ID of the player targeted by the NPC. May not work client side, or at all.",

      ["x"] = "The X tile coordinate (as reported by /pos) to spawn the NPC on.",
      ["y"] = "The Y tile coordinate (as reported by /pos) to spawn the NPC on.",

      ["wx"] = "The X world coordinate (tile coordinate * 16) to spawn the NPC on.",
      ["wy"] = "The Y world coordinate (tile coordinate * 16) to spawn the NPC on.",

      ["ai0"] = "AI parameter 0: a floating point number. May not work client side.",
      ["ai1"] = "AI parameter 1: a floating point number. May not work client side.",
      ["ai2"] = "AI parameter 2: a floating point number. May not work client side.",
      ["ai3"] = "AI parameter 3: a floating point number. May not work client side.",
    };

    private static void SpawnCommand(CommandArgs args)
    {
      if (args.Parameters.Count < 1)
      {
        args.Player.SendErrorMessage(_usage);
        args.Player.SendInfoMessage(_availableParameters, string.Join(", ", _validParameters.Keys));
        return;
      }

      if (args.Parameters[0].Equals("list", StringComparison.OrdinalIgnoreCase))
      {
        if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out var page))
          return;

        PaginationTools.SendPage(args.Player, page, _validParameters.Select(kv => kv.Key + ": " + kv.Value).ToArray(),
          new PaginationTools.Settings
          {
            HeaderFormat = "Available Parameters ({0}/{1})",
            FooterFormat = "Type /smx list {0} for more."
          });
        return;
      }

      NPC npcDefiniton;
      if (args.Parameters.Count == 1)
      {
        if (!TryFindOneNPC(args.Player, args.Parameters[0], out npcDefiniton))
          return;

        SpawnMob(npcDefiniton.type, (int) args.Player.X, (int) args.Player.Y);
        args.Player.SendSuccessMessage(_spawnSuccess, npcDefiniton.FullName);

        return;
      }

      if (!TryFindOneNPC(args.Player, args.Parameters[0], out npcDefiniton))
        return;

      args.Parameters.RemoveAt(0);

      if (int.TryParse(args.Parameters[0], out var count))
        args.Parameters.RemoveAt(0);
      else
        count = 1;

      if (count == 0)
      {
        args.Player.SendErrorMessage(_zeroNpcs);
        return;
      }

      if (args.Parameters.Count == 0)
      {
        for (var i = 0; i < count; i++)
          SpawnMob(npcDefiniton.type, (int) args.Player.X, (int) args.Player.Y);

        if (count == 1)
          args.Player.SendSuccessMessage(_spawnSuccess, npcDefiniton.FullName);
        else
          args.Player.SendSuccessMessage(_spawnSuccessMultiple, count, npcDefiniton.FullName);

        return;
      }

      var parameters = new Dictionary<string, string>();
      var data = args.Message.Remove(0, args.Message.IndexOf(args.Parameters[0], StringComparison.OrdinalIgnoreCase));

      foreach (var parameter in data.Split(' '))
      {
        var compound = parameter.Split('=');
        parameters.Add(compound[0], compound[1]);
      }

      for (var i = 0; i < count; i++)
        SpawnMobWithParameters(npcDefiniton.type, (int) args.Player.X, (int) args.Player.Y, parameters);

      if (count == 1)
        args.Player.SendSuccessMessage(_spawnSuccess, npcDefiniton.FullName);
      else
        args.Player.SendSuccessMessage(_spawnSuccessMultiple, count, npcDefiniton.FullName);
    }

    private static void SpawnMob(int type, int x, int y)
    {
      var npcIndex = NPC.NewNPC(x, y, type);
      var npc = Main.npc[npcIndex];
      TSPlayer.All.SendData(PacketTypes.NpcUpdate, npc.FullName, npcIndex);
    }

    private static void SpawnMobWithParameters(int type, int x, int y, IDictionary<string, string> parameters)
    {
      string givenName = null;
      float[] ai = {0f, 0f, 0f, 0f};

      var health = -1;
      var regen = -1;

      var target = 255;

      foreach (var parameter in parameters)
      {
        switch (parameter.Key.ToLowerInvariant())
        {
          case "name":
            givenName = parameter.Value;
            break;

          case "health":
          case "hp":
            health = int.Parse(parameter.Value);
            break;

          case "regen":
            regen = int.Parse(parameter.Value);
            break;

          case "target":
            target = int.Parse(parameter.Value);
            break;

          case "x":
            x = int.Parse(parameter.Value) * 16;
            break;

          case "y":
            y = int.Parse(parameter.Value) * 16;
            break;

          case "wx":
            x = int.Parse(parameter.Value);
            break;

          case "wy":
            y = int.Parse(parameter.Value);
            break;

          case "ai0":
            ai[0] = float.Parse(parameter.Value);
            break;

          case "ai1":
            ai[1] = float.Parse(parameter.Value);
            break;

          case "ai2":
            ai[2] = float.Parse(parameter.Value);
            break;

          case "ai3":
            ai[3] = float.Parse(parameter.Value);
            break;
        }
      }

      var npcIndex = NPC.NewNPC(x, y, type, 0, ai[0], ai[1], ai[2], ai[3], target);
      var npc = Main.npc[npcIndex];

      if (givenName != null)
        npc.GivenName = givenName;

      if (health != -1)
      {
        npc.life = health;
        npc.lifeMax = health;
      }

      if (regen != -1)
        npc.lifeRegen = regen;

      TSPlayer.All.SendData(PacketTypes.NpcUpdate, givenName ?? npc.FullName, npcIndex);
    }

    private static bool TryFindOneNPC(TSPlayer errorReceiver, string query, out NPC npc)
    {
      npc = null;
      var results = TShock.Utils.GetNPCByIdOrName(query);

      if (results.Count == 0)
      {
        errorReceiver.SendErrorMessage("NPC \"{0}\" not found!", query);
        return false;
      }

      if (results.Count > 1)
      {
        errorReceiver.SendMultipleMatchError(results.Select(n => n.FullName));
        return false;
      }

      npc = results[0];
      return true;
    }
  }
}
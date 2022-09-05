using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn.Utils;

namespace MoreDefenses.Services
{
    class TeamConfigManager
    {
        public static string ModLocation = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
        private static Dictionary<string, int> playerToTeam = new Dictionary<string, int>();

        public static int GetTeam(long playerId)
        {
            if (!playerToTeam.ContainsKey(playerId.ToString()))
            {
                playerToTeam[playerId.ToString()] = 0;
            }
            return playerToTeam[playerId.ToString()];
        }

        public static void SetTeam(long playerId, int team)
        {
            playerToTeam[playerId.ToString()] = team;
        }

        public static void LoadTeamFromJson()
        {
            var json = AssetUtils.LoadText($"{ModLocation}/Assets/TeamConfigs/teams.json");
            playerToTeam = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, int>>(json);
        }

        public static void WriteTeamsToJson()
        {
            var json = SimpleJson.SimpleJson.SerializeObject(playerToTeam);
            string path = Path.Combine(BepInEx.Paths.PluginPath, $"{ModLocation}/Assets/TeamConfigs/teams.json");
            File.WriteAllText(path, json);
        }
    }
}

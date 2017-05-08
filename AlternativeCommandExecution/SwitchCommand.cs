using System;
using System.IO;
using System.IO.Streams;
using System.Linq;
using AlternativeCommandExecution.SwitchCommand;
using TShockAPI;
using Terraria;
using Terraria.ID;

namespace AlternativeCommandExecution
{
	partial class Plugin
	{
		internal static SwitchCmdManager Scs;

		private static void SwitchCmd_OnPostInit()
		{
			Scs = new SwitchCmdManager(TShock.DB);
			Scs.UpdateSwitchCommands();
		}

		public static void OnHitSwitch(MemoryStream data, TSPlayer player)
		{
			var i = (int)data.ReadInt16();
			var j = (int)data.ReadInt16();

			if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
			{
				return;
			}

			var tile = Main.tile[i, j];

			if (tile.type == TileID.Lever)
			{
				if (tile.frameY == 0)
				{
					j++;
				}
				if (tile.frameX % 36 == 0)
				{
					i++;
				}
			}

			var info = SwitchCmdPlayerInfo.GetInfo(player);

			if (info.WaitingSelection)
			{
				info.Ss?.Invoke(i, j);
				info.WaitingSelection = false;
				info.Ss = null;
			}
			else
			{
				var sc = Scs.SwitchCmds.SingleOrDefault(s => s.X == i && s.Y == j);
				if (sc == null)
				{
					return;

				}
				if (!sc.TryUse(player))
				{
					player.SendErrorMessage("指令开关冷却中，无法使用。");
					return;
				}
				try
				{
					if (!sc.IgnorePermission)
						ShortCommand.ShortCommandUtil.HandleCommand(player, sc.Command);
					else
						ShortCommand.ShortCommandUtil.HandleCommandIgnorePermission(player, sc.Command);
				}
				catch (Exception e)
				{
					TShock.Log.ConsoleError("执行指令时出现异常，详细请看日志文件。");
					TShock.Log.Error(e.ToString());
				}
			}
		}
	}
}

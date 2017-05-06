using System;
using System.Collections.Generic;
using System.Linq;
using AlternativeCommandExecution.Extensions;
using TShockAPI;

namespace AlternativeCommandExecution.ShortCommand
{
	public static class ShortCommandUtil
	{
		private static readonly Type CommandsType = typeof(Commands);

		public static bool HandleCommand(TSPlayer player, string text)
		{
			var cmdText = text.Remove(0, 1);
			var cmdPrefix = text[0].ToString();
			var silent = cmdPrefix == Commands.SilentSpecifier;

			cmdPrefix = silent ? Commands.SilentSpecifier : Commands.Specifier;

			var index = -1;
			for (var i = 0; i < cmdText.Length; i++)
			{
				if (CommandsType.CallPrivateStaticMethod<bool>("IsWhiteSpace", cmdText[i]))
				{
					index = i;
					break;
				}
			}
			if (index == 0) // Space after the command specifier should not be supported
			{
				player.SendErrorMessage("指令无效；键入 {0}help 以获取可用指令。", Commands.Specifier);
				return true;
			}
			var cmdName = index < 0 ? cmdText.ToLower() : cmdText.Substring(0, index).ToLower();

			var args = index < 0 ?
				new List<string>() :
				CommandsType.CallPrivateStaticMethod<List<string>>("ParseParameters", cmdText.Substring(index));

			var sc = Plugin.ShortCommands.Where(x => x.HasName(cmdName)).ToList();

			if (!sc.Any())
				return false;

			foreach (var s in sc)
			{
				try
				{
					foreach (var c in s.Convert(new CommandExectionContext(player), args.ToArray()))
					{
						Commands.HandleCommand(player, cmdPrefix + c);
					}
				}
				catch (CommandArgumentException ex)
				{
					player.SendErrorMessage(ex.Message);
				}
			}

			return true;
		}
	}
}

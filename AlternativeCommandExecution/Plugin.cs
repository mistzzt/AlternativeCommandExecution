using System;
using System.Collections.Generic;
using AlternativeCommandExecution.ShortCommand;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace AlternativeCommandExecution
{
	[ApiVersion(2, 1)]
	public class Plugin : TerrariaPlugin
	{
		public override string Name => GetType().Namespace;

		public override string Author => "MistZZT";

		public override Version Version => GetType().Assembly.GetName().Version;

		public Plugin(Main game) : base(game) { }

		public Configuration Config;

		public override void Initialize()
		{
			void ReloadConfig()
			{
				Config = Configuration.Read();
				Config.Write();
				LoadShortCommands();
			}
			ReloadConfig();

			ServerApi.Hooks.ServerChat.Register(this, OnChat);

			TShockAPI.Hooks.GeneralHooks.ReloadEvent += args => ReloadConfig();
		}

		private void OnChat(ServerChatEventArgs args)
		{
			bool IsValidCmd(string commandText)
			{
				return (
							commandText.StartsWith(TShock.Config.CommandSpecifier) ||
							commandText.StartsWith(TShock.Config.CommandSilentSpecifier) ||
							commandText.StartsWith(Config.CommandSpecifier) ||
							commandText.StartsWith(Config.CommandSpecifier2)
						)
						&& !string.IsNullOrWhiteSpace(commandText.Substring(1));
			}

			if (args.Handled)
				return;

			var tsplr = TShock.Players[args.Who];
			if (tsplr == null)
			{
				args.Handled = true;
				return;
			}

			if (args.Text.Length > 500)
			{
				TShock.Utils.Kick(tsplr, "试图发送长聊天语句破坏服务器。", true);
				args.Handled = true;
				return;
			}

			var text = args.Text;

			// Terraria now has chat commands on the client side.
			// These commands remove the commands prefix (e.g. /me /playing) and send the command id instead
			// In order for us to keep legacy code we must reverse this and get the prefix using the command id
			foreach (var item in Terraria.UI.Chat.ChatManager.Commands._localizedCommands)
			{
				if (item.Value._name == args.CommandId._name)
				{
					if (!string.IsNullOrEmpty(text))
					{
						text = item.Key.Value + ' ' + text;
					}
					else
					{
						text = item.Key.Value;
					}
					break;
				}
			}

			if (!IsValidCmd(text))
			{
				return;
			}

			try
			{
				args.Handled = true;
				if (!ShortCommandUtil.HandleCommand(tsplr, text))
				{
					// This is required in case anyone makes HandleCommand return false again
					tsplr.SendErrorMessage("无法识别指令. 请联系管理员以寻求帮助.");
					//Log.ConsoleError("无法识别指令文本 '{0}' (玩家: {1}).", text, tsplr.Name);
				}
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError("执行指令时出现异常.");
				TShock.Log.Error(ex.ToString());
			}
		}

		private void LoadShortCommands()
		{
			var list = new List<ShortCommand.ShortCommand>();

			foreach (var item in Config.ShortCommands)
			{
				try
				{
					list.Add(ShortCommand.ShortCommand.Create(item.ArgumentDescription, item.CommandLines, item.Names));
				}
				catch (CommandParseException ex)
				{
					Console.WriteLine("加载简写指令时读取失败：{0}", ex);
				}
			}

			ShortCommands = list.ToArray();
		}

		internal static ShortCommand.ShortCommand[] ShortCommands;
	}
}

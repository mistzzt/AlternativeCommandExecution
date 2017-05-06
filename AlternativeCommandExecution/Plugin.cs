﻿using System;
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

		public static Configuration Config { get; private set; }

		public override void Initialize()
		{
			void ReloadConfig()
			{
				Config = Configuration.Read();
				Config.Write();
				LoadShortCommands();
			}
			ReloadConfig();

			ServerApi.Hooks.ServerChat.Register(this, OnChat, 1000);
			ServerApi.Hooks.ServerCommand.Register(this, OnServerCommand, 1000);

			TShockAPI.Hooks.GeneralHooks.ReloadEvent += args => ReloadConfig();
		}

		private static void OnServerCommand(CommandEventArgs args)
		{
			if (args.Handled)
				return;

			if (string.IsNullOrWhiteSpace(args.Command))
			{
				args.Handled = true;
				return;
			}

			var commandText = IsValidCmd(args.Command) ? args.Command : Commands.Specifier + args.Command;

			args.Handled = ShortCommandUtil.HandleCommand(TSPlayer.Server, commandText);
		}

		private static void OnChat(ServerChatEventArgs args)
		{
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

			args.Handled = ShortCommandUtil.HandleCommand(tsplr, text);
		}

		private static void LoadShortCommands()
		{
			var list = new List<ShortCommand.ShortCommand>();

			foreach (var item in Config.ShortCommands)
			{
				try
				{
					list.Add(ShortCommand.ShortCommand.Create(item.ParameterDescription, item.CommandLines, item.Names));
				}
				catch (CommandParseException ex)
				{
					Console.WriteLine("加载简写指令时读取失败：{0}", ex);
				}
			}

			ShortCommands = list.ToArray();
		}

		public static ShortCommand.ShortCommand[] ShortCommands { get; private set; }

		private static bool IsValidCmd(string commandText)
		{
			return (
				       commandText.StartsWith(TShock.Config.CommandSpecifier) ||
				       commandText.StartsWith(TShock.Config.CommandSilentSpecifier) ||
				       commandText.StartsWith(Config.CommandSpecifier) ||
				       commandText.StartsWith(Config.CommandSpecifier2)
			       )
			       && !string.IsNullOrWhiteSpace(commandText.Substring(1));
		}
	}
}

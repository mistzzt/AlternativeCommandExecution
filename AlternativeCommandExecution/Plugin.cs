using System;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace AlternativeCommandExecution
{
	[ApiVersion(2, 1)]
	public partial class Plugin : TerrariaPlugin
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
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);
			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInit, -1000);

			TShockAPI.Hooks.GeneralHooks.ReloadEvent += args => ReloadConfig();
		}

		private static void OnPostInit(EventArgs args)
		{
			SwitchCmd_OnPostInit();
		}

		private static void OnGetData(GetDataEventArgs args)
		{
			if (args.Handled)
			{
				return;
			}

			var player = TShock.Players[args.Msg.whoAmI];
			if (player == null || !player.ConnectionAlive)
			{
				return;
			}

			using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
			{
				switch (args.MsgID)
				{
					case PacketTypes.HitSwitch:
						OnHitSwitch(data, player);
						break;
				}
			}

			
		}
	}
}

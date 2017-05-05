using System;
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

		public override void Initialize()
		{
			
		}
	}
}

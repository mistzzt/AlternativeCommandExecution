using System;
using System.Runtime.InteropServices;

namespace AlternativeCommandExecution.ShortCommand
{
	[StructLayout(LayoutKind.Auto)]
	public struct Argument
	{
		public Argument(ArgumentKind kind, string argumentName, string defaultValue)
		{
			Kind = kind;
			DefaultValue = defaultValue;
			Name = argumentName;
		}

		public ArgumentKind Kind { get; }

		public string DefaultValue { get; }

		public string Name { get; }

		public override string ToString()
		{
			return string.Format("Name: {0}, Kind: {1}, Default: {2}",
										Name,
												Kind,
									string.IsNullOrWhiteSpace(DefaultValue) ? "null" : DefaultValue);
		}

		public string ToString(CommandExectionContext context, string value = null)
		{
			switch (Kind)
			{
				case ArgumentKind.DefaultValue:
				case ArgumentKind.NotRequired:
				{
					return (value ?? DefaultValue) ?? string.Empty;
				}
				case ArgumentKind.Required:
				{
					if (string.IsNullOrWhiteSpace(value))
					{
						throw new ArgumentNullException(nameof(value));
					}

					return value;
				}
				case ArgumentKind.PlayerName:
				{
					return "Íæ¼ÒÃû£¨Õæ£©";
					//return context.Player.Name;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(Kind));
			}
		}
	}
}
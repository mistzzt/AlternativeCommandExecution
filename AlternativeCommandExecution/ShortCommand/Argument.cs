using System;
using System.Runtime.InteropServices;

namespace AlternativeCommandExecution.ShortCommand
{
	[StructLayout(LayoutKind.Auto)]
	public struct Argument
	{
		public Argument(ArgumentType type, string argumentName, string defaultValue)
		{
			Type = type;
			DefaultValue = defaultValue;
			Name = argumentName;
		}

		public ArgumentType Type { get; }

		public string DefaultValue { get; }

		public string Name { get; }

		public override string ToString()
		{
			return string.Format("Name: {0}, Type: {1}, Default: {2}",
										Name,
												Type,
									string.IsNullOrWhiteSpace(DefaultValue) ? "null" : DefaultValue);
		}

		public string ToString(CommandExectionContext context, string value = null)
		{
			switch (Type)
			{
				case ArgumentType.DefaultValue:
				case ArgumentType.NotRequired:
					{
						return (value ?? DefaultValue) ?? string.Empty;
					}
				case ArgumentType.Required:
					{
						if (string.IsNullOrWhiteSpace(value))
						{
							throw new ArgumentNullException(nameof(value));
						}

						return value;
					}
				case ArgumentType.PlayerName:
					{
						return context?.Player?.Name ?? "None";
					}
				default:
					throw new ArgumentOutOfRangeException(nameof(Type));
			}
		}
	}
}
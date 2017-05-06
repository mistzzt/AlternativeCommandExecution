using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AlternativeCommandExecution.ShortCommand
{
	public sealed class ShortCommand
	{
		public static ShortCommand Create(string desc, string[] commandLines, params string[] names)
		{
			if (names.Length == 0)
			{
				throw new ArgumentException("需要至少一个参数名。", nameof(names));
			}

			var sc = new ShortCommand(desc, commandLines, names);

			sc.InitializeArguments();
			sc.InitializeCommandLines();
			sc.InitializeHelpText();

			return sc;
		}

		private ShortCommand(string desc, string[] lines, string[] names)
		{
			_argumentDescription = desc;
			_commandLines = (string[])lines.Clone();

			Names = (string[])names.Clone();
		}

		public string[] Names { get; }

		public Argument[] Arguments { get; private set; }

		public string[] FormatLines { get; private set; }

		public string ArgumentHelpText { get; private set; }

		private byte _fewestArgCount;

		public string[] Convert(CommandExectionContext ctx, string[] args)
		{
			if (args.Length < _fewestArgCount)
			{
				throw new CommandArgumentException("语法无效！正确语法：" + TShockAPI.Commands.Specifier + ArgumentHelpText);
			}

			var values = new string[Arguments.Length];
			for (var index = 0; index < values.Length; index++)
			{
				values[index] = Arguments[index].ToString(ctx, args.ElementAtOrDefault(index));
			}

			return FormatLines.Select(x => string.Format(x, values/*.Select(v => (object)v).ToArray()*/)).ToArray();
		}

		public bool HasName(string name) => Names.Any(x => x.Equals(name, StringComparison.Ordinal));

		private void InitializeArguments()
		{
			var args = new List<Argument>();

			var state = ParseState.Normal;

			var argName = new StringBuilder();
			var defaultValue = new StringBuilder();
			var kind = ArgumentType.Required;

			void InternalReset()
			{
				state = ParseState.Normal;
				argName.Clear();
				defaultValue.Clear();
				kind = ArgumentType.Required;
			}

			for (var index = 0; index < _argumentDescription.Length; index++)
			{
				var c = _argumentDescription[index];

				switch (c)
				{
					case LeftBracket:
						if (state != ParseState.Normal)
						{
							throw new CommandParseException($"invalid {c} here", _argumentDescription, index);
						}
						state = ParseState.InsideBracket;
						continue;
					case RightBracket:
						if (state != ParseState.InsideBracket)
						{
							throw new CommandParseException($"invalid {c} here", _argumentDescription, index);
						}
						if (argName.Length == 0)
						{
							throw new CommandParseException("Argument must have a name", _argumentDescription, index);
						}
						args.Add(new Argument(kind, argName.ToString(), defaultValue.ToString()));
						InternalReset();
						state = ParseState.Normal;
						continue;
				}

				switch (state)
				{
					case ParseState.Normal:
						// do nothing
						break;
					case ParseState.InsideBracket:
						if (VanRegex.IsMatch(c.ToString()))
						{
							argName.Append(c);
						}
						else
						{
							switch (c)
							{
								case DefaultValueRepresentation:
									while ((c = _argumentDescription[++index]) != RightBracket)
									{
										defaultValue.Append(c);
									}
									index--;
									kind = ArgumentType.DefaultValue;
									continue;
								case SpecialValueRepresentation: // special value
									var special = new StringBuilder();
									while ((c = _argumentDescription[++index]) != RightBracket)
									{
										special.Append(c);
									}
									index--;
									argName = special;
									switch (special.ToString().Trim())
									{
										case "Player":
											kind = ArgumentType.PlayerName;
											continue;
									}
									continue;
								case OptionalRepresentation: // optional
									if (argName.Length != 0)
									{
										throw new CommandParseException("Wrong position for " + OptionalRepresentation, _argumentDescription, index);
									}
									kind = ArgumentType.NotRequired;
									continue;
								default:
									throw new CommandParseException("Invalid character " + c, _argumentDescription, index);
							}
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (state != ParseState.Normal)
			{
				throw new CommandParseException("fuck you", "cnm", 0);
			}

			Arguments = args.ToArray();
			_fewestArgCount = (byte)Arguments.Count(x => x.Type == ArgumentType.Required);
		}

		private void InitializeCommandLines()
		{
			var lines = new List<string>();

			foreach (var line in _commandLines)
			{
				var state = ParseState.Normal;
				var argName = new StringBuilder();

				var format = new StringBuilder();

				foreach (var c in line)
				{
					switch (c)
					{
						case LeftBracket:
							if (state != ParseState.Normal)
							{
								throw new CommandParseException($"invalid {c} here");
							}
							state = ParseState.InsideBracket;
							continue;
						case RightBracket:
							if (state != ParseState.InsideBracket)
							{
								throw new CommandParseException($"invalid {c} here");
							}
							var name = argName.ToString();
							if (name.Length == 0)
							{
								throw new CommandParseException("Argument must have a name");
							}

							var formatIndex = Array.FindIndex(Arguments, x => x.Name.Equals(name, StringComparison.Ordinal));
							if (formatIndex == -1)
							{
								throw new CommandParseException("Undeclared argument: " + name);
							}
							format.AppendFormat("{{{0}}}", formatIndex);

							argName.Clear();
							state = ParseState.Normal;
							continue;
					}

					switch (state)
					{
						case ParseState.Normal:
							format.Append(c);
							break;
						case ParseState.InsideBracket:
							if (VanRegex.IsMatch(c.ToString()))
							{
								argName.Append(c);
							}
							break;
					}
				}

				lines.Add(format.ToString());
			}

			FormatLines = lines.ToArray();
		}

		private void InitializeHelpText()
		{
			var sb = new StringBuilder(Names[0]);

			const string requiredFormat = " <{0}>";
			const string notRequiredFormat = " [{0}]";

			foreach (var p in Arguments)
			{
				sb.AppendFormat(p.Type == ArgumentType.Required ? requiredFormat : notRequiredFormat, p.Name);
			}

			ArgumentHelpText = sb.ToString();
		}

		private readonly string _argumentDescription;

		private readonly string[] _commandLines;

		private static readonly Regex VanRegex = new Regex(ValidateArgumentNameRegex, RegexOptions.Compiled);

		private const char OptionalRepresentation = '%';

		private const char LeftBracket = '{';

		private const char RightBracket = '}';

		private const char DefaultValueRepresentation = '|';

		private const char SpecialValueRepresentation = '$';

		private const string ValidateArgumentNameRegex = @"[\w\-]";

		private enum ParseState : byte
		{
			Normal,
			InsideBracket
		}
	}
}

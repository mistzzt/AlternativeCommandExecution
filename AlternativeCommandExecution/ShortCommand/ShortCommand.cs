using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AlternativeCommandExecution.ShortCommand
{
	public sealed class ShortCommand
	{
		public static ShortCommand Create(string alias, string[] commandLines)
		{
			/*
			 * command alias: `test {arg-swap-2} {normal-arg} {arg-swap-1} {%opt} {...}`
			 * command lines: { "real-test {normal-arg}", "real-test-swap {arg-swap-2} {arg-swap-1}", "test-opt {normal-arg} {%opt} {...}" }
			*/

			var sc = new ShortCommand(alias, commandLines);

			sc.InitializeArguments();
			sc.InitializeCommandLines();

			return sc;
		}

		private ShortCommand(string alias, string[] lines)
		{
			_aliasText = alias;
			_commandLines = (string[])lines.Clone();
		}

		public Argument[] Arguments { get; private set; }

		public string[] FormatLines { get; private set; }

		private byte _fewestArgCount;

		public string[] Convert(CommandExectionContext ctx, string[] args)
		{
			if (args.Length < _fewestArgCount)
			{
				throw new CommandArgumentException("");
			}

			var values = new string[Arguments.Length];
			for (var index = 0; index < values.Length; index++)
			{
				values[index] = Arguments[index].ToString(ctx, args.ElementAtOrDefault(index));
			}

			return FormatLines.Select(x => string.Format(x, values/*.Select(v => (object)v).ToArray()*/)).ToArray();
		}

		private void InitializeArguments()
		{
			var args = new List<Argument>();

			var state = ParseState.Normal;

			var argName = new StringBuilder();
			var defaultValue = new StringBuilder();
			var kind = ArgumentKind.Required;

			void InternalReset()
			{
				state = ParseState.Normal;
				argName.Clear();
				defaultValue.Clear();
				kind = ArgumentKind.Required;
			}

			for (var index = 0; index < _aliasText.Length; index++)
			{
				var c = _aliasText[index];

				switch (c)
				{
					case LeftBracket:
						if (state != ParseState.Normal)
						{
							throw new CommandParseException($"invalid {c} here", _aliasText, index);
						}
						state = ParseState.InsideBracket;
						continue;
					case RightBracket:
						if (state != ParseState.InsideBracket)
						{
							throw new CommandParseException($"invalid {c} here", _aliasText, index);
						}
						if (argName.Length == 0)
						{
							throw new CommandParseException("Argument must have a name", _aliasText, index);
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
									while ((c = _aliasText[++index]) != RightBracket)
									{
										defaultValue.Append(c);
									}
									index--;
									kind = ArgumentKind.DefaultValue;
									continue;
								case SpecialValueRepresentation: // special value
									var special = new StringBuilder();
									while ((c = _aliasText[++index]) != RightBracket)
									{
										special.Append(c);
									}
									index--;
									argName = special;
									switch (special.ToString().Trim())
									{
										case "Player":
											kind = ArgumentKind.PlayerName;
											continue;
									}
									continue;
								case OptionalRepresentation: // optional
									if (argName.Length != 0)
									{
										throw new CommandParseException("Wrong position for " + OptionalRepresentation, _aliasText, index);
									}
									kind = ArgumentKind.NotRequired;
									continue;
								default:
									throw new CommandParseException("Invalid character " + c, _aliasText, index);
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
			_fewestArgCount = (byte)Arguments.Count(x => x.Kind == ArgumentKind.Required);
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

		private readonly string _aliasText;

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

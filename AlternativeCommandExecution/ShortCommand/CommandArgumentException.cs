using System;

namespace AlternativeCommandExecution.ShortCommand
{
	public sealed class CommandArgumentException : Exception
	{
		public CommandArgumentException(string msg) : base(msg) { }
	}
}

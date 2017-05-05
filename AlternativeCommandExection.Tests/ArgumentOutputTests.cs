using AlternativeCommandExecution.ShortCommand;
using NUnit.Framework;

namespace AlternativeCommandExection.Tests
{
	[TestFixture]
	public sealed class ArgumentOutputTests
	{
		[TestCase(ArgumentType.NotRequired)]
		[TestCase(ArgumentType.DefaultValue)]
		public void ArgumentOutput_DefaultValue(ArgumentType kind, string defaultValue = "default", string value = null)
		{
			Assert.AreEqual(new Argument(kind, null, defaultValue).ToString(TestConfiguration.Context, value), "default");
		}

		[TestCase(ArgumentType.Required, null)]
		public void ArgumentOutput_Exception(ArgumentType kind, string value = null)
		{
			Assert.That(() => new Argument(kind, null, null).ToString(TestConfiguration.Context, value), Throws.ArgumentNullException);
		}

		private static class TestConfiguration
		{
			public static CommandExectionContext Context { get; } = null;
		}
	}
}

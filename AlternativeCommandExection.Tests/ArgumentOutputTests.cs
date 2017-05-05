using AlternativeCommandExecution.ShortCommand;
using NUnit.Framework;

namespace AlternativeCommandExection.Tests
{
	[TestFixture]
	public sealed class ArgumentOutputTests
	{
		[TestCase(ArgumentKind.NotRequired)]
		[TestCase(ArgumentKind.DefaultValue)]
		public void ArgumentOutput_DefaultValue(ArgumentKind kind, string defaultValue = "default", string value = null)
		{
			Assert.AreEqual(new Argument(kind, null, defaultValue).ToString(TestConfiguration.Context, value), "default");
		}

		[TestCase(ArgumentKind.Required, null)]
		public void ArgumentOutput_Exception(ArgumentKind kind, string value = null)
		{
			Assert.That(() => new Argument(kind, null, null).ToString(TestConfiguration.Context, value), Throws.ArgumentNullException);
		}

		private static class TestConfiguration
		{
			public static CommandExectionContext Context { get; } = null;
		}
	}
}

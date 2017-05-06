using AlternativeCommandExecution.ShortCommand;
using NUnit.Framework;

namespace AlternativeCommandExection.Tests
{
	[TestFixture]
	public sealed class ArgumentOutputTests
	{
		[TestCase(ParameterType.NotRequired)]
		[TestCase(ParameterType.DefaultValue)]
		public void ArgumentOutput_DefaultValue(ParameterType kind, string defaultValue = "default", string value = null)
		{
			Assert.AreEqual(new Parameter(kind, null, defaultValue).ToString(TestConfiguration.Context, value), "default");
		}

		[TestCase(ParameterType.Required, null)]
		public void ArgumentOutput_Exception(ParameterType kind, string value = null)
		{
			Assert.That(() => new Parameter(kind, null, null).ToString(TestConfiguration.Context, value), Throws.ArgumentNullException);
		}

		private static class TestConfiguration
		{
			public static CommandExectionContext Context { get; } = null;
		}
	}
}

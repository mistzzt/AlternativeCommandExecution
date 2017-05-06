using System;
using AlternativeCommandExecution.ShortCommand;
using NUnit.Framework;

namespace AlternativeCommandExection.Tests
{
	[TestFixture]
	public class AliasParseTests
	{
		[TestCase("/a1 {arg1}", "/test {arg1} {arg1}")]
		[TestCase("/a3 {arg1} {arg2} {arg3}", "/test1 {arg2}", "/test2 {arg3} {arg3} {arg1}")]
		[TestCase("/Fi {arg1|def} {arg2|def2} {arg3} {%arg4}")]
		[TestCase("/测试 {$Player} {物品名|电音吉他} {数量} {%前缀}")]
		[TestCase("{$Player}")]
		public void ParseTest_Alias(string alias, params string[] lines)
		{
			Console.WriteLine("Input alias: {0}", alias);
			Console.WriteLine("Lines:");
			Console.WriteLine(string.Join(Environment.NewLine, lines));
			Console.WriteLine();

			var sc = ShortCommand.Create(alias, lines);

			foreach (var item in sc.Parameters)
			{
				Console.WriteLine(item.ToString());
			}

			Console.WriteLine();

			foreach (var item in sc.FormatLines)
			{
				Console.WriteLine(item);
			}
		}

		[TestCase("/f {argument1")]
		[TestCase("{}")]
		[TestCase("{arg1}", "{arg2}")]
		[TestCase("{%|}")]
		public void ParseTest_Throw(string alias, params string[] lines)
		{
			Assert.That(() => ShortCommand.Create(alias, lines), Throws.TypeOf<CommandParseException>());
		}

		[TestCase("第一", "第二")]
		[TestCase("第一", "第二", "你们好吗？")]
		[TestCase("第一", "第二", "还可以吧", "测试！")]
		public void ParseTest_Convert(params string[] args)
		{
			var list = Sc.Convert(Ctx, args);

			foreach (var item in list)
			{
				Console.WriteLine(item);
			}
		}

		public readonly ShortCommand Sc = ShortCommand.Create("/fuck {arg0} {arg1} {arg2|this is arg2} {%arg3} {$Player}",
			new[]
			{
				"/me I am {Player} and I want to say {arg2}",
				"/me now I also want to say fuck {arg0} and {arg1}",
				"/me hey, {arg3}!"
			});

		public readonly CommandExectionContext Ctx = new CommandExectionContext(null);
	}
}

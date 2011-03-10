﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Code2Xml.Languages.Java.XmlGenerators;
using NUnit.Framework;
using Ucpf.Core.Model;
using Ucpf.Core.Tests;
using Ucpf.Languages.CSharp.Tests;
using Ucpf.Languages.Java.Model;

namespace Ucpf.Languages.Java.Tests {
	[TestFixture]
	public class JavaFibonacciTest {
		private static readonly string Code =
			File.ReadAllText(Fixture.GetInputPath("Java", "Fibonacci.java"));

		[Test]
		public void CreateDefineFunction() {
			var ast =
				JavaXmlGenerator.Instance.Generate("public static int fibonacci(int n){}",
					p => p.methodDeclaration());
			var actual = JavaModelFactory.CreateDefineFunction(ast);
			var expectation = new UnifiedFunctionDefinition {
				Modifiers = {
					UnifiedModifier.Create("public"),
					UnifiedModifier.Create("static"),
				},
				Type = UnifiedType.Create("int"),
				Name = "fibonacci",
				Parameters = {
					new UnifiedParameter {
						Name = "n",
						Type = UnifiedType.Create("int"),
					}
				},
			};
			Assert.That(actual,
				Is.EqualTo(expectation).Using(StructuralEqualityComparer.Instance));
		}

		[Test]
		public void CreateFibonacci() {
			var actual = JavaModelFactory.CreateModel(Code);
			var expected = CSharpFibonacciTest.Model;
			Assert.That(actual,
				Is.EqualTo(expected).Using(StructuralEqualityComparer.Instance));
		}
	}
}
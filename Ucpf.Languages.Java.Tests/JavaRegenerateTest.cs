﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Paraiba.Core;
using Ucpf.Core.Model;
using Ucpf.Core.Tests;
using Ucpf.Languages.Java.Model;

namespace Ucpf.Languages.Java.Tests {
	[TestFixture]
	public class JavaRegenerateTest {
		private const string JavacPath = "javac";

		private static IEnumerable<byte[]> GetByteCode(string workPath,
		                                               string fileName) {
			var args = new[] {
				"\"" + Path.Combine(workPath, fileName) + "\""
			};
			var argg = args.JoinString(" ");
			var info = new ProcessStartInfo {
				FileName = JavacPath,
				Arguments = argg,
				CreateNoWindow = true,
				UseShellExecute = false,
				WorkingDirectory = workPath,
			};
			using (var p = Process.Start(info)) {
				p.WaitForExit();
			}

			return Directory.EnumerateFiles(workPath, "*.class",
				SearchOption.AllDirectories)
				.Select(File.ReadAllBytes);
		}

		private static IEnumerable<TestCaseData> TestCases {
			get {
				return Directory.EnumerateFiles(Fixture.GetInputPath("Java"))
					.Select(path => new TestCaseData(path));
			}
		}

		[Test, TestCase(@"..\..\fixture\Java\input\Fibonacci.java")]
		public void CompareThroughByteCodeForSameCode(string orgPath) {
			var workPath = Fixture.CleanTemporalPath();
			var fileName = Path.GetFileName(orgPath);
			var srcPath = Fixture.GetTemporalPath(fileName);
			File.Copy(orgPath, srcPath);
			var expected = GetByteCode(workPath, fileName);
			var actual = GetByteCode(workPath, fileName);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, TestCase(@"..\..\fixture\Java\input\Fibonacci.java")]
		public void CompareThroughModelForSameCode(string orgPath) {
			var orgCode = File.ReadAllText(orgPath);
			var expected = JavaModelFactory.CreateModel(orgCode);
			var actual = JavaModelFactory.CreateModel(orgCode);
			Assert.That(actual, Is.EqualTo(expected)
				.Using(StructuralEqualityComparer.Instance));
		}

		[Test, TestCaseSource("TestCases")]
		public void CompareThroughByteCode(string orgPath) {
			var workPath = Fixture.CleanTemporalPath();
			var fileName = Path.GetFileName(orgPath);
			var srcPath = Fixture.GetTemporalPath(fileName);
			File.Copy(orgPath, srcPath);
			var expected = GetByteCode(workPath, fileName);
			var orgCode = File.ReadAllText(orgPath);
			var model = JavaModelFactory.CreateModel(orgCode);
			var code = JavaCodeGenerator.Generate(model);
			File.WriteAllText(srcPath, code);
			var actual = GetByteCode(workPath, fileName);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, TestCaseSource("TestCases")]
		public void CompareThroughModel(string orgPath) {
			var orgCode = File.ReadAllText(orgPath);
			var expected = JavaModelFactory.CreateModel(orgCode);
			var newCode = JavaCodeGenerator.Generate(expected);
			var actual = JavaModelFactory.CreateModel(newCode);
			Assert.That(actual, Is.EqualTo(expected)
				.Using(StructuralEqualityComparer.Instance));
		}
	}
}
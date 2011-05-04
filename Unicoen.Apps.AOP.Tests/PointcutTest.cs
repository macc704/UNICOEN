﻿#region License

// Copyright (C) 2011 The Unicoen Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Paraiba.Text;
using Unicoen.Apps.AOP;
using Unicoen.Core.Model;
using Unicoen.Core.Tests;

namespace Unicoen.Apps.Aop.Tests {
	/// <summary>
	///   アスペクトが正しく織り込まれているかテストする。
	/// </summary>
	[TestFixture]
	internal class PointcutTest {
		private readonly string _fibonacciPath =
				Fixture.GetInputPath("Java", "Default", "Fibonacci.java");
		private readonly string _studentPath =
				Fixture.GetInputPath("Java", "Default", "Student.java");
		
		public UnifiedProgram CreateModel(string path) {
			var ext = Path.GetExtension(path);
			var code = File.ReadAllText(path, XEncoding.SJIS);
			return Program.CreateModel(ext, code);
		}

		[Test]
		public void WeavingAtBeforeExecutionAll() {
			var model = CreateModel(_fibonacciPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Fibonacci_functionBefore.java"));
			
			CodeProcessor.InsertAtBeforeExecutionAll(model, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test]
		public void WeavingAtAfterExecutionAll() {
			var model = CreateModel(_fibonacciPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Fibonacci_functionAfter.java"));
			
			CodeProcessor.InsertAtAfterExecutionAll(model, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test]
		[TestCase("^fib")]
		public void WeavingAtBeforeExecutionByRegex(string regex) {
			var model = CreateModel(_fibonacciPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Fibonacci_functionBefore.java"));
			
			CodeProcessor.InsertAtBeforeExecution(model, new Regex(regex), "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test]
		[TestCase("^fib")]
		public void WeavingAtAfterExecutionByRegex(string regex) {
			var model = CreateModel(_fibonacciPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Fibonacci_functionAfter.java"));
			
			CodeProcessor.InsertAtAfterExecution(model, new Regex(regex), "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test]
		[TestCase("fibonacci")]
		public void WeavingAtBeforeExecutionByName(string name) {
			var model = CreateModel(_fibonacciPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Fibonacci_functionBefore.java"));
			
			CodeProcessor.InsertAtBeforeExecutionByName(model, name, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test]
		[TestCase("fibonacci")]
		public void WeavingAtAfterExecutionByName(string name) {
			var model = CreateModel(_fibonacciPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Fibonacci_functionAfter.java"));
			
			CodeProcessor.InsertAtAfterExecutionByName(model, name, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}


		[Test]
		public void WeavingAtBeforeCallAll() {
			var model = CreateModel(_studentPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Student_callBefore.java"));

			CodeProcessor.InsertAtBeforeCallAll(model, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test]
		public void WeavingAtAfterCallAll() {
			var model = CreateModel(_studentPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Student_callAfter.java"));

			CodeProcessor.InsertAtAfterCallAll(model, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		//TODO 関数呼び出し(UnifiedCall)の名前の抽出が現状ではできないので、一旦無視する
		[Test, Ignore]
		[TestCase("^w")]
		public void WeavingAtBeforeCallByRegex(string regex) {
			var model = CreateModel(_studentPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Student_callBefore.java"));

			CodeProcessor.InsertAtBeforeCall(model, new Regex(regex), "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test, Ignore]
		[TestCase("^w")]
		public void WeavingAtAfterCallByRegex(string regex) {
			var model = CreateModel(_studentPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Student_callAfter.java"));

			CodeProcessor.InsertAtAfterCall(model, new Regex(regex), "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test, Ignore]
		[TestCase("write")]
		public void WeavingAtBeforeCallByName(string name) {
			var model = CreateModel(_studentPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Student_callBefore.java"));

			CodeProcessor.InsertAtBeforeCallByName(model, name, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		[Test, Ignore]
		[TestCase("write")]
		public void WeavingAtAfterCallByName(string name) {
			var model = CreateModel(_studentPath);
			var actual =
					CreateModel(
							Fixture.GetAopExpectationPath("Java", "Student_callAfter.java"));

			CodeProcessor.InsertAtAfterCallByName(model, name, "{Console.Write();}");

			//TODO ToString()しないと比較できないか
			Assert.That(model.ToString(), Is.EqualTo(actual.ToString()));
		}

		//TODO 多項式中や関数の引数として現れるUnifiedCallに対しては、処理が行われないことを確認するテストを書く
	}
}
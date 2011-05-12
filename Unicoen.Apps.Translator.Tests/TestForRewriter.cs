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

using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Unicoen.Apps.Translator.Filter;
using Unicoen.Core.Model;
using Unicoen.Core.Tests;
using Unicoen.Languages.Java;

namespace Unicoen.Apps.Translator.Tests {
	public class TestForRewriter {
		private UnifiedProgram _program;

		[SetUp]
		public void SetUp() {
			var filePath = FixtureUtil.GetInputPath("Java", "default", "Student.java");
			var code = File.ReadAllText(filePath, Encoding.Default);
			_program = JavaFactory.GenerateModel(code);
		}

		[Test]
		public void メソッド名を書き換えられる() {
			Console.WriteLine(JavaFactory.GenerateCode(_program));

			var functions =
					Finder.Finder.Instance.GetAllElements<UnifiedFunctionDefinition>(_program);
			var f = FunctionFinder.Instance.FindByName("getName", functions).ElementAt(0);
					// getName()
			Rewriter.Rewriter.Instance.RewiteIdentifierName("getName2", f);

			functions =
					Finder.Finder.Instance.GetAllElements<UnifiedFunctionDefinition>(_program);
			f = functions.ElementAt(0);
			Assert.That(f.Name.Value, Is.EqualTo("getName2"));

			Console.WriteLine(JavaFactory.GenerateCode(_program));
		}

		[Test]
		public void メソッドを削除できる() {
			Console.WriteLine(JavaFactory.GenerateCode(_program));

			var functions =
					Finder.Finder.Instance.GetAllElements<UnifiedFunctionDefinition>(_program);
			var f = FunctionFinder.Instance.FindByName("getName", functions).ElementAt(0);
					// getName()
			Rewriter.Rewriter.Instance.RewiteIdentifierName("getName2", f);

			functions =
					Finder.Finder.Instance.GetAllElements<UnifiedFunctionDefinition>(_program);
			f = functions.ElementAt(0);
			Rewriter.Rewriter.Instance.DeleteElement(f);

			Console.WriteLine(JavaFactory.GenerateCode(_program));
		}

		[Test]
		public void 要素を付け替えられる() {
			var newType = UnifiedType.CreateUsingString("Integer");

			var functions =
					Finder.Finder.Instance.GetAllElements<UnifiedFunctionDefinition>(_program);
			var f = functions.ElementAt(0);
			var type = f.Type;
			Console.WriteLine(type.Name);
			Console.WriteLine(newType.Name);
			Rewriter.Rewriter.Instance.ExchageElement(type, newType);

			Console.WriteLine(JavaFactory.GenerateCode(_program));
		}
	}
}
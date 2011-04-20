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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unicoen.Core.Tests;

namespace Unicoen.Languages.Java.Tests {
	public static class JavaFixture {
		/// <summary>
		///   テスト時に入力されるA.javaファイルのメソッド宣言の中身です。
		///   <c>class A { public void M1() { ... } }</c>の...部分に
		///   このプロパティで指定されたコード断片を埋め込んでA.javaファイルが生成されます。
		/// </summary>
		public static IEnumerable<TestCaseData> TestStatements {
			get {
				return new[] {
						"M1();",
						"new A();",
				}.Select(s => new TestCaseData(DecorateWithClassAndMethod(s)));
			}
		}

		/// <summary>
		///   テスト時に入力されるA.javaファイルのメソッド宣言の中身です。
		///   <c>class A { public void M1() { ... } }</c>の...部分に
		///   このプロパティで指定されたコード断片を埋め込んでA.javaファイルが生成されます。
		/// </summary>
		public static IEnumerable<TestCaseData> TestCodes {
			get {
				return new[] {
						"class A { }",
						"public class A { }",
				}.Select(s => new TestCaseData(s));
			}
		}

		public static IEnumerable<TestCaseData> TestFilePathes {
			get {
				// 必要に応じて以下の要素をコメントアウト
				return new[] {
						"Fibonacci.java",
				}
						.Select(s => new TestCaseData(Fixture.GetInputPath("Java", s)));
				//return Directory.EnumerateFiles(Fixture.GetInputPath("Java"))
				//    .Select(path => new TestCaseData(path));
			}
		}

		public static IEnumerable<TestCaseData> TestDirectoryPathes {
			get {
				return new[] {
						new { DirName = "default", Command = "javac", Arguments = "*.java" },
						new { DirName = "NewTestFiles", Command = "javac", Arguments = "*.java" },
				}
						.Select(
								o => new TestCaseData(
								     		Fixture.GetInputPath("Java", o.DirName),
								     		o.Command, o.Arguments));
			}
		}

		private static string DecorateWithClassAndMethod(string statement) {
			return "class A { public void M1() {" + statement + "} }";
		}

		public static IEnumerable<string> GetAllSourceFilePaths(string workPath) {
			return Directory.EnumerateFiles(
					workPath, "*.java",
					SearchOption.AllDirectories);
		}
	}
}
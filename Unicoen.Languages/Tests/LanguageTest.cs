﻿#region License

// Copyright (C) 2011-2012 The Unicoen Project
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Paraiba.IO;
using Paraiba.Linq;
using Paraiba.Text;
using Unicoen.Model;
using Unicoen.TestUtils;
using Unicoen.Tests;

namespace Unicoen.Languages.Tests {
	public class LanguageTest {
		public LanguageTest(Fixture fixture) {
			Fixture = fixture;
		}

		public Fixture Fixture { get; set; }

		/// <summary>
		///   テスト対象のソースコードの集合を取得します．
		/// </summary>
		public IEnumerable<TestCaseData> TestCodes {
			get { return Fixture.TestCodes; }
		}

		/// <summary>
		///   テスト対象のソースコードのパスの集合を取得します．
		/// </summary>
		public IEnumerable<TestCaseData> TestFilePaths {
			get { return Fixture.TestFilePaths; }
		}

		/// <summary>
		///   テスト対象のプロジェクトのパスとコンパイルのコマンドと引数の集合を取得します．
		/// </summary>
		public IEnumerable<TestCaseData> TestProjectInfos {
			get { return Fixture.TestProjectInfos; }
		}

		private Tuple<string, UnifiedProgram> GenerateCodeObject(string path) {
			var code = GuessEncoding.ReadAllText(path);
			var obj = Fixture.ProgramGenerator.Generate(code);
			return Tuple.Create(code, obj);
		}

		/// <summary>
		///   指定したソースコードから統一コードオブジェクトを生成して， 生成した統一コードオブジェクトが適切な性質を備えているか検査します．
		/// </summary>
		/// <param name="code"> 検査対象のソースコード </param>
		public void VerifyCodeObjectFeatureUsingCode(string code) {
			var codeObject = Fixture.ProgramGenerator.Generate(code);
			AssertModelFeature(codeObject, "no file");
		}

		/// <summary>
		///   指定したパスのソースコードの統一コードオブジェクトを生成して， 生成した統一コードオブジェクトが適切な性質を備えているか検査します．
		/// </summary>
		/// <param name="path"> 検査対象のソースコードのパス </param>
		public void VerifyCodeObjectFeatureUsingFile(string path) {
			var codeAndObject = GenerateCodeObject(path);
			AssertModelFeature(codeAndObject.Item2, path);
		}

		/// <summary>
		///   指定したパスのソースコードの統一コードオブジェクトを生成して， 生成した統一コードオブジェクトが適切な性質を備えているか検査します．
		/// </summary>
		/// <param name="dirPath"> 検査対象のソースコードが格納されているディレクトリのパス </param>
		/// <param name="relativePathsForBinaryFiles">使用しません</param>
		/// <param name="compileActionWithWorkDirPath">使用しません</param>
		public void VerifyCodeObjectFeatureUsingProject(
				string dirPath, IList<string> relativePathsForBinaryFiles,
				Action<string> compileActionWithWorkDirPath) {
			var paths = Fixture.GetAllSourceFilePaths(dirPath);
			foreach (var path in paths) {
				var codeAndObject = GenerateCodeObject(path);
				AssertModelFeature(codeAndObject.Item2, path);
			}
		}

		/// <summary>
		///   再生成を行わずAssertCompareModelが正常に動作するかテストします。 全く同じコードから生成したモデル同士で比較します。
		/// </summary>
		/// <param name="orgPath"> 再生成するソースコードのパス </param>
		public void VerifyAssertCompareModel(string orgPath) {
			var expected = GenerateCodeObject(orgPath);
			var actual = GenerateCodeObject(orgPath);
			Assert.That(
					actual.Item2,
					Is.EqualTo(expected.Item2).Using(
							StructuralEqualityComparerForDebug.Instance));
		}

		/// <summary>
		///   再生成を行わずAssertCompareCompiledCodeが正常に動作するかテストします。 全く同じコードをコンパイルしたバイナリファイル同士で比較します。
		/// </summary>
		/// <param name="orgPath"> 再生成するソースコードのパス </param>
		public void VerifyAssertCompareCompiledCode(string orgPath) {
			var workPath = FixtureUtil.CleanOutputAndGetOutputPath();
			var fileName = Path.GetFileName(orgPath);
			var srcPath = FixtureUtil.GetOutputPath(fileName);
			GuessEncoding.Convert(orgPath, srcPath, Encoding.Default);
			Fixture.Compile(workPath, srcPath);
			var expected = Fixture.GetAllCompiledCode(workPath);
			Fixture.Compile(workPath, srcPath);
			var actual = Fixture.GetAllCompiledCode(workPath);
			AssertFuzzyEquals(actual, expected);
		}

		/// <summary>
		///   指定したソースコードから統一コードオブジェクトを生成して， ソースコードと統一コードオブジェクトを正常に再生成できるか検査します．
		/// </summary>
		/// <param name="code"> 検査対象のソースコード </param>
		public void VerifyRegenerateCodeUsingCode(string code) {
			var codeObject = Fixture.ProgramGenerator.Generate(code);
			AssertEqualsModel(code, codeObject);
			AssertEqualsCompiledCode(code, "A" + Fixture.Extension, codeObject);
		}

		/// <summary>
		///   指定したパスのソースコードの統一コードオブジェクトを生成して， ソースコードと統一コードオブジェクトを正常に再生成できるか検査します．
		/// </summary>
		/// <param name="path"> 検査対象のソースコードのパス </param>
		public void VerifyRegenerateCodeUsingFile(string path) {
			var codeAndObject = GenerateCodeObject(path);
			var code = codeAndObject.Item1;
			var codeObject = codeAndObject.Item2;
			AssertEqualsModel(code, codeObject);
			AssertEqualsCompiledCode(code, Path.GetFileName(path), codeObject);
		}

		/// <summary>
		///   指定したディレクトリ内のソースコードから統一コードオブジェクトを生成して， ソースコードと統一コードオブジェクトを正常に再生成できるか検査します．
		/// </summary>
		/// <param name="dirPath"> 検査対象のソースコードが格納されているディレクトリのパス </param>
		/// <param name="relativePathsForBinaryFiles">バイナリファイルが存在するディレクトリの相対パスのリスト</param>
		/// <param name="compileActionByWorkDirPath"> コンパイル処理 </param>
		public void VerifyRegenerateCodeUsingProject(
				string dirPath,
				IList<string> relativePathsForBinaryFiles,
				Action<string> compileActionByWorkDirPath) {
			// コンパイル用の作業ディレクトリの取得
			var workPath = FixtureUtil.CleanOutputAndGetOutputPath();
			// 作業ディレクトリ内にソースコードを配置
			FileUtility.CopyRecursively(dirPath, workPath);
			// 作業ディレクトリ内でコンパイル
			compileActionByWorkDirPath(workPath);
			// コンパイル結果の取得
			var orgByteCode1 = Fixture.GetAllCompiledCode(
					workPath, relativePathsForBinaryFiles);
			var codePaths = Fixture.GetAllSourceFilePaths(workPath);
			foreach (var codePath in codePaths) {
				var orgCode1 = GuessEncoding.ReadAllText(codePath);

				// モデルを生成して，合わせて各種検査を実施する
				var codeAndObject = GenerateCodeObject(codePath);
				AssertEqualsModel(orgCode1, codeAndObject.Item2);

				var code2 = Fixture.CodeGenerator.Generate(codeAndObject.Item2);
				File.WriteAllText(codePath, code2, XEncoding.SJIS);
			}
			// 再生成したソースコードのコンパイル結果の取得
			compileActionByWorkDirPath(workPath);
			var byteCode2 =
					Fixture.GetAllCompiledCode(workPath, relativePathsForBinaryFiles);
			AssertFuzzyEquals(byteCode2, orgByteCode1);
		}

		/// <summary>
		///   指定した統一コードオブジェクトが適切な性質を備えているか検査します．
		/// </summary>
		/// <param name="codeObject"> 検査対象の統一コードオブジェクト </param>
		/// <param name="message"> アサーションに違反した際のエラーメッセージ </param>
		private static void AssertModelFeature(
				UnifiedProgram codeObject, string message) {
			AssertDeepCopy(codeObject, message);
			AssertGetElements(codeObject, message);
			AssertGetElementReferences(codeObject, message);
			AssertGetElementReferenecesOfFields(codeObject, message);
			AssertParentProperty(codeObject, message);
			AssertToString(codeObject, message);
		}

		/// <summary>
		///   深いコピーが正常に動作するか検査します．
		/// </summary>
		/// <param name="codeObject"> 検査対象のモデル </param>
		/// <param name="message"> アサーションに違反した際のエラーメッセージ </param>
		private static void AssertDeepCopy(
				UnifiedProgram codeObject, string message) {
			var copiedModel = codeObject.DeepCopy();
			Assert.That(
					copiedModel,
					Is.EqualTo(codeObject).Using(
							StructuralEqualityComparerForDebug.Instance));

			var pairs = copiedModel.Descendants().Zip(codeObject.Descendants());
			foreach (var pair in pairs) {
				Assert.That(pair.Item1.Parent, Is.Not.Null, message);
				Assert.That(
						ReferenceEquals(pair.Item1.Parent, pair.Item2.Parent),
						Is.False, message);
			}
		}

		/// <summary>
		///   子要素の列挙機能が正常に動作するかソースーコードを指定して検査します。
		/// </summary>
		/// <param name="codeObject"> 検査対象のモデル </param>
		/// <param name="message"> アサーションに違反した際のエラーメッセージ </param>
		private static void AssertGetElements(
				UnifiedProgram codeObject, string message) {
			foreach (var element in codeObject.Descendants()) {
				var elements = element.Elements(); // Depends on field refelection
				var references = element.ElementReferences();
				var referenecesOfPrivateFields =
						element.ElementReferencesOfFields();
				var propValues = GetProperties(element).ToList();
				// Depends on property reflection
				var refElements = references.Select(t => t.Element).ToList();
				var privateRefElements =
						referenecesOfPrivateFields.Select(t => t.Element).ToList
								();
				Assert.That(elements, Is.EqualTo(propValues), message);
				Assert.That(refElements, Is.EqualTo(propValues), message);
				Assert.That(privateRefElements, Is.EqualTo(propValues), message);
			}
		}

		private static IEnumerable<UnifiedElement> GetProperties(
				UnifiedElement element) {
			var elements = element as IEnumerable<UnifiedElement>;
			if (elements != null) {
				foreach (var e in elements) {
					yield return e;
				}
			}
			var props = element.GetType().GetProperties()
					.Where(prop => prop.Name != "Parent")
					.Where(prop => prop.GetIndexParameters().Length == 0)
					.Where(
							prop =>
							typeof(UnifiedElement).IsAssignableFrom(
									prop.PropertyType));
			if (element is UnifiedWrapType) {
				props = props.Where(prop => prop.Name != "BasicTypeName");
			}
			foreach (var prop in props) {
				yield return (UnifiedElement)prop.GetValue(element, null);
			}
		}

		/// <summary>
		///   子要素とセッターの列挙機能が正常に動作するかソースーコードを指定して検査します。
		/// </summary>
		/// <param name="codeObject"> 検査対象のモデル </param>
		/// <param name="message"> アサーションに違反した際のエラーメッセージ </param>
		private static void AssertGetElementReferences(
				UnifiedProgram codeObject, string message) {
			codeObject = codeObject.DeepCopy();
			var elements = codeObject.Descendants().ToList();
			foreach (var element in elements) {
				var references = element.ElementReferences();
				foreach (var reference in references) {
					reference.Element = null;
				}
			}
			foreach (var element in elements) {
				foreach (var child in element.Elements()) {
					Assert.That(child, Is.Null, message);
				}
			}
		}

		/// <summary>
		///   子要素とプロパティを介さないセッターの列挙機能が正常に動作するかソースーコードを指定して検査します。
		/// </summary>
		/// <param name="codeObject"> 検査対象のモデル </param>
		/// <param name="message"> アサーションに違反した際のエラーメッセージ </param>
		private static void AssertGetElementReferenecesOfFields(
				UnifiedProgram codeObject, string message) {
			codeObject = codeObject.DeepCopy();
			var elements = codeObject.Descendants().ToList();
			foreach (var element in elements) {
				var references = element.ElementReferencesOfFields();
				foreach (var reference in references) {
					reference.Element = null;
				}
			}
			foreach (var element in elements) {
				foreach (var child in element.Elements()) {
					Assert.That(child, Is.Null, message);
				}
			}
		}

		/// <summary>
		///   親要素に不適切な要素がないかソースコードを指定して検査します。
		/// </summary>
		/// <param name="codeObject"> 検査対象のモデル </param>
		/// <param name="message"> アサーションに違反した際のエラーメッセージ </param>
		private static void AssertParentProperty(
				UnifiedElement codeObject, string message) {
			foreach (var element in codeObject.Elements()) {
				if (element != null) {
					Assert.That(element.Parent, Is.SameAs(codeObject), message);
					AssertParentProperty(element, message);
				}
			}
		}

		/// <summary>
		///   全要素の文字列情報を取得できるかソースコードを指定して検査します。
		/// </summary>
		/// <param name="codeObject"> 検査対象のモデル </param>
		/// <param name="message"> アサーションに違反した際のエラーメッセージ </param>
		private static void AssertToString(
				UnifiedProgram codeObject, string message) {
			foreach (var element in codeObject.DescendantsAndSelf()) {
				Assert.That(element.ToString(), Is.Not.Null, message);
			}
		}

		/// <summary>
		///   モデルを通した再生成したコードが変化しないか検査します。 元コード1→モデル1→コード2→モデル2→コード3→モデル3と再生成します。 モデル2とモデル3を比較します。
		/// </summary>
		/// <param name="orgCode"> 検査対象のソースコード </param>
		/// <param name="codeObject"> 検査対象のモデル </param>
		private void AssertEqualsModel(
				string orgCode, UnifiedProgram codeObject) {
			string code2 = null, code3 = null;
			try {
				code2 = Fixture.CodeGenerator.Generate(codeObject);
				var obj2 = Fixture.ProgramGenerator.Generate(code2);
				code3 = Fixture.CodeGenerator.Generate(obj2);
				var obj3 = Fixture.ProgramGenerator.Generate(code3);
				Assert.That(
						obj3,
						Is.EqualTo(obj2).Using(
								StructuralEqualityComparerForDebug.Instance));
			} catch (Exception e) {
				var outPath = FixtureUtil.GetOutputPath();
				File.WriteAllText(
						Path.Combine(outPath, "orgignal.txt"), orgCode);
				if (code2 != null) {
					File.WriteAllText(
							Path.Combine(outPath, "generate.txt"), code2);
				}
				if (code3 != null) {
					File.WriteAllText(
							Path.Combine(outPath, "regenerate.txt"), code3);
				}
				throw e;
			}
		}

		/// <summary>
		///   コンパイル結果を通して再生成したコードが変化しないかテストします。 元コード1→モデル1→コード2と再生成します。 コンパイルしたアセンブリファイルの逆コンパイル結果を通して、 元コード1とコード2を比較します。
		/// </summary>
		/// <param name="orgCode"> 検査対象のソースコード </param>
		/// <param name="fileName"> 再生成するソースコードのファイル名 </param>
		/// <param name="codeObject"> 検査対象のモデル </param>
		private void AssertEqualsCompiledCode(
				string orgCode, string fileName, UnifiedProgram codeObject) {
			// コンパイル用の作業ディレクトリの取得
			var workPath = FixtureUtil.CleanOutputAndGetOutputPath();
			// 作業ディレクトリ内にソースコードを配置
			var srcPath = FixtureUtil.GetOutputPath(fileName);
			File.WriteAllText(srcPath, orgCode, XEncoding.SJIS);
			try {
				// 作業ディレクトリ内でコンパイル
				Fixture.Compile(workPath, srcPath);
			} catch (Exception e) {
				throw new Exception("Fail to compile the original code:\n" + orgCode, e);
			}
			// コンパイル結果の取得
			var orgByteCode1 = Fixture.GetAllCompiledCode(workPath);
			// 再生成したソースコードを配置
			var code2 = Fixture.CodeGenerator.Generate(codeObject);
			File.WriteAllText(srcPath, code2, XEncoding.SJIS);
			const string line = "------------------------";
			try {
				// 再生成したソースコードのコンパイル結果の取得
				Fixture.Compile(workPath, srcPath);
			} catch (Exception e) {
				throw new Exception(
						"Fail to compile the regenerated code:\n" + line + "regenerated code" + line
						+ "\n" + code2
						+ line + "original code" + line + "\n" + orgCode, e);
			}
			var byteCode2 = Fixture.GetAllCompiledCode(workPath);
			try {
				AssertFuzzyEquals(byteCode2, orgByteCode1);
			} catch (Exception e) {
				throw new Exception(
						"Differencies exist:\n" + line + "regenerated code" + line + "\n" + code2
						+ line + "original code" + line + "\n" + orgCode, e);
			}
		}

		/// <summary>
		///   閾値（許容する不一致文字の数）を設けてバイトコード同士を比較します．
		/// </summary>
		/// <param name="actual"> </param>
		/// <param name="expected"> </param>
		/// <returns> </returns>
		private void AssertFuzzyEquals(
				IEnumerable<Tuple<string, object>> actual,
				IEnumerable<Tuple<string, object>> expected) {
			var actuals = actual.ToList();
			var expecteds = expected.ToList();
			Assert.That(actuals.Count, Is.EqualTo(expecteds.Count));
			for (int i = 0; i < actuals.Count; i++) {
				Assert.That(actuals[i].Item1, Is.EqualTo(expecteds[i].Item1));
				if (actuals[i].Item2 is byte[]) {
					Assert.That(
							AssertFuzzyEquals(
									(byte[])actuals[i].Item2,
									(byte[])expecteds[i].Item2), Is.True);
				} else {
					Assert.That(
							actuals[i].Item2, Is.EqualTo(expecteds[i].Item2));
				}
			}
		}

		/// <summary>
		///   閾値（許容する不一致文字の数）を設けてバイトコード同士を比較します．
		/// </summary>
		/// <param name="actual"> </param>
		/// <param name="expected"> </param>
		/// <returns> </returns>
		private bool AssertFuzzyEquals(byte[] actual, byte[] expected) {
			if (actual.Length != expected.Length) {
				return false;
			}
			var count = Fixture.AllowedMismatchCount;
			for (int i = 0; i < actual.Length; i++) {
				if (actual[i] != expected[i] && --count < 0) {
					return false;
				}
			}
			return true;
		}
	}
}
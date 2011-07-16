﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Paraiba.Text;
using Unicoen.Model;
using Unicoen.Languages.Java.CodeFactories;

namespace Unicoen.Apps.Aop.Cui.Tests.CodeProcessorTest {
	/// <summary>
	/// 変数参照/代入に関するアスペクトの合成が正しく機能するかテストする
	/// </summary>
	[TestFixture]
	public class GetSetWeavingTest {
		//指定されたパスのファイルを読み込んで共通コードオブジェクトに変換します
		public UnifiedProgram CreateModel(string path) {
			var ext = Path.GetExtension(path);
			var code = File.ReadAllText(path, XEncoding.SJIS);
			return CodeProcessor.CreateModel(ext, code);
		}
		
		[Test]
		public void Getポイントカットを用いて代入文の直前にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b; b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtBeforeGet(model, new Regex("a"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}

		[Test]
		public void Getポイントカットを用いて初期化子つき変数宣言の直前にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtBeforeGet(model, new Regex("a"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}

		[Test]
		public void Getポイントカットを用いて代入文の直後にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b; b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtAfterGet(model, new Regex("a"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}

		[Test]
		public void Getポイントカットを用いて初期化子つき変数宣言の直後にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtAfterGet(model, new Regex("a"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}

		[Test]
		public void Setポイントカットを用いて代入文の直前にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b; b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtBeforeSet(model, new Regex("b"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}

		[Test]
		public void Setポイントカットを用いて初期化子つき変数宣言の直前にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtBeforeSet(model, new Regex("b"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}

		[Test]
		public void Setポイントカットを用いて代入文の直後にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b; b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtAfterSet(model, new Regex("b"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}

		[Test]
		public void Setポイントカットを用いて初期化子つき変数宣言の直後にアスペクトを合成できる() {
			const string code = @"class A{ public void M() { int a = 10; int b = a; } }";
			//モデル化
			var model = CodeProcessor.CreateModel(".java", code);
			var beforeNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;
			//アスペクトの合成
			CodeProcessor.InsertAtAfterSet(model, new Regex("b"), CodeProcessor.CreateAdvice("Java", "System.out.println();"));
			var afterNumBlock = model.Descendants().Where<UnifiedBlock>().ToCollection().Count;

			//for debug
			var gen = new JavaCodeFactory();
			Console.Write(gen.Generate(model));

			//アスペクトが合成されるためブロックの数が1つ増える
			Assert.That(afterNumBlock, Is.EqualTo(beforeNumBlock + 1));
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Unicoen.Apps.RefactoringDSL.Util;
using Unicoen.Languages.Java;
using Unicoen.Model;
using Unicoen.Tests;
using Unicoen.Apps.RefactoringDSL;

namespace Unicoen.Apps.RefactoringDSL.Tests.NamespaceDetector {
	class NamespaceDetectorTest {
		private UnifiedProgram _model;

		[SetUp]
		public void SetUp() {
			var inputPath = FixtureUtil.GetInputPath("Java", "default", "Namespace.java");
			var code = File.ReadAllText(inputPath, Encoding.Default);
			_model = JavaFactory.GenerateModel(code);
		}

		[Test]
		public void Xml表示用() {
			Console.WriteLine(_model.ToXml());
		}
		[Test]
		public void パッケージ名など最上位名前空間を取得するテスト() {
			var package = GetPackageName(_model);
			Console.WriteLine(package);
		}

		[Test]
		public void クラスの名前空間を取得するテスト() {
			var targetClass = FindUtil.FindClassByClassName(_model, "Cls").First();
			Console.WriteLine(targetClass.ToXml());
		}

		public static Namespace GetPackageName(UnifiedProgram program, string delimiter = ".") {
			var package = new Namespace();
			var namespaceNodes = program.Descendants<UnifiedNamespaceDefinition>();
			// パッケージ指定がないとき
			if (namespaceNodes.Count() == 0) {
				return new Namespace() {
					Parent = null,
					Value = "",
					NamespaceType = NamespaceType.Package,
				};
			}
			var namespaceNode = namespaceNodes.First();
			if (namespaceNode.Descendants<UnifiedProperty>().Count() == 0) {
				return new Namespace() {
					Parent = null,
					Value = namespaceNode.Descendants<UnifiedVariableIdentifier>().First().Name,
					NamespaceType = NamespaceType.Package,
				};
			}
			var firstProperty = namespaceNode.Descendants<UnifiedProperty>().First();
			return new Namespace() {
				Parent = null,
				Value = string.Join(delimiter, firstProperty.Descendants<UnifiedVariableIdentifier>().Select(e => e.Name)),
				NamespaceType = NamespaceType.Package,
			};
		}

		public static Namespace GetNamespace(UnifiedNamespaceDefinition packageNode) {
			if (packageNode.Descendants<UnifiedProperty>().Count() == 0) {
				return new Namespace() {
					Parent = null,
					Value = packageNode.Descendants<UnifiedVariableIdentifier>().First().Name,
					NamespaceType = NamespaceType.Package,
				};
			}
			var firstProperty = packageNode.Descendants<UnifiedProperty>().First();
			var delimiter = ".";
			return new Namespace() {
				Parent = null,
				Value = string.Join(delimiter, firstProperty.Descendants<UnifiedVariableIdentifier>().Select(e => e.Name)),
				NamespaceType = NamespaceType.Package,
			};
			
		}

		public static Namespace GetNamespace(UnifiedClassDefinition classNode) {
			var type = NamespaceType.Class;
			var className = classNode.FirstDescendant<UnifiedVariableIdentifier>().Name;
			var parents = GetParentTypes(type).Select(t => Namespace2UnifiedType(t));
			var parentNode = GetFirstFoundNode(classNode, parents);

			return new Namespace() {
				Value = className,
				NamespaceType = type,
				Parent = Dispatcher(parentNode),
			};

		}

		[Test]
		public void TestForGetNamespace_class() {
			var targetClass = FindUtil.FindClassByClassName(_model, "Inner").First();
			var nsString = GetNamespace(targetClass).GetNamespaceString();
			Console.WriteLine(nsString);
		}

		public static Namespace GetNamespace(UnifiedFunctionDefinition functionNode) {
			var type = NamespaceType.Function;
			var functionName = functionNode.Name.Name;
			var parents = GetParentTypes(type).Select(t => Namespace2UnifiedType(t));
			var parentNode = GetFirstFoundNode(functionNode, parents);

			return new Namespace() {
				Value = functionName,
				NamespaceType = type,
				Parent = Dispatcher(parentNode),
			};
		}

		[Test]
		public void TestForGetNamespace_function() {
			var targetClass = FindUtil.FindClassByClassName(_model, "Cls").First();
			var functionNode = targetClass.FirstDescendant<UnifiedFunctionDefinition>();
			var nsString = GetNamespace(functionNode).GetNamespaceString();
			Console.WriteLine(nsString);
		}

		public static Namespace GetNamespace(UnifiedVariableDefinition variableNode) {
			var type = NamespaceType.Variable;
			var variableName = variableNode.Name.Name;
			var parents = GetParentTypes(type).Select(t => Namespace2UnifiedType(t));
			var parentNode = GetFirstFoundNode(variableNode, parents);

			return new Namespace() {
				Value = variableName,
				NamespaceType = type,
				Parent = Dispatcher(parentNode),
			};
		}

		[Test]
		public void TestForGetNamespace_variabel() {
			var targetClass = FindUtil.FindClassByClassName(_model, "Cls").First();
			var functionNode = targetClass.FirstDescendant<UnifiedVariableDefinition>();
			var nsString = GetNamespace(functionNode).GetNamespaceString();
			Console.WriteLine(nsString);
		}


		public static Namespace Dispatcher(IUnifiedElement element) {
			if(element is UnifiedClassDefinition) {
				return GetNamespace(element as UnifiedClassDefinition);
			}
			if(element is UnifiedNamespaceDefinition) {
				return GetNamespace(element as UnifiedNamespaceDefinition);
			}

			return null;
		}

		public static IUnifiedElement GetFirstFoundNode(UnifiedElement node, IEnumerable<Type> type) {
			foreach (var ancestor in node.Ancestors()) {
				foreach (var t in type) {
					if(ancestor.GetType().Equals(t)) {
						return ancestor;
					}
				}
			}

			return null;

		}

		// 自分の親になりうる，かつ，名前空間構成要素になり得る要素タイプを返却する
		public static NamespaceType[] GetParentTypes(NamespaceType type) {
			switch(type) {
				case NamespaceType.Package:
					return new NamespaceType[] { };
				case NamespaceType.Class:
					return new NamespaceType[] { NamespaceType.Class, NamespaceType.Package };
				case NamespaceType.Function:
					return new NamespaceType[] { NamespaceType.Class, NamespaceType.Package, NamespaceType.Function};
				case NamespaceType.Variable:
					return new NamespaceType[] {NamespaceType.Class, NamespaceType.Function};
				default:
					throw new InvalidOperationException();

			}
		}

		// タイプから型へ変換（上へトラバースするときに使う）
		public static Type Namespace2UnifiedType(NamespaceType type) {
			switch(type) {
				case NamespaceType.Package:
					return UnifiedNamespaceDefinition.Create().GetType();
				case NamespaceType.Class:
					return UnifiedClassDefinition.Create().GetType();
				case NamespaceType.Function:
					return UnifiedFunctionDefinition.Create().GetType();
				default:
					throw new InvalidOperationException();

			}
			
		}
	}

	// 名前空間オブジェクトを表すクラス
	public class Namespace {
		public Namespace Parent { get; set; }
		public string Value { get; set; }
		public NamespaceType NamespaceType { get; set; }

		public override string ToString() {
			return Value;
		}

		public override bool Equals(object obj) {
			if (obj is Namespace) {
				return this.GetNamespaceString().Equals(((Namespace)obj).GetNamespaceString());
			} else {
				return false;
			}
		}

		// 自分を含めて親を全部たどって，名前空間文字列くっつける
		public string GetNamespaceString(string originalDelimiter = ".") {
			var delimiter = "";
			;
			var ns = "";
			var node = this;
			while(node != null) {
				ns = node.Value + delimiter + ns;
				node = node.Parent;
				delimiter = originalDelimiter;
			}

			return ns;
		}
	}


	// 名前空間を作りうる言語要素
	public enum NamespaceType {
		Package, Class, Function, Variable,
	}

}

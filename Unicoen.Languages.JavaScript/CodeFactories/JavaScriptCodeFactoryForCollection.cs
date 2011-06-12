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
using Unicoen.Core.Model;
using Unicoen.Core.Processor;

namespace Unicoen.Languages.JavaScript.CodeFactories {
	public partial class JavaScriptCodeFactory {
		public void VisitCollection<T, TSelf>(
				UnifiedElementCollection<T, TSelf> elements, VisitorArgument arg)
				where T : class, IUnifiedElement
				where TSelf : UnifiedElementCollection<T, TSelf> {
			var decoration = arg.Decoration;
			arg.Write(decoration.MostLeft);
			var splitter = "";
			foreach (var e in elements) {
				arg.Write(splitter);
				arg.Write(decoration.EachLeft);
				e.TryAccept(this, arg);
				arg.Write(decoration.EachRight);
				splitter = decoration.Delimiter;
			}
			arg.Write(decoration.MostRight);
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedArgumentCollection element, VisitorArgument arg) {
			VisitCollection(element, arg);
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedParameterCollection element, VisitorArgument arg) {
			VisitCollection(element, arg.Set(Paren));
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedModifierCollection element, VisitorArgument arg) {
			//JavaScriptでは修飾子は出現しない
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedExpressionCollection element, VisitorArgument arg) {
			//現在は使用していない
			throw new InvalidOperationException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedCaseCollection element, VisitorArgument arg) {
			arg = arg.IncrementDepth();
			foreach (var caseElement in element) {
				arg.WriteIndent();
				caseElement.TryAccept(this, arg);
			}
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedTypeCollection element, VisitorArgument arg) {
			//JavaScriptでは型の列挙は出現しない
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedTypeParameterCollection element, VisitorArgument arg) {
			//JavaScriptでは型パラメータは出現しない
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedTypeSupplementCollection element, VisitorArgument arg) {
			//JavaScriptでは型宣言時に'[]'は出現しない
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedIdentifierCollection element, VisitorArgument arg) {
			VisitCollection(element, arg.Set(CommaDelimiter));
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedMatcherCollection element, VisitorArgument arg) {
			VisitCollection(element, arg);
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedKeyValueCollection element, VisitorArgument arg) {
			VisitCollection(element, arg);
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedAnnotation element, VisitorArgument arg) {
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedAnnotationCollection element, VisitorArgument arg) {
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedVariableDefinitionList element, VisitorArgument arg) {
			VisitCollection(element, arg.Set(SemiColonDelimiter));
			return true;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedVariableDefinition element, VisitorArgument arg) {
			arg.Write("var ");
			element.Name.TryAccept(this, arg);
			if (element.InitialValue != null) {
				arg.Write(" = ");
				element.InitialValue.TryAccept(this, arg.Set(Bracket));
			}
			element.Arguments.TryAccept(this, arg.Set(Paren));
			element.Body.TryAccept(this, arg.Set(ForBlock));
			return false;
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedSupplementType element, VisitorArgument arg) {
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedGenericType element, VisitorArgument arg) {
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedSimpleType element, VisitorArgument arg) {
			throw new NotImplementedException();
		}

		bool IUnifiedModelVisitor<VisitorArgument, bool>.Visit(
				UnifiedArrayType element, VisitorArgument arg) {
			throw new NotImplementedException();
		}
	}
}
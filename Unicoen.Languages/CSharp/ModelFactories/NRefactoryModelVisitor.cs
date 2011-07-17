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
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;
using Unicoen.Model;
using Attribute = ICSharpCode.NRefactory.CSharp.Attribute;

namespace Unicoen.Languages.CSharp.ModelFactories {

	internal partial class NRefactoryModelVisitor : IAstVisitor<object, IUnifiedElement> {

		public IUnifiedElement VisitCompilationUnit(CompilationUnit unit, object data) {
			var prog = UnifiedProgram.Create(UnifiedBlock.Create());
			foreach (var child in unit.Children) {
				var elem = child.TryAcceptForExpression(this);
				if (elem != null)
					prog.Body.Add(elem);
			}
			return prog;
		}

		public IUnifiedElement VisitAnonymousMethodExpression(AnonymousMethodExpression expr, object data) {
			var parameters = expr.Parameters
					.Select(p => p.AcceptVisitor(this, data) as UnifiedParameter)
					.ToCollection();
			var body = expr.Body.TryAcceptForExpression(this).ToBlock();
			return UnifiedLambda.Create(parameters: parameters, body: body);
		}

		public IUnifiedElement VisitUndocumentedExpression(UndocumentedExpression expr, object data) {
			throw new NotImplementedException("UndocumentedExpression");
		}

		public IUnifiedElement VisitArrayCreateExpression(ArrayCreateExpression array, object data) {
			var type = LookupType(array.Type);
			var uArgs = array.Arguments
					.Select(nExpr => nExpr.TryAcceptForExpression(this))
					.Select(uExpr => UnifiedArgument.Create(value: uExpr))
					.ToCollection();
			var initValues = null as UnifiedArrayLiteral;
			if (array.Initializer != null) {
				initValues = array.Initializer.AcceptVisitor(this, data) as UnifiedArrayLiteral;
			}
			return UnifiedNew.Create(type.WrapRectangleArray(uArgs), initialValues: initValues);
		}

		public IUnifiedElement VisitArrayInitializerExpression(ArrayInitializerExpression arrayInit, object data) {
			return arrayInit.Elements
					.Select(e => e.TryAcceptForExpression(this))
					.ToArrayLiteral();
		}

		public IUnifiedElement VisitAsExpression(AsExpression asExpr, object data) {
			var op = UnifiedBinaryOperator.Create("as", UnifiedBinaryOperatorKind.As);
			var value = asExpr.Expression.TryAcceptForExpression(this);
			var type = LookupType(asExpr.Type);
			return UnifiedBinaryExpression.Create(value, op, type);
		}

		public IUnifiedElement VisitAssignmentExpression(AssignmentExpression assign, object data) {
			var op = UnifiedBinaryOperator.Create("=", UnifiedBinaryOperatorKind.Assign);
			var left = assign.Left.TryAcceptForExpression(this);
			var right = assign.Right.TryAcceptForExpression(this);
			return UnifiedBinaryExpression.Create(left, op, right);
		}

		public IUnifiedElement VisitBaseReferenceExpression(BaseReferenceExpression expr, object data) {
			return UnifiedSuperIdentifier.Create("base");
		}

		public IUnifiedElement VisitBinaryOperatorExpression(BinaryOperatorExpression expr, object data) {
			var op = LookupBinaryOperator(expr.Operator);
			var left = expr.Left.TryAcceptForExpression(this);
			var right = expr.Right.TryAcceptForExpression(this);
			return UnifiedBinaryExpression.Create(left, op, right);
		}

		public IUnifiedElement VisitCastExpression(CastExpression expr, object data) {
			var type = LookupType(expr.Type);
			var elem = expr.Expression.TryAcceptForExpression(this);
			return UnifiedCast.Create(type, elem);
		}

		public IUnifiedElement VisitCheckedExpression(
				CheckedExpression checkedExpression, object data) {
			throw new NotImplementedException("CheckedExpression");
		}

		public IUnifiedElement VisitConditionalExpression(ConditionalExpression expr, object data) {
			var cond = expr.Condition.TryAcceptForExpression(this);
			var former = expr.TrueExpression.TryAcceptForExpression(this);
			var latter = expr.FalseExpression.TryAcceptForExpression(this);
			return UnifiedTernaryExpression.Create(cond, former, latter);
		}

		public IUnifiedElement VisitDefaultValueExpression(DefaultValueExpression expr, object data) {
			var type = LookupType(expr.Type);
			return UnifiedDefault.Create(type);
		}

		public IUnifiedElement VisitDirectionExpression(DirectionExpression expr, object data) {
			var mods = LookupModifier(expr.FieldDirection).ToCollection();
			var value = expr.Expression.TryAcceptForExpression(this);
			return UnifiedArgument.Create(mods, null /* no target*/, value);
		}

		public IUnifiedElement VisitIdentifierExpression(IdentifierExpression ident, object data) {
			return UnifiedVariableIdentifier.Create(ident.Identifier);
		}

		public IUnifiedElement VisitIndexerExpression(IndexerExpression indexer, object data) {
			var target = indexer.Target.TryAcceptForExpression(this);
			var args =
					from arg in indexer.Arguments
					let uExpr = arg.TryAcceptForExpression(this)
					where uExpr != null
					select UnifiedArgument.Create(value: uExpr);
			return UnifiedIndexer.Create(target, args.ToCollection());
		}

		public IUnifiedElement VisitInvocationExpression(InvocationExpression invoc, object data) {
			var target = invoc.Target.TryAcceptForExpression(this);
			var uArgs = UnifiedArgumentCollection.Create();
			foreach (var arg in invoc.Arguments) {
				var value = arg.AcceptVisitor(this, data);
				var uArg = value as UnifiedArgument;
				if (uArg != null) {
					uArgs.Add(uArg);
				}
				else {
					var uExpr = value as IUnifiedExpression;
					if (uExpr != null)
						uArgs.Add(UnifiedArgument.Create(value: uExpr));
				}
			}
			return UnifiedCall.Create(target, uArgs);
		}

		public IUnifiedElement VisitIsExpression(IsExpression expr, object data) {
			var op = UnifiedBinaryOperator.Create("is", UnifiedBinaryOperatorKind.InstanceOf);
			var value = expr.Expression.TryAcceptForExpression(this);
			var type = LookupType(expr.Type);
			return UnifiedBinaryExpression.Create(value, op, type);
		}

		public IUnifiedElement VisitLambdaExpression(LambdaExpression lambda, object data) {
			var parameters = lambda.Parameters
					.Select(p => p.AcceptVisitor(this, data) as UnifiedParameter)
					.ToCollection();
			var body = lambda.Body.TryAcceptForExpression(this).ToBlock();
			return UnifiedLambda.Create(parameters: parameters, body: body);
		}

		public IUnifiedElement VisitMemberReferenceExpression(MemberReferenceExpression propExpr, object data) {
			var target = propExpr.Target.TryAcceptForExpression(this);
			var name = propExpr.MemberName.ToVariableIdentifier();
			return UnifiedProperty.Create(".", target, name);
		}

		public IUnifiedElement VisitNamedArgumentExpression(NamedArgumentExpression expr, object data) {
			var name = UnifiedVariableIdentifier.Create(expr.Identifier);
			var value = expr.Expression.TryAcceptForExpression(this);
			return UnifiedArgument.Create(target: name, value: value);
		}

		public IUnifiedElement VisitNullReferenceExpression(NullReferenceExpression expr, object data) {
			return UnifiedNullLiteral.Create();
		}

		public IUnifiedElement VisitObjectCreateExpression(ObjectCreateExpression create, object data) {
			var uType = LookupType(create.Type);
			var args =
					from arg in create.Arguments
					let value = arg.TryAcceptForExpression(this)
					select UnifiedArgument.Create(value: value);
			return UnifiedNew.Create(uType, args.ToCollection());
		}

		public IUnifiedElement VisitAnonymousTypeCreateExpression(
				AnonymousTypeCreateExpression anonymousTypeCreateExpression, object data) {
			throw new NotImplementedException("AnonymousTypeCreateExpression");
		}

		public IUnifiedElement VisitParenthesizedExpression(ParenthesizedExpression expr, object data) {
			return expr.Expression.AcceptVisitor(this, data);
		}

		public IUnifiedElement VisitPointerReferenceExpression(
				PointerReferenceExpression pointerReferenceExpression, object data) {
			throw new NotImplementedException("PointerReferenceExpression");
		}

		public IUnifiedElement VisitPrimitiveExpression(PrimitiveExpression prim, object data) {
			if (prim.Value == null) {
				return UnifiedNullLiteral.Create();
			}
			if (prim.Value is bool) {
				return UnifiedBooleanLiteral.Create((bool)prim.Value);
			}
			if (prim.Value is int) {
				return UnifiedIntegerLiteral.CreateInt32((int)prim.Value);
			}
			if (prim.Value is UInt64) {
				return UnifiedIntegerLiteral.CreateUInt64((UInt64)prim.Value);
			}
			if (prim.Value is char) {
				return UnifiedCharLiteral.Create(((char)prim.Value).ToString());
			}
			if (prim.Value is string) {
				return UnifiedStringLiteral.Create(prim.LiteralValue);
			}
			throw new NotImplementedException("value type is " + prim.Value.GetType());
		}

		public IUnifiedElement VisitSizeOfExpression(
				SizeOfExpression sizeOfExpression, object data) {
			throw new NotImplementedException("SizeOfExpression");
		}

		public IUnifiedElement VisitStackAllocExpression(
				StackAllocExpression stackAllocExpression, object data) {
			throw new NotImplementedException("StackAllocExpression");
		}

		public IUnifiedElement VisitThisReferenceExpression(ThisReferenceExpression expr, object data) {
			return UnifiedThisIdentifier.Create("this");
		}

		public IUnifiedElement VisitTypeOfExpression(TypeOfExpression expr, object data) {
			var type = LookupType(expr.Type);
			return UnifiedTypeof.Create(type);
		}

		public IUnifiedElement VisitTypeReferenceExpression(TypeReferenceExpression expr, object data) {
			return LookupType(expr.Type);
		}

		public IUnifiedElement VisitUnaryOperatorExpression(UnaryOperatorExpression unary, object data) {
			var op = LookupUnaryOperator(unary.Operator);
			var operand = unary.Expression.TryAcceptForExpression(this);
			return UnifiedUnaryExpression.Create(operand, op);
		}

		public IUnifiedElement VisitUncheckedExpression(
				UncheckedExpression uncheckedExpression, object data) {
			throw new NotImplementedException("UncheckedExpression");
		}

		public IUnifiedElement VisitEmptyExpression(EmptyExpression empty, object data) {
			return null;
		}

		public IUnifiedElement VisitQueryExpression(
				QueryExpression queryExpression, object data) {
			throw new NotImplementedException("QueryExpression");
		}

		public IUnifiedElement VisitQueryContinuationClause(
				QueryContinuationClause queryContinuationClause, object data) {
			throw new NotImplementedException("QueryContinuationClause");
		}

		public IUnifiedElement VisitQueryFromClause(
				QueryFromClause queryFromClause, object data) {
			throw new NotImplementedException("QueryFromClause");
		}

		public IUnifiedElement VisitQueryLetClause(
				QueryLetClause queryLetClause, object data) {
			throw new NotImplementedException("QueryLetClause");
		}

		public IUnifiedElement VisitQueryWhereClause(
				QueryWhereClause queryWhereClause, object data) {
			throw new NotImplementedException("QueryWhereClause");
		}

		public IUnifiedElement VisitQueryJoinClause(
				QueryJoinClause queryJoinClause, object data) {
			throw new NotImplementedException("QueryJoinClause");
		}

		public IUnifiedElement VisitQueryOrderClause(
				QueryOrderClause queryOrderClause, object data) {
			throw new NotImplementedException("QueryOrderClause");
		}

		public IUnifiedElement VisitQueryOrdering(
				QueryOrdering queryOrdering, object data) {
			throw new NotImplementedException("QueryOrdering");
		}

		public IUnifiedElement VisitQuerySelectClause(
				QuerySelectClause querySelectClause, object data) {
			throw new NotImplementedException("QuerySelectClause");
		}

		public IUnifiedElement VisitQueryGroupClause(
				QueryGroupClause queryGroupClause, object data) {
			throw new NotImplementedException("QueryGroupClause");
		}

		public IUnifiedElement VisitAttribute(Attribute attribute, object data) {
			var type = LookupType(attribute.Type);
			if (attribute.HasArgumentList == false)
				return UnifiedAnnotation.Create(type);

			var uArgs = UnifiedArgumentCollection.Create();
			foreach(var nArg in attribute.Arguments) {
				var uElem = nArg.AcceptVisitor(this, data);
				var uArg = uElem as UnifiedArgument;
				if (uArg != null) {
					uArgs.Add(uArg);
					continue;
				}
				var uExpr = uElem as IUnifiedExpression;
				if (uExpr != null) {
					uArgs.Add(UnifiedArgument.Create(value: uExpr));
				}
			}
			return UnifiedAnnotation.Create(type, uArgs);
		}

		public IUnifiedElement VisitAttributeSection(AttributeSection attrSec, object data) {
			// TODO: AttributeTarget
			return attrSec.Attributes
					.Select(a => a.AcceptVisitor(this, data))
					.OfType<UnifiedAnnotation>()
					.ToCollection();
		}

		public IUnifiedElement VisitDelegateDeclaration(
				DelegateDeclaration delegateDeclaration, object data) {
			throw new NotImplementedException("DelegateDeclaration");
		}

		public IUnifiedElement VisitNamespaceDeclaration(NamespaceDeclaration dec, object data) {
			var ns = dec.Identifiers
					.Select(ident => ident.Name.ToVariableIdentifier() as IUnifiedExpression)
					.Aggregate((left, right) => UnifiedProperty.Create(".", left, right));
			var body = dec.Members
					.Select(mem => mem.TryAcceptForExpression(this))
					.ToBlock();
			return UnifiedNamespaceDefinition.Create(name: ns, body: body);
		}

		public IUnifiedElement VisitTypeDeclaration(TypeDeclaration dec, object data) {
			var attrs = dec.Attributes.AcceptVisitor(this, data);
			var mods = LookupModifiers(dec.Modifiers);
			var name = UnifiedVariableIdentifier.Create(dec.Name);
			var body = UnifiedBlock.Create();
			foreach (var node in dec.Members) {
				var uExpr = node.TryAcceptForExpression(this);
				if (uExpr != null)
					body.Add(uExpr);
			}
			// TODO: Attribute and Generics
			switch (dec.ClassType) {
			case ClassType.Class:
				return UnifiedClassDefinition.Create(attrs, mods, name, body: body);
			case ClassType.Struct:
				return UnifiedStructDefinition.Create(attrs, mods, name, body: body);
			case ClassType.Interface:
				return UnifiedInterfaceDefinition.Create(attrs, mods, name, body: body);
			case ClassType.Enum:
				return UnifiedEnumDefinition.Create(attrs, mods, name, body: body);
			}
			var msg = "LookupClassKind : " + dec.ClassType + "には対応していません。";
			throw new InvalidOperationException(msg);
		}

		public IUnifiedElement VisitUsingAliasDeclaration(UsingAliasDeclaration dec, object data) {
			var name = dec.Alias;
			var import = dec.Import.TryAcceptForExpression(this);
			return UnifiedImport.Create(import, name);
		}

		public IUnifiedElement VisitUsingDeclaration(UsingDeclaration dec, object data) {
			var target = LookupType(dec.Import);
			return UnifiedImport.Create(target);
		}

		public IUnifiedElement VisitExternAliasDeclaration(
				ExternAliasDeclaration externAliasDeclaration, object data) {
			throw new NotImplementedException("ExternAliasDeclaration");
		}

		public IUnifiedElement VisitBlockStatement(BlockStatement block, object data) {
			return block.Statements
					.Select(stmt => stmt.TryAcceptForExpression(this))
					.Where(stmt => stmt != null)
					.ToBlock();
		}

		public IUnifiedElement VisitBreakStatement(BreakStatement stmt, object data) {
			return UnifiedBreak.Create();
		}

		public IUnifiedElement VisitCheckedStatement(
				CheckedStatement checkedStatement, object data) {
			throw new NotImplementedException("CheckedStatement");
		}

		public IUnifiedElement VisitContinueStatement(ContinueStatement stmt, object data) {
			return UnifiedContinue.Create();
		}

		public IUnifiedElement VisitDoWhileStatement(DoWhileStatement stmt, object data) {
			var body = stmt.EmbeddedStatement.TryAcceptForExpression(this).ToBlock();
			var cond = stmt.Condition.TryAcceptForExpression(this);
			return UnifiedDoWhile.Create(cond, body);
		}

		public IUnifiedElement VisitEmptyStatement(EmptyStatement stmt, object data) {
			return UnifiedBlock.Create();
		}

		public IUnifiedElement VisitExpressionStatement(ExpressionStatement exprStmt, object data) {
			return exprStmt.Expression.TryAcceptForExpression(this);
		}

		public IUnifiedElement VisitFixedStatement(
				FixedStatement fixedStatement, object data) {
			throw new NotImplementedException("FixedStatement");
		}

		public IUnifiedElement VisitForeachStatement(ForeachStatement stmt, object data) {
			var type = LookupType(stmt.VariableType);
			var name = stmt.VariableName.ToVariableIdentifier();
			var set = stmt.InExpression.TryAcceptForExpression(this);
			var body = stmt.EmbeddedStatement.TryAcceptForExpression(this).ToBlock();

			var varDec = UnifiedVariableDefinition.Create(type: type, name: name);
			return UnifiedForeach.Create(varDec.ToVariableDefinitionList(), set, body);
		}

		public IUnifiedElement VisitForStatement(ForStatement forStmt, object data) {
			// C#はstatementは一つのStatementあるいはBlockなためFirstOrDefaultで問題ない。
			// 複数あるのはVBを表す場合のため。
			var initStmt = forStmt.Initializers
					.Select(s => s.TryAcceptForExpression(this))
					.FirstOrDefault();
			var condExpr = forStmt.Condition.TryAcceptForExpression(this);
			var stepStmt = forStmt.Iterators
					.Select(s => s.TryAcceptForExpression(this))
					.FirstOrDefault();
			var body = forStmt.EmbeddedStatement.TryAcceptForExpression(this).ToBlock();
			return UnifiedFor.Create(initStmt, condExpr, stepStmt, body);
		}

		public IUnifiedElement VisitGotoCaseStatement(
				GotoCaseStatement gotoCaseStatement, object data) {
			throw new NotImplementedException("GotoCaseStatement");
		}

		public IUnifiedElement VisitGotoDefaultStatement(
				GotoDefaultStatement gotoDefaultStatement, object data) {
			throw new NotImplementedException("GotoDefaultStatement");
		}

		public IUnifiedElement VisitGotoStatement(
				GotoStatement gotoStatement, object data) {
			return UnifiedGoto.Create(gotoStatement.Label);
		}

		public IUnifiedElement VisitIfElseStatement(IfElseStatement stmt, object data) {
			var cond = stmt.Condition.TryAcceptForExpression(this);
			var trueBlock = stmt.TrueStatement.TryAcceptForExpression(this).ToBlock();

			var nElseStmt = stmt.FalseStatement;
			if (nElseStmt == null) {
				return UnifiedIf.Create(cond, trueBlock);
			}
			else {
				var falseBlock = nElseStmt.TryAcceptForExpression(this).ToBlock();
				return UnifiedIf.Create(cond, trueBlock, falseBlock);
			}
		}

		public IUnifiedElement VisitLabelStatement(LabelStatement label, object data) {
			return UnifiedLabelIdentifier.Create(label.Label);
		}

		public IUnifiedElement VisitLockStatement(
				LockStatement lockStatement, object data) {
			throw new NotImplementedException("LockStatement");
		}

		public IUnifiedElement VisitReturnStatement(ReturnStatement retStmt, object data) {
			var nExpr = retStmt.Expression;
			if (nExpr == null)
				return UnifiedReturn.Create();
			var uExpr = nExpr.TryAcceptForExpression(this);
			return UnifiedReturn.Create(uExpr);
		}

		public IUnifiedElement VisitSwitchStatement(SwitchStatement stmt, object data) {
			var uExpr = stmt.Expression.TryAcceptForExpression(this);
			var caseCollection = UnifiedCaseCollection.Create();
			foreach (var sec in stmt.SwitchSections) {
				var body = sec.Statements
						.Select(s => s.TryAcceptForExpression(this))
						.ToBlock();
				var lastIx = sec.CaseLabels.Count - 1;
				Func<IUnifiedExpression, int, UnifiedCase> func = (expr, ix) => {
					return (ix == lastIx)
						? UnifiedCase.Create(expr, body)
						: UnifiedCase.Create(expr);
				};
				var cases = sec.CaseLabels
						.Select(lbl => lbl.Expression.TryAcceptForExpression(this))
						.Select(func);
				foreach (var c in cases)
					caseCollection.Add(c);
			}
			return UnifiedSwitch.Create(uExpr, caseCollection);
		}

		public IUnifiedElement VisitSwitchSection(
				SwitchSection switchSection, object data) {
			throw new NotImplementedException("SwitchSection");
		}

		public IUnifiedElement VisitCaseLabel(CaseLabel caseLabel, object data) {
			throw new NotImplementedException("CaseLabel");
		}

		public IUnifiedElement VisitThrowStatement(ThrowStatement stmt, object data) {
			var uExpr = stmt.Expression.TryAcceptForExpression(this);
			return UnifiedThrow.Create(uExpr);
		}

		public IUnifiedElement VisitTryCatchStatement(
				TryCatchStatement tryCatchStatement, object data) {

			var uTry = tryCatchStatement.TryBlock.TryAcceptForExpression(this).ToBlock();
			var uCatchs = tryCatchStatement
					.CatchClauses
					.Select(c => c.AcceptVisitor(this, data))
					.Cast<UnifiedCatch>()
					.ToCollection();
			var nFinally = tryCatchStatement.FinallyBlock;
			var uFinally = nFinally == null
								? null
								: nFinally.TryAcceptForExpression(this).ToBlock();
			return UnifiedTry.Create(uTry, uCatchs, /* else */null, uFinally);
		}

		public IUnifiedElement VisitCatchClause(CatchClause catchClause, object data) {
			var type = LookupType(catchClause.Type);
			var name = UnifiedVariableIdentifier.Create(catchClause.VariableName);
			var uMatcher = UnifiedMatcher.Create(matcher: type, assign: name);
			var body = catchClause.Body.TryAcceptForExpression(this).ToBlock();
			return UnifiedCatch.Create(uMatcher.ToCollection(), body);
		}

		public IUnifiedElement VisitUncheckedStatement(
				UncheckedStatement uncheckedStatement, object data) {
			throw new NotImplementedException("UncheckedStatement");
		}

		public IUnifiedElement VisitUnsafeStatement(
				UnsafeStatement unsafeStatement, object data) {
			throw new NotImplementedException("UnsafeStatement");
		}

		public IUnifiedElement VisitUsingStatement(UsingStatement stmt, object data) {
			stmt.ResourceAcquisition.AcceptVisitor(this, data);
			stmt.EmbeddedStatement.AcceptVisitor(this, data);
			// TODO: implement
			//return null;
			throw new NotImplementedException("VisitUsingStatement");
		}

		public IUnifiedElement VisitVariableDeclarationStatement(VariableDeclarationStatement dec, object data) {
			var uType = LookupType(dec.Type);
			var uMods = LookupModifiers(dec.Modifiers);
			var variables =
					from nVar in dec.Variables
					let name = nVar.Name
					let nInitValue = nVar.Initializer
					let uInitValue = nInitValue.TryAcceptForExpression(this)
					select UnifiedVariableDefinition.Create(
							type: uType.DeepCopy(),
							modifiers: uMods.DeepCopy(),
							name: name.ToVariableIdentifier(),
							initialValue: uInitValue);
			return variables.ToVariableDefinitionList();
		}

		public IUnifiedElement VisitWhileStatement(WhileStatement stmt, object data) {
			var cond = stmt.Condition.TryAcceptForExpression(this);
			var body = stmt.EmbeddedStatement.TryAcceptForExpression(this).ToBlock();
			return UnifiedWhile.Create(cond, body);
		}

		public IUnifiedElement VisitYieldBreakStatement(
				YieldBreakStatement yieldBreakStatement, object data) {
			throw new NotImplementedException("YieldBreakStatement");
		}

		public IUnifiedElement VisitYieldStatement(YieldStatement stmt, object data) {
			if (stmt.Expression == null) throw new NotImplementedException("YieldStatement");

			var value = stmt.Expression.TryAcceptForExpression(this);
			return UnifiedYieldReturn.Create(value);
		}

		public IUnifiedElement VisitAccessor(Accessor accessor, object data) {
			var block = accessor.Body.TryAcceptForExpression(this).ToBlock();
			var mods = LookupModifiers(accessor.Modifiers);

			var body = new UnifiedPropertyBody();
			body.Body = block;
			body.Modifiers = mods;
			return body;
		}

		public IUnifiedElement VisitConstructorDeclaration(ConstructorDeclaration ctorDec, object data) {
			var uMods = LookupModifiers(ctorDec.Modifiers);
			var uBody = ctorDec.Body.AcceptVisitor(this, data) as UnifiedBlock;
			var uParms = UnifiedParameterCollection.Create();
			foreach (var param in ctorDec.Parameters) {
				var type = LookupType(param.Type);
				var names = UnifiedVariableIdentifier.Create(param.Name).ToCollection();
				uParms.Add(UnifiedParameter.Create(type: type, names: names));
			}

			return UnifiedConstructor.Create(uBody, modifiers: uMods, parameters: uParms);
		}

		public IUnifiedElement VisitConstructorInitializer(
				ConstructorInitializer constructorInitializer, object data) {
			throw new NotImplementedException("ConstructorInitializer");
		}

		public IUnifiedElement VisitDestructorDeclaration(
				DestructorDeclaration destructorDeclaration, object data) {
			throw new NotImplementedException("DestructorDeclaration");
		}

		public IUnifiedElement VisitEnumMemberDeclaration(EnumMemberDeclaration dec, object data) {
			var name = UnifiedVariableIdentifier.Create(dec.Name);
			var value = dec.Initializer.TryAcceptForExpression(this);
			return UnifiedVariableDefinition.Create(name: name, initialValue: value);
		}

		public IUnifiedElement VisitEventDeclaration(
				EventDeclaration eventDeclaration, object data) {
			throw new NotImplementedException("EventDeclaration");
		}

		public IUnifiedElement VisitCustomEventDeclaration(
				CustomEventDeclaration customEventDeclaration, object data) {
			throw new NotImplementedException("CustomEventDeclaration");
		}

		public IUnifiedElement VisitFieldDeclaration(FieldDeclaration dec, object data) {
			var uType = LookupType(dec.ReturnType);
			var uMods = LookupModifiers(dec.Modifiers);
			var definitions =
					from nVar in dec.Variables
					let name = nVar.Name
					let nInitValue = nVar.Initializer
					let uInitValue = nInitValue.TryAcceptForExpression(this)
					select UnifiedVariableDefinition.Create(
							type: uType.DeepCopy(),
							modifiers: uMods.DeepCopy(),
							name: name.ToVariableIdentifier(),
							initialValue: uInitValue);
			return definitions.ToVariableDefinitionList();
		}

		public IUnifiedElement VisitIndexerDeclaration(IndexerDeclaration dec, object data) {
			// TODO: implementation
			//return null;
			throw new NotImplementedException("IndexerDeclaration");
		}

		public IUnifiedElement VisitMethodDeclaration(MethodDeclaration dec, object data) {
			var attrs = dec.Attributes
					.Select(attr => attr.AcceptVisitor(this, data) as UnifiedAnnotation)
					.ToCollection();
			var mods = LookupModifiers(dec.Modifiers);
			var type = LookupType(dec.ReturnType);
			var name = UnifiedVariableIdentifier.Create(dec.Name);
			var generics = dec.TypeParameters
					.Select(t => t.AcceptVisitor(this, data) as UnifiedGenericParameter)
					.ToCollection();
			if (generics.IsEmptyOrNull()) {
				generics = null;
			}
			var parameters = dec.Parameters
					.Select(p => p.AcceptVisitor(this, data))
					.OfType<UnifiedParameter>()
					.ToCollection();
			var body = UnifiedBlock.Create();
			foreach (var node in dec.Body) {
				var uExpr = node.TryAcceptForExpression(this);
				if (uExpr != null)
					body.Add(uExpr);
			}
			return UnifiedFunctionDefinition.Create(
					attrs, mods, type, generics, name, parameters, /* C# dont have throws */ null, body);
		}

		public IUnifiedElement VisitOperatorDeclaration(
				OperatorDeclaration operatorDeclaration, object data) {
			throw new NotImplementedException("OperatorDeclaration");
		}

		public IUnifiedElement VisitParameterDeclaration(ParameterDeclaration dec, object data) {
			var attrs = dec.Attributes
					.Select(attr => attr.AcceptVisitor(this, data))
					.OfType<UnifiedAnnotation>()
					.ToCollection();
			var mods = LookupModifier(dec.ParameterModifier).ToCollection();
			var type = LookupType(dec.Type);
			var names = dec.Name.ToVariableIdentifier().ToCollection();
			var defaultValue = dec.DefaultExpression.TryAcceptForExpression(this);

			return UnifiedParameter.Create(attrs, mods, type, names, defaultValue);
		}

		public IUnifiedElement VisitPropertyDeclaration(PropertyDeclaration dec, object data) {
			var dfn = new UnifiedPropertyDefinition();
			dfn.Modifiers = LookupModifiers(dec.Modifiers);
			if (dec.Getter != null) {
				dfn.Get = dec.Getter.AcceptVisitor(this, data) as UnifiedPropertyBody;
			}
			if (dec.Setter != null) {
				dfn.Set = dec.Setter.AcceptVisitor(this, data) as UnifiedPropertyBody;
			}
			return dfn;
		}

		public IUnifiedElement VisitVariableInitializer(
				VariableInitializer variableInitializer, object data) {
			throw new NotImplementedException("VariableInitializer");
		}

		public IUnifiedElement VisitFixedFieldDeclaration(
				FixedFieldDeclaration fixedFieldDeclaration, object data) {
			throw new NotImplementedException("FixedFieldDeclaration");
		}

		public IUnifiedElement VisitFixedVariableInitializer(
				FixedVariableInitializer fixedVariableInitializer, object data) {
			throw new NotImplementedException("FixedVariableInitializer");
		}

		public IUnifiedElement VisitSimpleType(SimpleType simpleType, object data) {
			var type = UnifiedType.Create(simpleType.Identifier);
			// TODO: Send a Patch to NRefactory
			if (Object.ReferenceEquals(simpleType.TypeArguments, null))
				return type;
			var uTypeArgs = ToArgumentCollection(simpleType.TypeArguments);
			return type.WrapGeneric(uTypeArgs);
		}

		public IUnifiedElement VisitMemberType(MemberType memberType, object data) {
			var ident = UnifiedTypeIdentifier.Create(memberType.MemberName);
			var target = memberType.Target.TryAcceptForExpression(this);
			var uProp = UnifiedType.Create(UnifiedProperty.Create(".", target, ident));
			// TODO: Send a Patch to NRefactory
			if (Object.ReferenceEquals(memberType.TypeArguments, null))
				return uProp;
			var uTypeArgs = ToArgumentCollection(memberType.TypeArguments);
			return uProp.WrapGeneric(uTypeArgs);
		}

		public IUnifiedElement VisitComposedType(
				ComposedType composedType, object data) {
			throw new NotImplementedException("ComposedType");
		}

		public IUnifiedElement VisitArraySpecifier(
				ArraySpecifier arraySpecifier, object data) {
			throw new NotImplementedException("ArraySpecifier");
		}

		public IUnifiedElement VisitPrimitiveType(
				PrimitiveType primitiveType, object data) {
			throw new NotImplementedException("PrimitiveType");
		}

		public IUnifiedElement VisitComment(Comment comment, object data) {
			return null;
		}

		public IUnifiedElement VisitTypeParameterDeclaration(TypeParameterDeclaration dec, object data) {
			return UnifiedType.Create(dec.Name);
		}

		public IUnifiedElement VisitConstraint(Constraint constraint, object data) {
			throw new NotImplementedException("Constraint");
		}

		public IUnifiedElement VisitCSharpTokenNode(
				CSharpTokenNode tokenNode, object data) {
			// TODO 調べる
			//return null;
			throw new NotImplementedException("CSharpTokenNode");
		}

		public IUnifiedElement VisitIdentifier(Identifier identifier, object data) {
			throw new NotImplementedException("Identifier");
		}

		public IUnifiedElement VisitPatternPlaceholder(
				AstNode placeholder, Pattern pattern, object data) {
			throw new NotImplementedException("AstNode");
		}
	}
}
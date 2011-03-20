﻿using System;
using System.IO;
using Ucpf.Core.Model;
using Ucpf.Core.Model.Visitors;

namespace Ucpf.Languages.CSharp {

	public class CSharpCodeGenerator : IUnifiedModelVisitor {

		public static string Generate(UnifiedProgram program) {
			var buff = new StringWriter();
			var visitor = new CSharpCodeGenerator(buff);
			visitor.Visit(program);
			return buff.ToString();
		}

		private readonly TextWriter _writer;
		private int _indent;

		public string IndentSpace { get; set; }

		private CSharpCodeGenerator(TextWriter writer) {
			_writer = writer;
			_indent = 0;
			IndentSpace = "\t";
		}
		
		private void WriteIndent() {
			for (int i = 0; i < _indent; i++)
				_writer.Write(IndentSpace);
		}

		#region program, namespace, class, method, filed ...

		public void Visit(UnifiedProgram program) {
			foreach (var elem in program) {
				elem.Accept(this);
			}
		}

		public void Visit(UnifiedModifierCollection mods) {
			foreach (var mod in mods) {
				mod.Accept(this);
				_writer.Write(" ");
			}
		}

		public void Visit(UnifiedModifier mod) {
			_writer.Write(mod.Name);
		}

		public void Visit(UnifiedType type) {
			_writer.Write(type.Name);
		}

		public void Visit(UnifiedExpressionCollection element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedWhile element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedDoWhile element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedBreak element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedContinue element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedNamespace element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedIndexer element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedTypeParameter element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedTypeParameterCollection element) {
			throw new NotImplementedException();
		}

		public void Visit(UnifiedClassDefinition clsDef) {
			WriteIndent();
			_writer.Write("class ");
			_writer.WriteLine(clsDef.Name);
			clsDef.Body.Accept(this);
		}

		public void Visit(UnifiedFunctionDefinition funcDef) {
			WriteIndent();
			funcDef.Modifiers.Accept(this);
			funcDef.Type.Accept(this);
			_writer.Write(" ");
			_writer.Write(funcDef.Name);
			funcDef.Parameters.Accept(this);
			funcDef.Body.Accept(this);
		}

		public void Visit(UnifiedParameterCollection parameters) {
			_writer.Write("(");
			var splitter = "";
			foreach (var p in parameters) {
				_writer.Write(splitter);
				p.Accept(this);
				splitter = ", ";
			}
			_writer.Write(")");
		}

		public void Visit(UnifiedParameter parameter) {
			parameter.Type.Accept(this);
			_writer.Write(" ");
			_writer.Write(parameter.Name);
		}

		#endregion

		#region statement

		public void Visit(UnifiedBlock block) {
			WriteIndent();
			_writer.WriteLine("{");
			_indent++;
			foreach (var stmt in block) {
				WriteIndent();
				stmt.Accept(this);
				_writer.WriteLine(";");
			}
			_indent--;
			WriteIndent();
			_writer.WriteLine("}");
		}

		public void Visit(UnifiedIf ifStmt) {
			_writer.Write("if (");
			ifStmt.Condition.Accept(this);
			_writer.WriteLine(")");
			ifStmt.TrueBody.Accept(this);
			if (ifStmt.FalseBody != null) {
				WriteIndent();
				_writer.WriteLine("else");
				ifStmt.FalseBody.Accept(this);
			}
		}

		public void Visit(UnifiedReturn element) {
			_writer.Write("return ");
			element.Value.Accept(this);
		}

		#endregion

		#region expression

		public void Visit(UnifiedBinaryExpression expr) {
			_writer.Write("(");
			expr.LeftHandSide.Accept(this);
			expr.Operator.Accept(this);
			expr.RightHandSide.Accept(this);
			_writer.Write(")");
		}

		public void Visit(UnifiedBinaryOperator op) {
			_writer.Write(op.Sign);
		}

		public void Visit(UnifiedCall call) {
			call.Function.Accept(this);
			call.Arguments.Accept(this);
		}

		public void Visit(UnifiedArgumentCollection args) {
			_writer.Write("(");
			var splitter = "";
			foreach (var arg in args) {
				_writer.Write(splitter);
				arg.Accept(this);
				splitter = ", ";
			}
			_writer.Write(")");
		}

		public void Visit(UnifiedArgument arg) {
			arg.Value.Accept(this);
		}

		#endregion

		#region value

		public void Visit(UnifiedVariable variable) {
			_writer.Write(variable.Name);
		}

		public void Visit(UnifiedLiteral lit) {
			_writer.Write(lit.ToString());
		}

		public void Visit<T>(UnifiedTypedLiteral<T> lit) {
			_writer.Write(lit.Value);
		}

		#endregion

		#region not implement

		void IUnifiedModelVisitor.Visit(UnifiedUnaryOperator element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedImport element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedConstructorDefinition element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedVariableDefinition element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedNew element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedArrayNew element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedFor element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedForeach element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedUnaryExpression element) {
			throw new NotImplementedException();
		}

		void IUnifiedModelVisitor.Visit(UnifiedProperty element) {
			throw new NotImplementedException();
		}

		#endregion

	}
}
﻿using System;
using System.Linq;
using System.Xml.Linq;
using Ucpf.CodeModel;
using Ucpf.CodeModelToCode;

namespace Ucpf.Languages.JavaScript.CodeModel {
	// statement
	// : statementBlock
	// | variableStatement
	// | emptyStatement
	// | expressionStatement
	// | ifStatement
	// | iterationStatement
	// | continueStatement
	// | breakStatement
	// | returnStatement
	// | withStatement
	// | labelledStatement
	// | switchStatement
	// | throwStatement
	// | tryStatement
	public class JSStatement : IStatement{
		private XElement _node;

		//constructor
		public JSStatement(XElement xElement) {
			_node = xElement;
		}

		//function
		public static JSStatement CreateStatement(XElement xElement) {
			var element = xElement.Elements().First();

			//case statementBlock
			if (element.Name.LocalName == "statementBlock")
				return new JSBlock(element);

			//case ifStatement
			if (element.Name.LocalName == "ifStatement")
				return new JSIfStatement(element);

			//case returnStatement
			if (element.Name.LocalName == "returnStatement")
				return new JSReturnStatement(element);

			//case error
			throw new NotImplementedException();
		}

		public virtual void Accept(JSCodeModelToCode conv)
		{
			conv.Generate(this);
		}

		void ICodeElement.Accept(ICodeModelToCode conv) {
			conv.Generate(this);
		}

		public override string ToString()
		{
			return _node.Value;
		}

	}
}
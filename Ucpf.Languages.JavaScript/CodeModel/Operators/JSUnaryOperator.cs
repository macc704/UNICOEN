﻿using System;
using System.Xml.Linq;
using Ucpf.Common.CodeModel;
using Ucpf.Common.CodeModel;
using Ucpf.Common.CodeModelToCode;

namespace Ucpf.Languages.JavaScript.CodeModel 
{
	public class JSUnaryOperator : IUnaryOperator
	{
		//properties
		public string Sign { get; private set; }
		public UnaryOperatorType Type { get; private set; }
	
		//constructor
		public JSUnaryOperator(string sign, UnaryOperatorType type) 
		{
			Sign = sign;
			Type = type;
		}

		//function
		public static JSUnaryOperator CreatePrefixOperator(XElement node) {
			string name = node.Value;
			UnaryOperatorType type;

			//TODO implement more OperationType cases
			if ( name == "++") {
				type = UnaryOperatorType.PrefixIncrement;
			}
			else {
				throw new InvalidOperationException();
			}

			return new JSUnaryOperator(name, type);
		}

		public static JSUnaryOperator CreatePostfixOperator(XElement node) {
			string name = node.Value;
			UnaryOperatorType type;

			if ( name == "++") {
				type = UnaryOperatorType.PostfixIncrement;
			}
			else {
				throw new InvalidOperationException();
			}

			return new JSUnaryOperator(name, type);
		}

		public void Accept(JSCodeModelToCode conv) {
			conv.Generate(this);
		}
		
		void ICodeElement.Accept(ICodeModelToCode conv) {
			conv.Generate(this);
		}

		public override string ToString()
		{
			return Sign;
		}

	}
}
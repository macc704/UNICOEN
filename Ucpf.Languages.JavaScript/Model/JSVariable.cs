﻿using System;
using System.Xml.Linq;
using Ucpf.Common.Model;
using Ucpf.Common.ModelToCode;

namespace Ucpf.Languages.JavaScript.Model
{
	public class JSVariable : IVariable
	{
		//property
		public string Name { get; private set; }

		//TODO Which select: how to get name "in constructor" or "as parameter". 
		//constructor
		public JSVariable(XElement node) {
			Name = node.Value;
		}
		
		//function
		public void Accept(IModelToCode conv)
		{
			conv.Generate(this);
		}

		//Common-Interface
		IType IVariable.Type { //JavaScript has no explicit type modifier. 
			get {
				return null;
			}
			set {
				throw new NotImplementedException();
			}
		}

		string IVariable.Name {
			get {
				return Name;
			}
			set {
				throw new NotImplementedException();
			}
		}

	}
}
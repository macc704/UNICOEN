﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ucpf.CodeModel
{
	public interface IReturnStatement : IStatement
	{
		IExpression Expression { get; }
	}
}

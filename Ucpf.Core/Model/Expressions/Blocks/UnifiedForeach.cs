﻿using System;
using System.Collections.Generic;
using Ucpf.Core.Model.Visitors;

namespace Ucpf.Core.Model
{
	/// <summary>
	///   foreach文あるいは拡張for文を表します。
	/// </summary>
	public class UnifiedForeach : UnifiedExpressionWithBlock<UnifiedForeach>
	{
		private UnifiedVariableDefinition _element;

		public UnifiedVariableDefinition Element
		{
			get { return _element; }
			set { _element = SetParentOfChild(value, _element); }
		}

		private IUnifiedExpression _set;

		public IUnifiedExpression Set
		{
			get { return _set; }
			set { _set = SetParentOfChild(value, _set); }
		}

		private UnifiedForeach() {}

		public override void Accept(IUnifiedModelVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void Accept<TData>(IUnifiedModelVisitor<TData> visitor,
		                                   TData data)
		{
			visitor.Visit(this, data);
		}

		public override TResult Accept<TData, TResult>(
			IUnifiedModelVisitor<TData, TResult> visitor, TData data)
		{
			return visitor.Visit(this, data);
		}

		public override IEnumerable<IUnifiedElement> GetElements()
		{
			yield return Element;
			yield return Set;
			yield return Body;
		}

		public override IEnumerable<Tuple<IUnifiedElement, Action<IUnifiedElement>>>
			GetElementAndSetters()
		{
			yield return Tuple.Create<IUnifiedElement, Action<IUnifiedElement>>
				(Element, v => Element = (UnifiedVariableDefinition)v);
			yield return Tuple.Create<IUnifiedElement, Action<IUnifiedElement>>
				(Set, v => Set = (IUnifiedExpression)v);
			yield return Tuple.Create<IUnifiedElement, Action<IUnifiedElement>>
				(Body, v => Body = (UnifiedBlock)v);
		}

		public override IEnumerable<Tuple<IUnifiedElement, Action<IUnifiedElement>>>
			GetElementAndDirectSetters()
		{
			yield return Tuple.Create<IUnifiedElement, Action<IUnifiedElement>>
				(_element, v => _element = (UnifiedVariableDefinition)v);
			yield return Tuple.Create<IUnifiedElement, Action<IUnifiedElement>>
				(_set, v => _set = (IUnifiedExpression)v);
			yield return Tuple.Create<IUnifiedElement, Action<IUnifiedElement>>
				(_body, v => _body = (UnifiedBlock)v);
		}

		public static UnifiedForeach Create(UnifiedVariableDefinition element,
		                                    IUnifiedExpression set)
		{
			return new UnifiedForeach {
				Element = element,
				Set = set,
			};
		}

		public static UnifiedForeach Create(UnifiedVariableDefinition element,
		                                    IUnifiedExpression set, UnifiedBlock body)
		{
			return new UnifiedForeach {
				Element = element,
				Set = set,
				Body = body,
			};
		}
	}
}
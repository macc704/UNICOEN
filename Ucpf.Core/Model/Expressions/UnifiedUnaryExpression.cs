﻿using System;
using System.Collections.Generic;
using Ucpf.Core.Model.Visitors;

namespace Ucpf.Core.Model {
	public class UnifiedUnaryExpression : UnifiedExpression {
		private UnifiedUnaryOperator _operator;

		public UnifiedUnaryOperator Operator {
			get { return _operator; }
			set {
				_operator = SetParentOfChild(value, this, _operator);
			}
		}

		private UnifiedExpression _operand;

		public UnifiedExpression Operand {
			get { return _operand; }
			set {
				_operand = SetParentOfChild(value, this, _operand);
			}
		}

		public override void Accept(IUnifiedModelVisitor visitor) {
			visitor.Visit(this);
		}

		public override TResult Accept<TData, TResult>(
				IUnifiedModelVisitor<TData, TResult> visitor, TData data) {
			return visitor.Visit(this, data);
		}

		public override IEnumerable<UnifiedElement> GetElements() {
			yield return Operator;
			yield return Operand;
		}

		public override IEnumerable<Tuple<UnifiedElement, Action<UnifiedElement>>>
				GetElementAndSetters() {
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(Operator, v => Operator = (UnifiedUnaryOperator)v);
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(Operand, v => Operand = (UnifiedExpression)v);
		}

		public override IEnumerable<Tuple<UnifiedElement, Action<UnifiedElement>>>
				GetElementAndDirectSetters() {
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(_operator, v => _operator = (UnifiedUnaryOperator)v);
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(_operand, v => _operand = (UnifiedExpression)v);
		}

		public override UnifiedElement Normalize() {
			NormalizeChildren();
			var operand = Operand as UnifiedIntegerLiteral;
			if (operand != null) {
				if (Operator.Type == UnifiedUnaryOperatorType.UnaryPlus) {
					return operand;
				}
				if (Operator.Type == UnifiedUnaryOperatorType.Negate) {
					operand.Value = -operand.Value;
					return operand;
				}
			}
			return this;
		}
	}
}
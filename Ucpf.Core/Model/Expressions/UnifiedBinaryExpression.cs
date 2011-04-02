﻿using System;
using System.Collections.Generic;
using Ucpf.Core.Model.Visitors;

namespace Ucpf.Core.Model {
	public class UnifiedBinaryExpression : UnifiedExpression {
		private UnifiedExpression _leftHandSide;

		public UnifiedExpression LeftHandSide {
			get { return _leftHandSide; }
			set {
				_leftHandSide = SetParentOfChild(value, this, _leftHandSide);
			}
		}

		private UnifiedBinaryOperator _operator;

		public UnifiedBinaryOperator Operator {
			get { return _operator; }
			set {
				_operator = SetParentOfChild(value, this, _operator);
			}
		}

		private UnifiedExpression _rightHandSide;

		public UnifiedExpression RightHandSide {
			get { return _rightHandSide; }
			set {
				_rightHandSide = SetParentOfChild(value, this, _rightHandSide);
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
			yield return LeftHandSide;
			yield return Operator;
			yield return RightHandSide;
		}

		public override IEnumerable<Tuple<UnifiedElement, Action<UnifiedElement>>>
				GetElementAndSetters() {
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(LeftHandSide, v => LeftHandSide = (UnifiedExpression)v);
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(Operator, v => Operator = (UnifiedBinaryOperator)v);
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(RightHandSide, v => RightHandSide = (UnifiedExpression)v);
		}

		public override IEnumerable<Tuple<UnifiedElement, Action<UnifiedElement>>>
				GetElementAndDirectSetters() {
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(_leftHandSide, v => _leftHandSide = (UnifiedExpression)v);
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(_operator, v => _operator = (UnifiedBinaryOperator)v);
			yield return Tuple.Create<UnifiedElement, Action<UnifiedElement>>
					(_rightHandSide, v => _rightHandSide = (UnifiedExpression)v);
		}
	}
}
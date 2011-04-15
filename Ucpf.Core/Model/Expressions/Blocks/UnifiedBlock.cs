﻿using System.Collections.Generic;
using Ucpf.Core.Model.Visitors;

namespace Ucpf.Core.Model {
	/// <summary>
	///   "{}"で囲まれた式の列を表します。
	/// </summary>
	public class UnifiedBlock
			: UnifiedElementCollection<IUnifiedExpression, UnifiedBlock>,
			  IUnifiedExpression {
		private UnifiedBlock() {}

		private UnifiedBlock(IEnumerable<IUnifiedExpression> expressions)
				: base(expressions) {}

		public override void Accept(IUnifiedModelVisitor visitor) {
			visitor.Visit(this);
		}

		public override void Accept<TData>(IUnifiedModelVisitor<TData> visitor,
		                                   TData data) {
			visitor.Visit(this, data);
		}

		public override TResult Accept<TData, TResult>(
				IUnifiedModelVisitor<TData, TResult> visitor, TData data) {
			return visitor.Visit(this, data);
		}

		public static UnifiedBlock Create() {
			return new UnifiedBlock();
		}

		public static UnifiedBlock Create(params IUnifiedExpression[] expressions) {
			return new UnifiedBlock(expressions);
		}

		public static UnifiedBlock Create(IEnumerable<IUnifiedExpression> expressions) {
			return new UnifiedBlock(expressions);
		}
			  }
}
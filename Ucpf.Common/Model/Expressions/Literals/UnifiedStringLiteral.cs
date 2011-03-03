﻿using Ucpf.Common.Model.Visitors;

namespace Ucpf.Common.Model {
	public class UnifiedStringLiteral : UnifiedTypedLiteral<string> {
		public override void Accept(IUnifiedModelVisitor visitor) {
			visitor.Visit(this);
		}

		public static UnifiedStringLiteral Create(string value) {
			return new UnifiedStringLiteral {
				TypedValue = value,
			};
		}
	}
}
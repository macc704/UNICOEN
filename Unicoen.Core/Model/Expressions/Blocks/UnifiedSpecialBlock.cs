﻿#region License

// Copyright (C) 2011 The Unicoen Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using Unicoen.Core.Processor;

namespace Unicoen.Core.Model {
	/// <summary>
	///   synchronizedなど特殊なブロックを表します。
	///   e.g. Javaにおける<c>synchronized(this) {...}</c>
	/// </summary>
	public class UnifiedSpecialBlock
			: UnifiedExpressionWithBlock<UnifiedSpecialBlock> {
		public UnifiedSpecialBlockKind Kind { get; set; }

		private IUnifiedExpression _value;

		public IUnifiedExpression Value {
			get { return _value; }
			set { _value = SetChild(value, _value); }
		}

		private UnifiedSpecialBlock() {}

		public override void Accept(IUnifiedModelVisitor visitor) {
			visitor.Visit(this);
		}

		public override void Accept<TData>(
				IUnifiedModelVisitor<TData> visitor,
				TData state) {
			visitor.Visit(this, state);
		}

		public override TResult Accept<TData, TResult>(
				IUnifiedModelVisitor<TData, TResult> visitor, TData state) {
			return visitor.Visit(this, state);
		}

		public static UnifiedSpecialBlock Create(
				UnifiedSpecialBlockKind kind,
				IUnifiedExpression value,
				UnifiedBlock body) {
			return new UnifiedSpecialBlock {
					Kind = kind,
					Value = value,
					Body = body,
			};
		}
			}
}
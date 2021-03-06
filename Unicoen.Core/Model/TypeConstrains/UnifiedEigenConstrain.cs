﻿#region License

// Copyright (C) 2011-2012 The Unicoen Project
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

using System.Diagnostics;
using Unicoen.Processor;

namespace Unicoen.Model {
	/// <summary>
	/// 特異クラスに所属するオブジェクトを表します．
	/// e.g. Rubyにおける <c>class &lt;&lt; obj ... end</c> の <c>obj</c>
	/// </summary>
	public class UnifiedEigenConstrain : UnifiedTypeConstrain {
		private UnifiedExpression _eigenObject;

		/// <summary>
		///   制約の対象を表します e.g. Javaにおける <c>class C extends P { ... }</c> の <c>P</c>
		/// </summary>
		public UnifiedExpression EigenObject {
			get { return _eigenObject; }
			set { _eigenObject = SetChild(value, _eigenObject); }
		}

		protected UnifiedEigenConstrain() {}

		[DebuggerStepThrough]
		public override void Accept(UnifiedVisitor visitor) {
			visitor.Visit(this);
		}

		[DebuggerStepThrough]
		public override void Accept<TArg>(
				UnifiedVisitor<TArg> visitor, TArg arg) {
			visitor.Visit(this, arg);
		}

		[DebuggerStepThrough]
		public override TResult Accept<TArg, TResult>(
				UnifiedVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public static UnifiedEigenConstrain Create(
				UnifiedExpression eigenObject) {
			return new UnifiedEigenConstrain {
					EigenObject = eigenObject,
			};
		}
	}
}
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

using System.Diagnostics;
using Unicoen.Core.Processor;

namespace Unicoen.Core.Model {
	/// <summary>
	///   ブログラム全体を表します。
	/// </summary>
	public class UnifiedProgram : UnifiedElement {
		private UnifiedComment _comments;

		/// <summary>
		///   ソースコードの先頭に表記されたマジックコメントを取得もしくは設定します．
		///   e.g. Pythonにおける<c># -*- coding: utf-8 -*-</c>
		/// </summary>
		public UnifiedComment Comments {
			get { return _comments; }
			set { _comments = SetChild(value, _comments); }
		}

		private UnifiedBlock _body;

		/// <summary>
		///   プログラム全体を構成するブロックを取得もしくは設定します．
		/// </summary>
		public UnifiedBlock Body {
			get { return _body; }
			set { _body = SetChild(value, _body); }
		}

		protected UnifiedProgram() {}

		[DebuggerStepThrough]
		public override void Accept(IUnifiedVisitor visitor) {
			visitor.Visit(this);
		}

		[DebuggerStepThrough]
		public override void Accept<TArg>(
				IUnifiedVisitor<TArg> visitor, TArg arg) {
			visitor.Visit(this, arg);
		}

		[DebuggerStepThrough]
		public override TResult Accept<TArg, TResult>(
				IUnifiedVisitor<TArg, TResult> visitor, TArg arg) {
			return visitor.Visit(this, arg);
		}

		public static UnifiedProgram Create(
				UnifiedBlock body, UnifiedComment comments = null) {
			return new UnifiedProgram {
					Body = body,
					Comments = comments,
			};
		}
	}
}
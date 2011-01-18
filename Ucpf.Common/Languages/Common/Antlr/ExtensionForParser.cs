﻿using System.Collections.Generic;
using Antlr.Runtime;
using Antlr.Runtime.Collections;

namespace Ucpf.Common.Languages.Common.Antlr {
	public class Set : HashSet<object> {}

	public class HashSet : Set {}

	public struct boolean {
		private readonly bool _value;

		public boolean(bool value) {
			_value = value;
		}

		public static implicit operator bool(boolean value) {
			return value._value;
		}

		public static implicit operator boolean(bool value) {
			return new boolean(value);
		}
	}

	public static class ExtensionForParser {
		public static object get(this StackList stack, int i) {
			return stack[i];
		}

		public static int size(this StackList stack) {
			return stack.Count;
		}

		public static bool add<T>(this HashSet<T> set, T item) {
			return set.Add(item);
		}

		public static bool contains<T>(this HashSet<T> set, T item) {
			return set.Contains(item);
		}

		public static string getText(this IToken token) {
			return token.Text;
		}

		public static void skip(this Lexer lexer) {
			lexer.Skip();
		}
	}
}
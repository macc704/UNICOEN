﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;
using Paraiba.Xml.Linq;

namespace Paraiba.Xml.Tests.Linq {
	[TestFixture]
	public class XElementExtensionsTest {
		private static readonly XElement Root;
		private static readonly XElement A1Element;
		private static readonly XElement A2Element;
		private static readonly XElement B1Element;
		private static readonly XElement C1Element;
		private static readonly XElement D1Element;

		static XElementExtensionsTest() {
			Root = new XElement("e");
			A1Element = new XElement("a") { Value = "a1" };
			A2Element = new XElement("a") { Value = "a2" };
			B1Element = new XElement("b") { Value = "b1" };
			C1Element = new XElement("c") { Value = "c1" };
			D1Element = new XElement("d") { Value = "d1" };
			B1Element.Add(C1Element);
			Root.Add(A1Element, A2Element, B1Element, D1Element);
		}

		[Test]
		[TestCase("a", Result = true)]
		[TestCase("c", Result = false)]
		public bool Contains(string name) {
			return Root.Contains(name);
		}

		[Test]
		[TestCase("a", Result = "a")]
		[TestCase("b", Result = "b")]
		public string Name(string name) {
			return Root.Element(name).Name();
		}

		[Test]
		public void PreviousElement() {
			B1Element.PreviousElement().Value.Is("a2");
		}

		[Test]
		public void PreviousElementOrDefault() {
			A1Element.PreviousElementOrDefault().Is((XElement)null);
		}

		[Test]
		public void NextElement() {
			A2Element.NextElement().Name().Is("b");
		}

		[Test]
		public void NextElementOrDefault() {
			D1Element.NextElementOrDefault().Is((XElement)null);
		}

		[Test]
		public void FirstElement() {
			Root.FirstElement().Value.Is("a1");
		}

		[Test]
		public void FirstElementOrDefault() {
			D1Element.FirstElementOrDefault().Is((XElement)null);
		}

		[Test]
		public void LastElement() {
			Root.LastElement().Value.Is("d1");
		}

		[Test]
		public void LastElementOrDefault() {
			D1Element.LastElementOrDefault().Is((XElement)null);
		}

		[Test]
		public void NthElement() {
			Root.NthElement(1).Value.Is("a2");
		}

		[Test]
		public void NthElementOrDefault() {
			D1Element.NthElementOrDefault(5).Is((XElement)null);
		}

		[Test]
		public void FirstElementBeforeSelf() {
			B1Element.FirstElementBeforeSelf().Value.Is("a1");
		}

		[Test]
		public void FirstElementBeforeSelfOrDefault() {
			A1Element.FirstElementBeforeSelfOrDefault().Is((XElement)null);
		}

		[Test]
		public void LastElementBeforeSelf() {
			B1Element.LastElementBeforeSelf().Value.Is("a2");
		}

		[Test]
		public void LastElementBeforeSelfOrDefault() {
			A1Element.LastElementBeforeSelfOrDefault().Is((XElement)null);
		}

		[Test]
		public void NthElementBeforeSelf() {
			B1Element.NthElementBeforeSelf(1).Value.Is("a2");
		}

		[Test]
		public void NthElementBeforeSelfOrDefault() {
			D1Element.NthElementBeforeSelfOrDefault(5).Is((XElement)null);
		}

		[Test]
		public void FirstElementAfterSelf() {
			A1Element.FirstElementAfterSelf().Value.Is("a2");
		}

		[Test]
		public void FirstElementAfterSelfOrDefault() {
			D1Element.FirstElementAfterSelfOrDefault().Is((XElement)null);
		}

		[Test]
		public void LastElementAfterSelf() {
			A1Element.LastElementAfterSelf().Value.Is("d1");
		}

		[Test]
		public void LastElementAfterSelfOrDefault() {
			D1Element.LastElementAfterSelfOrDefault().Is((XElement)null);
		}

		[Test]
		public void NthElementAfterSelf() {
			A1Element.NthElementAfterSelf(1).Value.Is("b1c1");
		}

		[Test]
		public void NthElementAfterSelfOrDefault() {
			D1Element.NthElementAfterSelfOrDefault(5).Is((XElement)null);
		}
	}
}

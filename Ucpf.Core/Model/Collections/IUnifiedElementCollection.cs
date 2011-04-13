﻿using System.Collections.Generic;

namespace Ucpf.Core.Model
{
	public interface IUnifiedElementCollection<TElement>
		: IUnifiedElement, IEnumerable<TElement>
		where TElement : class, IUnifiedElement
	{
		TElement this[int index] { get; set; }
		int Count { get; }
		void PrivateAdd(TElement element);
	}
}
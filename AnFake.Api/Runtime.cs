﻿using System;

namespace AnFake.Api
{
	public static class Runtime
	{
		public static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;
	}
}
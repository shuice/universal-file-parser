﻿#region -- copyright --
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
#endregion
using System;
using System.Reflection;

namespace Neo.IronLua
{
	#region -- class LuaChunk ---------------------------------------------------------

	/// <summary>Represents the compiled chunk.</summary>
	public class LuaChunk
	{
		private readonly Lua lua;
		private readonly string name;
		private Delegate chunk = null;

		/// <summary>Create the chunk</summary>
		/// <param name="lua">Attached runtime</param>
		/// <param name="name">Name of the chunk</param>
		/// <param name="chunk"></param>
		protected internal LuaChunk(Lua lua, string name, Delegate chunk)
		{
			this.lua = lua;
			this.name = name;
			this.chunk = chunk;
		} // ctor

		/// <summary>Assign a methodname with the current chunk.</summary>
		/// <param name="name">unique method name</param>
		protected void RegisterMethod(string name)
			=> Lua.RegisterMethod(name, this);

		/// <summary>Gets for the StackFrame the position in the source file.</summary>
		/// <param name="method"></param>
		/// <param name="ilOffset"></param>
		/// <returns></returns>
		protected internal virtual ILuaDebugInfo GetDebugInfo(MethodBase method, int ilOffset)
			=> null;

		/// <summary>Executes the Chunk on the given Environment</summary>
		/// <param name="env"></param>
		/// <param name="callArgs"></param>
		/// <returns></returns>
		public LuaResult Run(LuaTable env, params object[] callArgs)
		{
			if (!IsCompiled)
				throw new ArgumentException(Properties.Resources.rsChunkNotCompiled, "chunk");

			var args = new object[callArgs == null ? 1 : callArgs.Length + 1];
			args[0] = env;
			if (callArgs != null)
				Array.Copy(callArgs, 0, args, 1, callArgs.Length);

			try
			{
				var r = chunk.DynamicInvoke(args);
				return r is LuaResult ? (LuaResult)r : new LuaResult(r);
			}
			catch (TargetInvocationException e)
			{
				throw e.InnerException; // rethrow with new stackstrace
			}
		} // proc Run

		/// <summary>Returns the associated LuaEngine</summary>
		public Lua Lua => lua;
		/// <summary>Set or get the compiled script.</summary>
		protected internal Delegate Chunk { get => chunk; set => chunk = value; }

		/// <summary>Name of the compiled chunk.</summary>
		public string ChunkName => name;

		/// <summary>Is the chunk compiled and executable.</summary>
		public bool IsCompiled => chunk != null;
		/// <summary>Is the chunk compiled with debug infos</summary>
		public virtual bool HasDebugInfo => false;

		/// <summary>Returns the declaration of the compiled chunk.</summary>
		public MethodInfo Method => chunk?.GetMethodInfo();

		/// <summary>Get the IL-Size</summary>
		public virtual int Size
		{
			get
			{
				if (chunk == null)
					return 0;

				// Gib den Type zurück
				var miChunk = chunk.GetMethodInfo();
				var typeMethod = miChunk.GetType();
				if (typeMethod == RuntimeMethodInfoType)
				{
					dynamic methodBody = ((dynamic)miChunk).GetMethodBody();
					return methodBody.GetILAsByteArray().Length;
				}
				else if (typeMethod == RtDynamicMethodType)
				{
					dynamic dynamicMethod = RtDynamicMethodOwnerFieldInfo.GetValue(miChunk);
					if (dynamicMethod == null)
						return -1;
					return dynamicMethod.GetILGenerator().ILOffset;
				}
				else
					return -1;
			}
		} // prop Size

		// -- Static --------------------------------------------------------------

#pragma warning disable IDE1006 // Naming Styles
		private static readonly Type RuntimeMethodInfoType = Type.GetType("System.Reflection.RuntimeMethodInfo");
		private static readonly Type DynamicMethodType = Type.GetType("System.Reflection.Emit.DynamicMethod");
		private static readonly Type RtDynamicMethodType = Type.GetType("System.Reflection.Emit.DynamicMethod+RTDynamicMethod");
		private static readonly FieldInfo RtDynamicMethodOwnerFieldInfo;
#pragma warning restore IDE1006 // Naming Styles

		static LuaChunk()
		{
			if (RtDynamicMethodType != null)
			{
				RtDynamicMethodOwnerFieldInfo = RtDynamicMethodType.GetTypeInfo().FindDeclaredField("m_owner", ReflectionFlag.NonPublic);
				if (RtDynamicMethodOwnerFieldInfo == null)
					throw new InvalidOperationException("RTDynamicMethod:m_owner not found");
			}
			else
			{
				RtDynamicMethodOwnerFieldInfo = null;
			}
		} // sctor
	} // class LuaChunk

	#endregion
}

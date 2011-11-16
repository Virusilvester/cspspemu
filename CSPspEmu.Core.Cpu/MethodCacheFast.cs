﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu
{
	sealed public class MethodCacheFast
	{
		private Action<CpuThreadState>[] Methods2 = new Action<CpuThreadState>[PspMemory.MainSize / 4];
		private SortedSet<uint> MethodsInList = new SortedSet<uint>();

		public void ClearRange(uint Low, uint High)
		{
			/*
			for (uint n = Low; n < High; n += 4)
			{
				Methods2[n] = null;
			}
			*/
			foreach (var Index in MethodsInList.Where(Index => ((Index >= Low) && (Index < High))).ToArray())
			{
				Methods2[Index] = null;
				MethodsInList.Remove(Index);
			}
		}

		public void Clear()
		{
			//Methods2 = new Action<CpuThreadState>[PspMemory.MainSize / 4];
			foreach (var Index in MethodsInList) Methods2[Index] = null;
			MethodsInList = new SortedSet<uint>();
		}

		public Action<CpuThreadState> TryGetMethodAt(uint PC)
		{
			//PC &= PspMemory.MemoryMask;
			if (PC < PspMemory.MainOffset) throw (new PspMemory.InvalidAddressException(PC));
			uint Index = (PC - PspMemory.MainOffset) / 4;
			if (Index < 0 || Index >= PspMemory.MainSize / 4)
			{
				throw(new IndexOutOfRangeException(
					String.Format("Can't jump to '{0}'. Invalid address.", "0x%08X".Sprintf(PC))
				));
			}
			Action<CpuThreadState> Action = Methods2[Index];
			return Action;
		}

		public void SetMethodAt(uint PC, Action<CpuThreadState> Action)
		{
			//PC &= PspMemory.MemoryMask;
			if (PC < PspMemory.MainOffset) throw (new PspMemory.InvalidAddressException(PC));
			uint Index = (PC - PspMemory.MainOffset) / 4;
			Methods2[Index] = Action;
			MethodsInList.Add(Index);
		}
	}
}
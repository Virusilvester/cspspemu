﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Hle.Vfs.Local
{
	public class HleIoDriverLocalFileSystem : IHleIoDriver
	{
		protected string LocalPath;

		public HleIoDriverLocalFileSystem(string LocalPath)
		{
			this.LocalPath = LocalPath;
		}

		protected string GetFullNormalizedAndSanitizedPath(string Path)
		{
			return LocalPath + "/" + Path;
		}

		public unsafe int IoInit()
		{
			return 0;
		}

		public unsafe int IoExit()
		{
			return 0;
		}

		public unsafe int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
		{
			var RealFileName = GetFullNormalizedAndSanitizedPath(FileName);
			FileMode FileMode = FileMode.Open;
			FileAccess FileAccess = 0;
			bool Append = (Flags & HleIoFlags.Append) != 0;
			bool Read = (Flags & HleIoFlags.Read) != 0;
			bool Write = (Flags & HleIoFlags.Write) != 0;
			bool Truncate = (Flags & HleIoFlags.Truncate) != 0;
			bool Create = (Flags & HleIoFlags.Create) != 0;

			if (Append)
			{
				FileMode = FileMode.Append;
			}
			else if (Create)
			{
				FileMode = FileMode.Create;
			}
			else if (Truncate)
			{
				FileMode = FileMode.Truncate;
			}

			if (Read) FileAccess |= FileAccess.Read;
			if (Write) FileAccess |= FileAccess.Write;
			//if (Append) FileMode |= FileMode.Open;

			HleIoDrvFileArg.FileArgument = File.Open(RealFileName, FileMode, FileAccess);
			return 0;
		}

		public unsafe int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
			FileStream.Close();
			return 0;
		}

		public unsafe int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
		{
			var Buffer = new byte[OutputLength];
			var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
			int Readed = FileStream.Read(Buffer, 0, OutputLength);
			for (int n = 0; n < Readed; n++) *OutputPointer++ = Buffer[n];
			return Readed;
		}

		public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, int Whence)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			var RealFileName = GetFullNormalizedAndSanitizedPath(Name);
			try
			{
				HleIoDrvFileArg.FileArgument = new DirectoryInfo(RealFileName).EnumerateFileSystemInfos().GetEnumerator();
				return 0;
			}
			catch (DirectoryNotFoundException DirectoryNotFoundException)
			{
				Console.WriteLine(DirectoryNotFoundException);
				return -1;
			}
		}

		public unsafe int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
		{
			//throw new NotImplementedException();
			return 0;
		}

		public unsafe int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* IoDirent)
		{
			var Enumerator = (IEnumerator<FileSystemInfo>)HleIoDrvFileArg.FileArgument;

			// More items.
			if (Enumerator.MoveNext())
			{
				var FileSystemInfo = Enumerator.Current;
				var FileInfo = (FileSystemInfo as FileInfo);
				var DirectoryInfo = (FileSystemInfo as DirectoryInfo);
				{
					PointerUtils.StoreStringOnPtr(FileSystemInfo.Name, Encoding.UTF8, IoDirent[0].Name);
					IoDirent[0].Stat.Size = (FileInfo != null) ? FileInfo.Length : 0;
					//IoDirent[0].Stat.Mode = SceMode
					IoDirent[0].Stat.DeviceDependentData[0] = 10;
				}
				return 0;
			}
			// No more items.
			else
			{
				return -1;
			}
		}

		public unsafe int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoMount(HleIoDrvFileArg HleIoDrvFileArg)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoUmount(HleIoDrvFileArg HleIoDrvFileArg)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			throw new NotImplementedException();
		}

		public unsafe int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
		{
			throw new NotImplementedException();
		}
	}
}

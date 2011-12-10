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
		protected string BasePath;

		public HleIoDriverLocalFileSystem(string BasePath)
		{
			this.BasePath = BasePath;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		static public string GetSanitizedPath(string Path)
		{
			var Parts = new Stack<string>();
			foreach (var Part in Path.Split('/', '\\'))
			{
				switch (Part)
				{
					case "": if (Parts.Count == 0) Parts.Push(""); break;
					case ".": break;
					case "..": if (Parts.Count > 0) Parts.Pop(); break;
					default: Parts.Push(Part); break;
				}
			}

			return String.Join("/", Parts.Reverse());
		}

		protected string GetFullNormalizedAndSanitizedPath(string Path)
		{
			var Normalized = BasePath + "/" + GetSanitizedPath(Path);
			Normalized = Normalized.Replace('/', '\\').Replace("\\\\", "\\");
			return Normalized;
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

			HleIoDrvFileArg.FileArgument = File.Open(RealFileName, FileMode, FileAccess, FileShare.Read);
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
			try
			{
				var Buffer = new byte[OutputLength];
				var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
				int Readed = FileStream.Read(Buffer, 0, OutputLength);
				for (int n = 0; n < Readed; n++) *OutputPointer++ = Buffer[n];
				return Readed;
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
				return -1;
			}
		}

		public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
		{
			try
			{
				var Buffer = new byte[InputLength];
				for (int n = 0; n < InputLength; n++) Buffer[n]  = * InputPointer++;

				var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
				FileStream.Write(Buffer, 0, InputLength);
				FileStream.Flush();
				return InputLength;
			}
			catch (Exception Exception)
			{
				//Console.Error.WriteLine(Exception);
				return -1;
			}
		}

		public unsafe long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
		{
			var FileStream = ((FileStream)HleIoDrvFileArg.FileArgument);
			switch (Whence)
			{
				case SeekAnchor.Set:
					FileStream.Position = Offset;
					break;
				case SeekAnchor.Cursor:
					FileStream.Position = FileStream.Position + Offset;
					break;
				case SeekAnchor.End:
					FileStream.Position = FileStream.Length + Offset;
					break;
			}
			return FileStream.Position;
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
			var RealFileName = GetFullNormalizedAndSanitizedPath(Name);
			Directory.CreateDirectory(RealFileName);
			//HleIoDrvFileArg.
			//throw new NotImplementedException();
			return 0;
		}

		public unsafe int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			throw new NotImplementedException();
		}

		private class FileInfoEnumerator
		{
			int Index = -1;
			FileSystemInfo[] List;

			public FileInfoEnumerator(FileSystemInfo[] List)
			{
				this.List = List;
			}

			public bool MoveNext()
			{
				Index++;
				return (GetLeft() > 0);
			}

			public FileSystemInfo Current
			{
				get
				{
					return List[Index];
				}
			}

			public int GetLeft()
			{
				return List.Length - Index;
			}
		}

		public unsafe int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
		{
			var RealFileName = GetFullNormalizedAndSanitizedPath(Name);
			try
			{
				//Console.Error.WriteLine("'{0}'", RealFileName);
				var FileSystemInfo = new DirectoryInfo(RealFileName).EnumerateFileSystemInfos().ToArray();
				/*
				foreach (var Info in FileSystemInfo)
				{
					Console.Error.WriteLine(Info);
				}
				*/
				HleIoDrvFileArg.FileArgument = new FileInfoEnumerator(FileSystemInfo);
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
			var Enumerator = (FileInfoEnumerator)HleIoDrvFileArg.FileArgument;

			// More items.
			if (Enumerator.MoveNext())
			{
				//Console.Error.WriteLine("'{0}'", Enumerator.Current.ToString());
				var FileSystemInfo = Enumerator.Current;
				var FileInfo = (FileSystemInfo as FileInfo);
				var DirectoryInfo = (FileSystemInfo as DirectoryInfo);
				{
					PointerUtils.StoreStringOnPtr(FileSystemInfo.Name, Encoding.UTF8, IoDirent[0].Name);
					IoDirent[0].Stat.Size = (FileInfo != null) ? FileInfo.Length : 0;
					//IoDirent[0].Stat.Mode = SceMode

					if (DirectoryInfo != null)
					{
						IoDirent[0].Stat.Mode = SceMode.Directory | (SceMode)0777;
						IoDirent[0].Stat.Attributes = IOFileModes.Directory;
					}
					else
					{
						IoDirent[0].Stat.Mode = SceMode.File | (SceMode)0777;
						IoDirent[0].Stat.Attributes = IOFileModes.File | IOFileModes.CanRead | IOFileModes.CanWrite | IOFileModes.CanExecute;
					}

					IoDirent[0].Stat.DeviceDependentData0 = 10;
				}
			}
			// No more items.
			else
			{
			}

			return Enumerator.GetLeft();
		}

		public unsafe int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
		{
			var RealFileName = GetFullNormalizedAndSanitizedPath(FileName);
			//Console.WriteLine(RealFileName);
			var FileInfo = new FileInfo(RealFileName);
			Stat[0].Size = FileInfo.Length;
			return 0;
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

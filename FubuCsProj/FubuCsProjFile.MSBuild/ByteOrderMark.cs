using System.IO;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class ByteOrderMark
	{
		private static readonly ByteOrderMark[] table;

		public string Name
		{
			get;
			private set;
		}

		public byte[] Bytes
		{
			get;
			private set;
		}

		public int Length
		{
			get
			{
				return this.Bytes.Length;
			}
		}

		private ByteOrderMark(string name, byte[] bytes)
		{
			this.Bytes = bytes;
			this.Name = name;
		}

		public static ByteOrderMark GetByName(string name)
		{
			for (int i = 0; i < ByteOrderMark.table.Length; i++)
			{
				if (ByteOrderMark.table[i].Name == name)
				{
					return ByteOrderMark.table[i];
				}
			}
			return null;
		}

		public static bool TryParse(byte[] buffer, int available, out ByteOrderMark bom)
		{
			if (buffer.Length >= 2)
			{
				for (int i = 0; i < ByteOrderMark.table.Length; i++)
				{
					bool matched = true;
					if (available >= ByteOrderMark.table[i].Bytes.Length)
					{
						for (int j = 0; j < ByteOrderMark.table[i].Bytes.Length; j++)
						{
							if (buffer[j] != ByteOrderMark.table[i].Bytes[j])
							{
								matched = false;
								break;
							}
						}
						if (matched)
						{
							bom = ByteOrderMark.table[i];
							return true;
						}
					}
				}
			}
			bom = null;
			return false;
		}

		public static bool TryParse(Stream stream, out ByteOrderMark bom)
		{
			byte[] buffer = new byte[4];
			int nread;
			if ((nread = stream.Read(buffer, 0, buffer.Length)) < 2)
			{
				bom = null;
				return false;
			}
			return ByteOrderMark.TryParse(buffer, nread, out bom);
		}

		static ByteOrderMark()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ByteOrderMark[] array = new ByteOrderMark[14];
			array[0] = new ByteOrderMark("UTF-8", new byte[]
			{
				239,
				187,
				191
			});
			array[1] = new ByteOrderMark("UTF-32BE", new byte[]
			{
				0,
				0,
				254,
				255
			});
			ByteOrderMark[] arg_6F_0 = array;
			int arg_6F_1 = 2;
			string arg_6A_0 = "UTF-32LE";
			byte[] array2 = new byte[4];
			array2[0] = 255;
			array2[1] = 254;
			arg_6F_0[arg_6F_1] = new ByteOrderMark(arg_6A_0, array2);
			array[3] = new ByteOrderMark("UTF-16BE", new byte[]
			{
				254,
				255
			});
			array[4] = new ByteOrderMark("UTF-16LE", new byte[]
			{
				255,
				254
			});
			array[5] = new ByteOrderMark("UTF-7", new byte[]
			{
				43,
				47,
				118,
				56
			});
			array[6] = new ByteOrderMark("UTF-7", new byte[]
			{
				43,
				47,
				118,
				57
			});
			array[7] = new ByteOrderMark("UTF-7", new byte[]
			{
				43,
				47,
				118,
				43
			});
			array[8] = new ByteOrderMark("UTF-7", new byte[]
			{
				43,
				47,
				118,
				47
			});
			array[9] = new ByteOrderMark("UTF-1", new byte[]
			{
				247,
				100,
				76
			});
			array[10] = new ByteOrderMark("UTF-EBCDIC", new byte[]
			{
				221,
				115,
				102,
				115
			});
			array[11] = new ByteOrderMark("SCSU", new byte[]
			{
				14,
				254,
				255
			});
			array[12] = new ByteOrderMark("BOCU-1", new byte[]
			{
				251,
				238,
				40
			});
			array[13] = new ByteOrderMark("GB18030", new byte[]
			{
				132,
				49,
				149,
				51
			});
			ByteOrderMark.table = array;
		}
	}
}

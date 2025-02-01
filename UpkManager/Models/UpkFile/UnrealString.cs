using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile
{

    public class UnrealString : UnrealUpkBuilderBase
    {

        #region Properties

        public int Size { get; private set; }
        public string String { get; private set; }

        #endregion Properties

        #region Unreal Methods

        public async Task ReadString(ByteArrayReader reader)
        {
            Size = reader.ReadInt32();

            if (Size == 0)
            {
                String = String.Empty;

                return;
            }

            if (Size < 0)
            {
                int size = -Size * 2;

                byte[] str = await reader.ReadBytes(size).ConfigureAwait(false);

                String = Encoding.Unicode.GetString(str);
            }
            else
            {
                byte[] str = await reader.ReadBytes(Size - 1).ConfigureAwait(false);

                reader.Skip(1); // NULL Terminator

                String = Encoding.ASCII.GetString(str);
            }
        }

        public void SetString(string value)
        {
            String = value;

            Size = isUnicode() ? -value.Length : value.Length + 1;
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(int)
                        + (Size < 0 ? String.Length * 2 : String.Length + 1);

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            Writer.WriteInt32(Size);

            if (Size < 0)
            {
                await Writer.WriteBytes(Encoding.Unicode.GetBytes(String)).ConfigureAwait(false);
            }
            else
            {
                await Writer.WriteBytes(Encoding.ASCII.GetBytes(String)).ConfigureAwait(false);

                Writer.WriteByte(0);
            }
        }

        #endregion UnrealUpkBuilderBase Implementation

        #region Private Methods

        private bool isUnicode()
        {
            const int maxAnsiCode = 255;

            return String.Any(c => c > maxAnsiCode);
        }

        #endregion Private Methods

    }

}

using System.Collections.Generic;
using System.Linq;

using UpkManager.Models.UpkFile;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Extensions
{

    internal static class ListExtensions
    {

        public static UnrealNameTableEntry AddUnrealNameTableEntry(this List<UnrealNameTableEntry> nameTable, string value)
        {
            UnrealString valueString = new UnrealString();

            valueString.SetString(value);

            int index = nameTable.Max(nt => nt.TableIndex) + 1;

            UnrealNameTableEntry entry = new UnrealNameTableEntry();

            entry.SetNameTableEntry(valueString, 0x0007001000000000, index);

            nameTable.Add(entry);

            return entry;
        }

    }

}

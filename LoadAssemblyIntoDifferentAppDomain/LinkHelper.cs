using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace LoadAssemblyIntoDifferentAppDomain
{
    class LinkHelper
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport("kernel32.dll")]
        static extern int GetLastError();

        internal static void CreateSymbolicLinkFromTo(string linkName, string targetName, SymbolicLink type)
        {
            if (CreateSymbolicLink(linkName, targetName, type)) return;

            throwLastError();
        }

        private static void throwLastError()
        {
            var errorcode = GetLastError();
            var message = GetSystemMessage(errorcode);
            throw new Exception(message);
        }

        internal static void RemoveSymbolicLinkFrom(string linkName, SymbolicLink type)
        {
            if (RemoveSymbolicLink(linkName, type)) return;

            throwLastError();
        }

        static bool RemoveSymbolicLink(string lpSymlinkFileName, SymbolicLink dwFlags)
        {
            switch (dwFlags)
            {
                case SymbolicLink.Directory:
                    try
                    {
                        Directory.Delete(lpSymlinkFileName);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                case SymbolicLink.File:
                    try
                    {
                        File.Delete(lpSymlinkFileName);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        static string GetSystemMessage(int errorCode)
        {
            int capacity = 512;
            int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
            StringBuilder sb = new StringBuilder(capacity);
            FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, errorCode, 0,
                sb, sb.Capacity, IntPtr.Zero);
            int i = sb.Length;
            if (i > 0 && sb[i - 1] == 10) i--;
            if (i > 0 && sb[i - 1] == 13) i--;
            sb.Length = i;
            return sb.ToString();
        }

        [DllImport("kernel32.dll")]
        static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId,
            int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr Arguments);

    }


    enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }
}

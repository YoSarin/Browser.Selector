namespace Browser.Selector.Lib
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public interface IParentProcessFinder
    {
        Process GetParent();
        Process GetParent(Process process);
        Process GetParent(int processId);
        Process GetParent(IntPtr handle);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ParentProcessUtilities
    {
        // These members must match PROCESS_BASIC_INFORMATION
        internal IntPtr Reserved1;
        internal IntPtr PebBaseAddress;
        internal IntPtr Reserved2_0;
        internal IntPtr Reserved2_1;
        internal IntPtr UniqueProcessId;
        internal IntPtr InheritedFromUniqueProcessId;
    }

    public class ParentProcessFinder : IParentProcessFinder
    {

        ParentProcessUtilities ParentProcessUtilities;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public Process GetParent()
        {
            return GetParent(Process.GetCurrentProcess());
        }

        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public Process GetParent(int id)
        {
            Process process = Process.GetProcessById(id);
            return GetParent(process.Handle);
        }

        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public Process GetParent(Process process)
        {
            return GetParent(process.Handle);
        }

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public Process GetParent(IntPtr handle)
        {
            var pbi = new ParentProcessFinder();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref pbi.ParentProcessUtilities, Marshal.SizeOf(pbi.ParentProcessUtilities), out returnLength);
            if (status != 0)
            {
                throw new Win32Exception(status);
            }

            try
            {
                return Process.GetProcessById(pbi.ParentProcessUtilities.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }
    }
}

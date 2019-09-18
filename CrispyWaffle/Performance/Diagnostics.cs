namespace CrispyWaffle.Performance
{
#if !DEBUG
    using Extensions;
    using System.Threading;
#endif

    /// <summary>
    /// A diagnostics.
    /// </summary>
    /// <remarks>
    /// http://kiranpatils.wordpress.com/2010/03/06/how-to-get-cpu-usagememory-usagephysical-memory/
    /// Troubleshooting #1 - Exception at PerformanceCounter: http://stackoverflow.com/questions/17980178/cannot-load-counter-name-data-because-an-invalid-index-exception
    /// Troubleshooting #2 - Class invalid: http://msdn.microsoft.com/en-us/library/windows/desktop/aa394181(v=vs.85).aspx</remarks>
    public static class Diagnostics
    {
        #region ~Ctor

        /// <summary>
        /// Initializes static members of the <see cref="Diagnostics" /> class.
        /// </summary>
        static Diagnostics()
        {
#if DEBUG
            CPUUsage = -1;
#else
            var thread = new Thread(UpdateCPUUsage)
            {
                Name = "Update CPU usage"
            };
            thread.Start();
#endif
        }

        #endregion

        #region Private methods

#if !DEBUG

        /// <summary>
        /// Updates the cpu usage.
        /// </summary>
        /// 
        private static void UpdateCPUUsage()
        {
            while (true)
            {
                GetCPUUsage();
                Thread.Sleep(1000);
                if (ShouldStop)
                    break;
            }
        }



        /// <summary>
        /// Gets CPU Usage in %.
        /// </summary>
        /// <returns>The CPU usage.</returns>
        /// <remarks>Versão: 1.63.3774.776
        /// Autor: Guilherme Branco Stracini
        /// Data: 12/07/2014.</remarks>
        [Localizable(false)]
        private static void GetCPUUsage()
        {
            try
            {
                var processor = new ManagementObject("Win32_PerfFormattedData_PerfOS_Processor.Name='_Total'");
                processor.Get();
                CPUUsage = processor.Properties["PercentProcessorTime"].Value.ToString().ToInt32();
            }
            catch (Exception)
            {
                CPUUsage = -1;
            }
        }
#endif
        ///// <summary>
        ///// Gets total physical memory.
        ///// </summary>
        ///// <returns>The physical memory.</returns>

        //[Localizable(false)]
        //private static double GetPhysicalMemory()
        //{
        //    var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
        //    var searcher = new ManagementObjectSearcher(winQuery);

        //    double memory = 0;

        //    foreach (var o in searcher.Get())
        //    {
        //        var item = (ManagementObject)o;
        //        memory = double.Parse(item["TotalVisibleMemorySize"].ToString());
        //    }

        //    return memory;
        //}

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the should stop.
        /// </summary>
        /// <value>The should stop.</value>
        public static bool ShouldStop { get; set; }

        /// <summary>
        /// Gets the cpu usage.
        /// </summary>
        /// <value>The cpu usage.</value>
        public static int CPUUsage { get; private set; }

        #endregion

        //#region Public methods

        ///// <summary>
        ///// Gets memory usage in %.
        ///// </summary>
        ///// <returns>The memory usage.</returns>

        //public static double GetMemoryUsage()
        //{
        //    var pCntr = new PerformanceCounter("Memory", "Available KBytes", true);
        //    double memAvailable = pCntr.NextValue();
        //    var memPhysical = GetPhysicalMemory();
        //    return (memPhysical - memAvailable) * 100 / memPhysical;
        //}

        //#endregion
    }
}

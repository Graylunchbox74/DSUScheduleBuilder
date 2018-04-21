﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DSUScheduleBuilder
{
    using Network;
    using Models;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            new HttpRequester("http://localhost:4200");
            HttpRequester.Default.Login("HalversonTom@pluto.com", "Password1!");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
        }
    }
}

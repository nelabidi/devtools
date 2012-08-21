//
// Project: vsxa2p, visual studio express attach to process.
//
//  this add the missing attach to process feature to C# Express, in C++ Express
//  makes it easy and faster to attach to a process.
//  
// Tested on: C#/C++ Express  2008
//
// by naoufel el abidi, naoufel@semcat.net  08/20/2012 
////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Reflection;

using EnvDTE;
using EnvDTE80;



namespace vsxa2p
{
    class Program
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        static void Main(string[] args)
        {

            if (args.Length != 2)
            { 
                Console.WriteLine("vsxa2p Attach to process tool for C#/C++ Express");
                Console.WriteLine("usage: vsxa2p <solution> <process>" );
                Console.WriteLine("     <solution>  the currently open solution path in C#/C++ Express." );
                Console.WriteLine("     <process>   full or partial path of the process to attach to." );
                Console.WriteLine("example: vsxa2p $(SolutionFileName)   dir\\process.exe");
                return;
            }
            string solPath = args[0];
            string processPath = args[1];

            IBindCtx ctx;
            IRunningObjectTable table;
            IEnumMoniker mon;
            IMoniker[] lst = new IMoniker[1];
            Object vcObject = null;
            int retVal;
            
            // find the File moniker object in the ROT running object table
            // When an instance of VS Express is running and a solution is open it creates a Solution object
            // and register it in the ROT
            retVal = CreateBindCtx(0, out ctx);
            if (retVal != 0)
            {
                Console.WriteLine("Error: CreateBindCtx failed with error " + retVal);
                return;
            }
            ctx.GetRunningObjectTable(out table);
            table.EnumRunning(out mon);
            //walk through the table
            while (mon.Next(1, lst, IntPtr.Zero) == 0)
            {
                string displayName;
                lst[0].GetDisplayName(ctx, lst[0], out displayName);

                if (displayName.IndexOf(solPath,StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    table.GetObject(lst[0], out vcObject);
                    break;
                }
            }
            //object found ?
            if (vcObject == null)
            {
                Console.WriteLine("Error: solution not found " + solPath);
                return;
            }
            Solution objSol = (Solution)vcObject;
            DTE objDTE = objSol.DTE;
            Debugger objDbg = objSol.DTE.Debugger;
              

            //find the process and attach to it
            foreach (EnvDTE.Process p in objDbg.LocalProcesses)
            {
                if (p.Name.IndexOf(processPath, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    objDbg.DetachAll();
                    //wait for the debugger to detach from any process
                    System.Threading.Thread.Sleep(200);
                    p.Attach();

                    return;
                }
            }
            
            Console.WriteLine("Error: process not found " + processPath);
            return;
                
        }

    }
}

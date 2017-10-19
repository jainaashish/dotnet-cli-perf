﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetCliPerf
{
    public class NetCoreCli : TempDir
    {
        private IDictionary<string, string> _environment = new Dictionary<string, string>();

        [Params("console", "mvc")]
        public string Template { get; set; }

        [IterationSetup(Target = nameof(New))]
        public void IterationSetupNew()
        {
            IterationSetup();
            _environment["NUGET_PACKAGES"] = Path.Combine(IterationTempDir, "nuget-packages");
            _environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(IterationTempDir, "nuget-http-cache");
        }

        [Benchmark]
        public void New()
        {
            DotNet($"new {Template} --no-restore");
        }

        [IterationSetup(Target = nameof(RestoreInitial))]
        public void IterationSetupRestoreInitial()
        {
            IterationSetupNew();
            New();
        }

        [Benchmark]
        public void RestoreInitial()
        {
            DotNet("restore");
        }

        [IterationSetup(Target = nameof(RestoreNoChanges))]
        public void IterationSetupRestoreNoChanges()
        {
            IterationSetupRestoreInitial();
            RestoreInitial();
        }

        [Benchmark]
        public void RestoreNoChanges()
        {
            DotNet("restore");
        }

        [IterationSetup(Target = nameof(AddPackage))]
        public void IterationSetupAddPackage()
        {
            IterationSetupRestoreInitial();
            RestoreInitial();
        }

        [Benchmark]
        public void AddPackage()
        {
            DotNet("add package NUnit --version 3.8.1 --no-restore");
        }

        [IterationSetup(Target = nameof(RestoreAfterAdd))]
        public void IterationSetupRestoreAfterAdd()
        {
            IterationSetupAddPackage();
            AddPackage();
        }

        [Benchmark]
        public void RestoreAfterAdd()
        {
            DotNet("restore");
        }

        [IterationSetup(Target = nameof(BuildInitial))]
        public void IterationSetupBuildInitial()
        {
            IterationSetupRestoreInitial();
            RestoreInitial();
        }

        [Benchmark]
        public void BuildInitial()
        {
            DotNet("build");
        }

        [IterationSetup(Target = nameof(BuildNoChanges))]
        public void IterationSetupBuildNoChanges()
        {
            IterationSetupBuildInitial();
            BuildInitial();
        }

        [Benchmark]
        public void BuildNoChanges()
        {
            DotNet("build");
        }

        [IterationSetup(Target = nameof(BuildAfterChange))]
        public void IterationSetupBuildAfterChange()
        {
            IterationSetupBuildInitial();
            BuildInitial();
            ModifySource();
        }

        [Benchmark]
        public void BuildAfterChange()
        {
            DotNet("build");
        }

        [IterationSetup(Target = nameof(RunNoChanges))]
        public void IterationSetupRunNoChanges()
        {
            IterationSetupBuildInitial();
            if (Template == "mvc")
            {
                TerminateAfterWebAppStarted();
            }
            BuildInitial();
        }

        [Benchmark]
        public void RunNoChanges()
        {
            DotNet("run");
        }

        [IterationSetup(Target = nameof(RunAfterChange))]
        public void IterationSetupRunAfterChange()
        {
            IterationSetupRunNoChanges();
            RunNoChanges();
            ModifySource();
        }

        [Benchmark]
        public void RunAfterChange()
        {
            DotNet("run");
        }

        private void DotNet(string arguments)
        {
            Util.RunProcess("dotnet", arguments, IterationTempDir, environment: _environment);
        }

        private void ModifySource()
        {
            var replacements = new Dictionary<string, Tuple<string, string, string>>
            {
                { "mvc", Tuple.Create("Startup.cs", "default", "default2") },
                { "console", Tuple.Create("Program.cs", "Hello", "Hello2") },
            };

            var path = Path.Combine(IterationTempDir, replacements[Template].Item1);
            var oldValue = replacements[Template].Item2;
            var newValue = replacements[Template].Item3;

            File.WriteAllText(path, File.ReadAllText(path).Replace(oldValue, newValue));
        }

        private void TerminateAfterWebAppStarted()
        {
            var path = Path.Combine(IterationTempDir, "Program.cs");
            File.WriteAllText(path, File.ReadAllText(path).Replace("Run()", "RunAsync()"));
        }
    }
}

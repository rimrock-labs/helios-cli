namespace Rimrock.Helios.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Rimrock.Helios.Common;
    using Microsoft.Diagnostics.Symbols;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Symbol store class.
    /// </summary>
    public sealed class SymbolStore : IDisposable
    {
        private readonly ILogger logger;
        private readonly TextWriter readerLog;
        private readonly SymbolReader reader;

        private readonly HashSet<string> missingModuleSymbols;
        private readonly Dictionary<string, string> moduleNameCache;
        private readonly Dictionary<string, string> methodNameCache;

        private int totalAddressCount;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolStore"/> class.
        /// </summary>
        /// <param name="context">The analysis context.</param>
        /// <param name="logger">The logger.</param>
        public SymbolStore(
            AnalysisContext context,
            ILogger logger)
        {
            this.logger = logger;

            string readerLogPath = context.TracePath + ".symbols.log";
            this.readerLog = TextWriter.Synchronized(new StreamWriter(readerLogPath));

            // TODO: make symbol path configurable
            this.reader = new SymbolReader(this.readerLog, SymbolPath.SymbolPathFromEnvironment);

            this.missingModuleSymbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.moduleNameCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.methodNameCache = new Dictionary<string, string>();
        }

        /// <summary>
        /// Tries to resolve the stack.
        /// </summary>
        /// <param name="inStack">The input stack.</param>
        /// <param name="outStack">The output stack.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool TryResolve(
            TraceCallStack? inStack,
            [NotNullWhen(true)] out DataFrame? outStack)
        {
            bool result = false;
            outStack = default;
            DataFrame? previous = default;
            if (inStack != null)
            {
                while (inStack != null)
                {
                    this.ResolveCodeAddress(inStack.CodeAddress, out string moduleName, out string methodName);

                    DataFrame frame = new()
                    {
                        ModuleName = moduleName,
                        MethodName = methodName,
                    };

                    if (previous == null)
                    {
                        outStack = previous = frame;
                        result = true;
                    }
                    else
                    {
                        frame.AddChild(previous);
                        previous = frame;
                    }

                    inStack = inStack.Caller;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.reader?.Dispose();
                this.readerLog?.Dispose();

                this.disposed = true;
            }
        }

        /// <summary>
        /// Formats the specified names.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="methodName">Name of the method.</param>
        internal static void Format(ref string moduleName, ref string methodName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                moduleName = "?";
            }

            if (string.IsNullOrEmpty(methodName))
            {
                methodName = "?";
            }

            TrimMethodOffsets(ref methodName);
            TrimModuleExtension(ref moduleName);
            TrimOtherArtifacts(ref moduleName, ref methodName);
        }

        internal static void TrimMethodOffsets(ref string methodName)
        {
            int methodTrimIndex = methodName.LastIndexOf('$');
            if (methodTrimIndex > 0 &&
                methodTrimIndex < methodName.Length - 1 &&
                methodName[methodTrimIndex - 1] == ')' &&
                methodName[methodTrimIndex + 1] == '#')
            {
                methodName = methodName[..methodTrimIndex];
            }
        }

        internal static void TrimModuleExtension(ref string moduleName)
        {
            int length = moduleName.Length;
            if (length > 3 &&
                moduleName[length - 3] == '.' &&
                moduleName[length - 2] == 'n' &&
                moduleName[length - 1] == 'i')
            {
                moduleName = moduleName[..(length - 3)];
            }
        }

        internal static void TrimOtherArtifacts(ref string moduleName, ref string methodName)
        {
            if (methodName.Length > 2 &&
                methodName[0] == '?' &&
                methodName[1] == '?')
            {
                methodName = methodName[2..];
            }

            if (methodName.Length > 7 &&
                methodName[0] == '[' &&
                methodName[1] == 'C' &&
                methodName[2] == 'O' &&
                methodName[3] == 'L' &&
                methodName[4] == 'D' &&
                methodName[5] == ']' &&
                methodName[6] == ' ')
            {
                methodName = methodName[7..];
            }

            if (moduleName.Length == 24 &&
                moduleName.StartsWith("App_global.asax.", StringComparison.OrdinalIgnoreCase))
            {
                moduleName = moduleName[..15];
            }

            if (methodName.StartsWith("dynamicClass.", StringComparison.OrdinalIgnoreCase))
            {
                if (methodName[13] == 'G' && methodName[14] == 'o')
                {
                    methodName = "dynamicClass.Go";
                }
                else
                {
                    int indexOfParanthesis = methodName.IndexOf('(');
                    if (indexOfParanthesis > 0)
                    {
                        methodName = methodName[..indexOfParanthesis];
                    }
                }
            }
        }

        private static string GetFromCache(Dictionary<string, string> cache, string key)
        {
            if (!cache.TryGetValue(key, out string? symbol))
            {
                cache[key] = symbol = key;
            }

            return symbol;
        }

        private void ResolveCodeAddress(
            TraceCodeAddress codeAddress,
            out string moduleName,
            out string methodName)
        {
            this.OptimizeCodeAddress(codeAddress);

            if (codeAddress.Method == null)
            {
                TraceModuleFile moduleFile = codeAddress.ModuleFile;
                if (moduleFile != null && !this.missingModuleSymbols.Contains(moduleFile.Name))
                {
                    try
                    {
                        this.reader.FindSymbolFilePathForModule(moduleFile.FilePath);
                        codeAddress.CodeAddresses.LookupSymbolsForModule(this.reader, moduleFile);
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogInformation("Symbol resolution failed for '{ModuleFileFilePath}', with exception:{NewLine}{Exception}", moduleFile.FilePath, Environment.NewLine, exception);
                    }

                    if (string.IsNullOrEmpty(codeAddress.FullMethodName))
                    {
                        this.missingModuleSymbols.Add(moduleFile.Name);
                    }
                }
            }

            moduleName = codeAddress.ModuleName;
            methodName = codeAddress.FullMethodName;

            Format(ref moduleName, ref methodName);

            moduleName = GetFromCache(this.moduleNameCache, moduleName);
            methodName = GetFromCache(this.methodNameCache, methodName);
        }

        private TraceCodeAddress OptimizeCodeAddress(TraceCodeAddress codeAddress)
        {
            if (this.totalAddressCount == 0)
            {
                TraceCodeAddresses addresses = codeAddress.CodeAddresses;
                this.totalAddressCount = addresses.Count;
                if (this.totalAddressCount != 0)
                {
                    return addresses[(CodeAddressIndex)(this.totalAddressCount - 1)];
                }
            }

            return codeAddress;
        }
    }
}
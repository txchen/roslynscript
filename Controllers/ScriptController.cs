using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace RoslynScript
{
    public class RSContext
    {
        private Dictionary<string, object> _inputs;
        public RSContext(Dictionary<string, object> inputs)
        {
            _inputs = inputs;
        }

        // json number will be cast as long in aspnet
        public long GetLong(string key)
        {
            if (_inputs.ContainsKey(key) && _inputs[key] is long)
            {
                return (long)_inputs[key];
            }
            return 0L;
        }

        public string GetString(string key)
        {
            if (_inputs.ContainsKey(key) && _inputs[key] is string)
            {
                return (string)_inputs[key];
            }
            return "";
        }

        public double GetDouble(string key)
        {
            if (_inputs.ContainsKey(key) && _inputs[key] is double)
            {
                return (double)_inputs[key];
            }
            return 0;
        }
    }

    [Route("/api/script")]
    public class RulesController
    {
        private ScriptOptions _scriptOptions;

        public RulesController()
        {
            _scriptOptions = ScriptOptions.Default;
            // Add reference to mscorlib
            var mscorlib = typeof(System.Object).GetTypeInfo().Assembly;
            var colLib = typeof(System.Collections.IDictionary).GetTypeInfo().Assembly;
            var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;
            var thisAssembly = typeof(Decision).GetTypeInfo().Assembly;
            // NOTE: if we add current assembly into reference, in dotnet core, Dictionary<,> then cannot be used!
            _scriptOptions = _scriptOptions.AddReferences(mscorlib, colLib, systemCore, thisAssembly);
            // Add namespaces
            _scriptOptions = _scriptOptions.WithImports("System");
            _scriptOptions = _scriptOptions.WithImports("System.Linq");
            _scriptOptions = _scriptOptions.WithImports("System.Collections.Generic");
            _scriptOptions = _scriptOptions.WithImports("RoslynScript");
        }

        [HttpGet]
        public string Get()
        {
            return "Hello";
        }

        [HttpPost]
        public async Task<ExecutionResult> Post([FromBody]ExecutionRequest rule)
        {
            var script = CSharpScript.Create<Decision>(rule.ScriptContent, _scriptOptions, typeof(RSContext));

            var compilation = script.GetCompilation();
            var diagnostics = compilation.GetDiagnostics();

            if (diagnostics.Any())
            {
                string errorDesc = "Compile error: ";
                foreach (var diagnostic in diagnostics)
                {
                    errorDesc += $"[[ {diagnostic.Location.GetLineSpan().EndLinePosition} : {diagnostic.GetMessage()} ]], ";
                }
                return new ExecutionResult() { Decision = Decision.None.ToString(),
                    ErrorCode = 1,
                    ErrorDescription = errorDesc };
            }

            ScriptState<Decision> scriptResult = null;
            try
            {
                scriptResult = await script.RunAsync(new RSContext(rule.Inputs));
            }
            catch (Exception e)
            {
                return new ExecutionResult() { Decision = Decision.None.ToString(), ErrorCode = 2,
                    ErrorDescription = "Execution error: " + e.ToString() };
            }

            var result = new ExecutionResult();
            result.Decision = scriptResult.ReturnValue.ToString();
            result.Vars = new Dictionary<string, object>();
            foreach (var variable in scriptResult.Variables)
            {
                result.Vars[variable.Name] = variable.Value;
            }

            return result;
        }
    }
}

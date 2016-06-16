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
    public class Globals
    {
        public Dictionary<string, object> Inputs;
        public Dictionary<string, object> Outputs;
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
            var colLib2 = typeof(System.Collections.Generic.Dictionary<string, object>).GetTypeInfo().Assembly;
            var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;
            var thisAssembly = typeof(Decision).GetTypeInfo().Assembly;
            Console.WriteLine(colLib.ToString());

            Console.WriteLine(colLib2.ToString());
            _scriptOptions = _scriptOptions.AddReferences(mscorlib, colLib, colLib2, systemCore, thisAssembly);
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
            var script = CSharpScript.Create<Decision>(rule.ScriptContent, _scriptOptions, typeof(Globals));
            try
            {
                script.Compile();
            }
            catch (Exception e)
            {
                return new ExecutionResult() { Decision = Decision.None.ToString(), ErrorCode = 1,
                    ErrorDescription = "Compile error: " + e.ToString() };
            }

            ScriptState<Decision> scriptResult = null;
            try
            {
                scriptResult = await script.RunAsync(new Globals() { Inputs = rule.Inputs } );
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

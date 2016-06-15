using System.Collections.Generic;

namespace RoslynScript
{
    public class ExecutionRequest
    {
        public string ScriptContent { get; set; }

        public Dictionary<string, object> Inputs { get; set; }
    }
}

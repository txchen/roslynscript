using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace RoslynScript
{
    [Route("/api/script")]
    public class RulesController
    {
        [HttpGet]
        public string Get()
        {
            return "Hello";
        }

        [HttpPost]
        public ExecutionResult Post([FromBody]ExecutionRequest rule)
        {
            return new ExecutionResult() { Decision = Decision.Approve.ToString() };
        }
    }
}

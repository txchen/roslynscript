using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace RoslynScript
{
    [Route("/api/script")]
    public class RulesController
    {
        [HttpPost]
        public ExecutionResult Post([FromBody]ExecutionRequest rule)
        {
            return new ExecutionResult();
        }
    }
}

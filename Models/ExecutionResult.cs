using System.Collections.Generic;

namespace RoslynScript
{
    public enum Decision
    {
       None = 1,
       Approve,
       NeedReview,
       Reject,
    }

    public class ExecutionResult
    {
        public Decision Decision { get; set; }

        public Dictionary<string, object> Outputs { get; set; }
    }
}

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
        public int ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public string Decision { get; set; }

        public Dictionary<string, object> Vars { get; set; }
    }
}

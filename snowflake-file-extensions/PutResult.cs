using System;

namespace Snowflake.FileStream
{
    public class PutResult
    {
        public readonly IPutFileItem Filename;
        public readonly string Result;
        public readonly Exception Exception;

        public PutResult(IPutFileItem filename, string result)
        {
            Filename = filename;
            Result = result;
            Exception = null;
        }
        public PutResult(IPutFileItem filename, string result, Exception exception)
        {
            Filename = filename;
            Result = result;
            Exception = exception;
        }
    }
}
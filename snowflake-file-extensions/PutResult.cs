using System;

namespace Snowflake.FileStream
{
    public class PutResult
    {
        public readonly string Filename;
        public readonly string Result;
        public readonly Exception Exception;

        public PutResult(string filename, string result)
        {
            Filename = filename;
            Result = result;
            Exception = null;
        }
        public PutResult(string filename, string result, Exception exception)
        {
            Filename = filename;
            Result = result;
            Exception = exception;
        }
    }
}
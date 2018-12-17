using System;

namespace Bugger.Features.RoleAssignment
{
    public class PhraseAlreadyAddedException : Exception
    {
        public PhraseAlreadyAddedException()
        {
        }

        public PhraseAlreadyAddedException(string message)
            : base(message)
        {
        }

        public PhraseAlreadyAddedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class InvalidPhraseException : Exception
    {
        public InvalidPhraseException()
        {
        }

        public InvalidPhraseException(string message)
            : base(message)
        {
        }

        public InvalidPhraseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class RoleIdAlreadyAddedException : Exception
    {
        public RoleIdAlreadyAddedException()
        {
        }

        public RoleIdAlreadyAddedException(string message)
            : base(message)
        {
        }

        public RoleIdAlreadyAddedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class RelationAlreadyExistsException : Exception
    {
        public RelationAlreadyExistsException()
        {
        }

        public RelationAlreadyExistsException(string message)
            : base(message)
        {
        }

        public RelationAlreadyExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class RelationNotFoundException : Exception
    {
        public RelationNotFoundException()
        {
        }

        public RelationNotFoundException(string message)
            : base(message)
        {
        }

        public RelationNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
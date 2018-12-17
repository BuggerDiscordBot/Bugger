using System;
using System.Collections.Generic;
using System.Text;

namespace Bugger.Features.Lists
{
    public class ListException : Exception
    {
        public ListException(string message) : base(message)
        {
        }

        public class ListManagerException : ListException
        {
            public ListManagerException(string message) : base(message)
            {
            }
        }

        public class ListPermissionException : ListException
        {
            public ListPermissionException(string message) : base(message)
            {
            }
        }

        public static ListManagerException GetListManagerException()
        {
            return GetListManagerException(ListErrorMessage.General.UnknownError);
        }

        public static ListManagerException GetListManagerException(string message, params string[] parameters)
        {
            var formattedMessage = String.Format(message, parameters);
            return new ListManagerException(formattedMessage);
        }

        public static ListPermissionException GetListPermissionException()
        {
            return GetListPermissionException(ListErrorMessage.Permission.NoPermission_list);
        }

        public static ListPermissionException GetListPermissionException(string message, params string[] parameters)
        {
            var formattedMessage = String.Format(message, parameters);
            return new ListPermissionException(formattedMessage);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Bugger.Features.Lists
{
    public static class ListErrorMessage
    {
        public struct General : IEquatable<object>
        {
            public static readonly string ListDoesNotExist_list = "List '{0}' does not exist.";
            public static readonly string ListAlreadyExists_list = "List '{0}' already exists.";
            public static readonly string ListIsEmpty_list = "The list '{0}' is empty";
            public static readonly string IndexOutOfBounds_list = "The index was out of the bounds of the list '{0}'";
            public static readonly string WrongInputForIndex = "The index value must be an integer";
            public static readonly string NoLists = "There are no lists";
            public static readonly string WrongFormat = "Wrong format";
            public static readonly string UnknownPermission_shortcut = "A permission with the shortcut '{0}' does not exists.";
            public static readonly string RoleDoesNotExist_rolename = "The role '{0}' does not exist.";
            public static readonly string UnknownCommand_command = "Unknown command '{0}'.";
            public static readonly string UnknownError = "Oops, something went wrong";
        }

        public struct Permission : IEquatable<object>
        {
            public static readonly string NoPermission = "Permission denied";
            public static readonly string NoPermission_list = "You do not have the permission to access the list '{0}'";
        }
    }
}

﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class QueryAsserterBase
    {
        public virtual ISetExtractor SetExtractor { get; set; }
        public virtual IExpectedData ExpectedData { get; set; }

        #region AssertQuery

        public abstract Task AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        #endregion

        #region AssertQueryScalar

        public abstract Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        #endregion

        #region AssertQueryScalar - nullable

        public abstract void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TResult : struct;

        public abstract void AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct;

        #endregion
    }
}

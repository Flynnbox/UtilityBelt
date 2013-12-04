using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;

namespace UtilityBelt
{
	public class DatabaseRetryProvider
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public IEnumerable<int> SqlErrorsToRetry { get; private set; }
		public TimeSpan? RetryDelay { get; private set; }
		public int RetryCount { get; private set; }

		public DatabaseRetryProvider(IEnumerable<int> sqlErrorCodesToRetry, TimeSpan retryDelay, int retryCount)
		{
			SqlErrorsToRetry = sqlErrorCodesToRetry;
			RetryDelay = retryDelay;
			RetryCount = retryCount;
		}

		public DatabaseRetryProvider(IEnumerable<int> sqlErrorCodesToRetry, TimeSpan retryDelay) 
			: this(sqlErrorCodesToRetry, retryDelay, 2)
		{
		}

		public DatabaseRetryProvider(IEnumerable<int> sqlErrorCodesToRetry)
			:this (sqlErrorCodesToRetry, TimeSpan.FromSeconds(1))
		{
		}

		public DatabaseRetryProvider()
			:this(new[]{ 1205 })
		{
		}

		public T Retry<T>(Func<T> function)
		{
			int count = RetryCount;
			while (true)
			{
				try
				{
					return function();
				}
				catch (SqlException ex)
				{
					--count;
					if (count <= 0)
					{
						throw;
					}

					if (SqlErrorsToRetry.Contains(ex.Number))
					{
						if (log.IsErrorEnabled)
						{
							log.Error(String.Format("Retrying after Sql Error {0}", ex.Number), ex);
						}
					}
					else
					{
						throw;
					}
					if (RetryDelay.HasValue)
					{
						Thread.Sleep(RetryDelay.Value);
					}
				}
			}
		}

		public void Retry(Action action)
		{
			Retry(() => { action(); return true; });
		}
	}
}

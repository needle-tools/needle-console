using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class RetryHandler
	{
		public static async Task<T> ExecuteAsync<T>(Func<Task<T>> op, int maxAttempts = 3)
		{
			int attempt = 0;
			while (true)
			{
				attempt++;
				try
				{
					if (attempt > 1) Debug.Log($"RetryHandler: attempt {attempt}");
					var result = await op();
					return result;
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"RetryHandler: transient failure on attempt {attempt}\n{ex.GetType().Name}: {ex.Message}");
					if (attempt >= maxAttempts) throw;
					await Task.Delay(25 * attempt);
				}
			}
		}
	}
}

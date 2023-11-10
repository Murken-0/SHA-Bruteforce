using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sha_bruteforce;

class Program
{
	private static bool _foundFlag = false;
	public static void Main()
	{
		var targetHashes = new List<string>
		{
			"1115dd800feaacefdf481f1f9070374a2a81e27880f187396db67958b207cbad",
			"3a7bd3e2360a3d29eea436fcfb7e44c735d117c42d1c1835420b6b9942dd4f1b",
			"74e1bb62f8dabb8125a58852b63bdf6eaef667cb56ac7f7cdba6d7305c50a22f"
		};

		while (true)
		{
			Console.WriteLine("Выберете подбираемый хеш:");
			for (int i = 0; i < targetHashes.Count; i++)
				Console.WriteLine($"{i}. {targetHashes[i]}");
			int selection = int.Parse(Console.ReadLine());

			Console.Write("Количество потоков: ");
			int numThreads = int.Parse(Console.ReadLine());
			if (numThreads < 1)
			{
                Console.WriteLine("Указано некорректное кол-во потоков!");
				continue;
            }

			Console.WriteLine("Идет процесс подбора пароля...");
			GC.Collect();
			var stopwatch = new Stopwatch();

			stopwatch.Start();
			Run(targetHashes[selection - 1], numThreads);
			stopwatch.Stop();

			Console.WriteLine($"Подбор завершен. Затраченное время: {stopwatch.Elapsed}");
			Console.WriteLine("Введите ENTER, чтобы продожить");
			Console.WriteLine();
			Console.ReadKey();
			_foundFlag = false;
		}
	}

	private static void Run(string targetHashes, int numThreads)
	{
		var chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
		var chunkSize = chars.Length / numThreads;
		var tasks = new List<Task>();

		for (int i = 0; i < numThreads; i++)
		{
			var startIndex = i * chunkSize;
			var endIndex = (i == numThreads - 1) ? chars.Length : startIndex + chunkSize;
			var threadChars = new char[endIndex - startIndex];
			Array.Copy(chars, startIndex, threadChars, 0, endIndex - startIndex);

			tasks.Add(Task.Run(() =>
			{
				foreach (var c1 in threadChars)
				{
					foreach (var c2 in chars)
					{
						foreach (var c3 in chars)
						{
							foreach (var c4 in chars)
							{
								foreach (var c5 in chars)
								{
									if (_foundFlag != true)
									{
										var password = new string(new[] { c1, c2, c3, c4, c5 });
										var hash = GetHash(password);
										if (targetHashes.Contains(hash))
										{
											Console.WriteLine($"Найден пароль: {password}, соответствующий хэшу SHA-256: {hash}");
											_foundFlag = true;
											break;
										}
									}
									else
									{
										return;
									}

								}
							}
						}
					}
				}
			}));
		}
		Task.WaitAll(tasks.ToArray());
	}

	private static string GetHash(string password)
	{
		using SHA256 sha256 = SHA256.Create();
		var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
		var builder = new StringBuilder();
		for (int i = 0; i < hashBytes.Length; i++)
			builder.Append(hashBytes[i].ToString("x2"));
		return builder.ToString();
	}
}
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Smith.MatrixSdk;

static string AskString(string title)
{
    Console.Write($"{title}: ");
    return Console.ReadLine()!;
}

var logger = NullLogger.Instance;
using var httpClient = new HttpClient();
var client = new MatrixClient(logger, httpClient, new Uri("https://matrix.org"));

var user = AskString("User");
var password = AskString("Password");
var loginResponse = await client.Login(user, password);

Console.WriteLine($"Access token: {loginResponse.AccessToken}");

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleCompiler.Analysis
{
    /// <summary>
    /// Analyzes the original source code for potential logical bugs.
    /// This class reads the source file and sends its contents to a locally hosted LLM
    /// (via Ollama) with a prompt specifically asking for bug detection.
    /// </summary>
    public class BugDetectionAnalyzer
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Initiates bug detection analysis on the given source file.
        /// </summary>
        /// <param name="sourceFilePath">Path to the source code file (e.g., tests/files/test.minic).</param>
        public async Task AnalyzeSourceAsync(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine("Source file not found: " + sourceFilePath);
                return;
            }

            // Read the entire source file.
            string sourceCode = await File.ReadAllTextAsync(sourceFilePath);

            // Construct a detailed prompt emphasizing bug detection.
            string prompt = @"
                You are a static analysis tool for a C-like programming language.

                Review the following source code and find real *logical bugs* or *unnecessary conditionals* based on the known, hardcoded variable values. 
                Use reasoning based on the actual variable assignments to evaluate:
                - Whether each `if`/`else` condition always evaluates the same way (i.e., deterministic).
                - Whether certain branches of code are unreachable due to constant conditions.
                - Any incorrect or misleading assignments (e.g., confusing use of logical operators).
                - Any operator precedence mistakes or unexpected outcomes.

                This language uses C-like syntax. You don't need to worry about exact syntax or boilerplate formatting.
                Focus only on the logic and control flow, given that all variable values are known at compile time.

                Output a concise list of the real or potential bugs or issues, each with a short explanation. Do NOT include redundant warnings or generic advice.

                Here is the code:
                " + sourceCode;


            var payload = new
            {
                model = "llama3.2:3b", // or whichever model you wish to use
                prompt = prompt,
                stream = false
            };

            // Serialize payload and send it to your Ollama endpoint.
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                httpClient.Timeout = TimeSpan.FromSeconds(200);
                HttpResponseMessage response = await httpClient.PostAsync("http://localhost:11434/api/generate", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"LLM call failed with status {response.StatusCode}");
                    return;
                }
                string result = await response.Content.ReadAsStringAsync();

                try
                {
                    using var doc = JsonDocument.Parse(result);
                    if (doc.RootElement.TryGetProperty("response", out var responseText))
                    {
                        Console.WriteLine("Bug Detection Analysis Report:");
                        Console.WriteLine(responseText.GetString());
                    }
                    else
                    {
                        Console.WriteLine("Unexpected response format:");
                        Console.WriteLine(result);
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Failed to parse LLM response JSON: " + ex.Message);
                    Console.WriteLine("Raw response:\n" + result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling the local LLM: " + ex.Message);
            }
        }
    }
}

#region Legal
/*
Copyright (c) 2015-2025, AccurateAppend Corp
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using Net_8_Sample_code.Models;
using Net_8_Sample_code.Models.Request;
using Net_8_Sample_code.Models.Response;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

/// <summary>
/// Simple console demo application to interact with the Management API endpoints.
/// For more information, go to <seealso cref="https://docs.accurateappend.com/docs/management-api-overview"/>
/// </summary>
class Program
{

    #region Fields

    private static HttpClient _httpClient;
    private static readonly String _baseURL = "http://api.accurateappend.com/Management/V1/Queue/";
    private static Guid _apiKey;

    #endregion

    #region Main


    private static async Task Main(string[] args)
    {
        Console.WriteLine("Accurate Append NET 8 demo. Press Ctrl+C to exit anytime.\n");

        Console.WriteLine("Enter your API key (UUID format):");
        var apiKeyString = Console.ReadLine();

        if (!Guid.TryParse(apiKeyString, out _apiKey))
        {
            Console.WriteLine("Invalid UUID format. Exiting...");
            return;
        }

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", apiKeyString);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        _httpClient.Timeout = TimeSpan.FromMinutes(5);

        while (true)
        {
            Console.WriteLine("\nSelect an option and press Enter key:\n");
            Console.WriteLine("1. Upload new file");
            Console.WriteLine("2. View your jobs");
            Console.WriteLine("3. View job details");
            Console.WriteLine("4. Download job");
            Console.WriteLine("5. List configured products");
            Console.WriteLine("6. Exit");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": await UploadNewFile(); break;
                case "2": await ViewJobs(); break;
                case "3": await ViewJobDetails(); break;
                case "4": await DownloadJob(); break;
                case "5": await ListConfiguredProducts(); break;
                case "6": return;
                default: Console.WriteLine("Invalid option. Try again."); break;
            }
        }

    }
    #endregion

    #region Methods


    /// <summary>
    /// Method to upload the CSV, it's maping and delimeters.
    /// </summary>
    static async Task UploadNewFile()
    {
        var job = new JobSubmit();

        Console.WriteLine("Enter the path to your CSV file:");
        var filePath = Console.ReadLine();
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found.");
            return;
        }

        job.FilePath = filePath;
        job.ApiKey = _apiKey;

        Console.WriteLine("\nRequired fields for API submission:");
        Console.WriteLine("- Request ID (UUID)");
        Console.WriteLine("- API Key (UUID)");
        Console.WriteLine("- Product Name");
        Console.WriteLine("- Column Mapping");
        Console.WriteLine("- Delimiter (optional, default is comma)");

        Console.WriteLine("Enter Product Name:");
        job.ProductName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(job.ProductName))
        {
            Console.WriteLine("Product Name is required.");
            return;
        }

        Console.WriteLine("Enter column mapping (semicolon-separated field names):");

        Console.WriteLine("Example: FirstName;LastName;Email;Phone");
        job.ColumnMap = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(job.ColumnMap))
        {
            Console.WriteLine("Column mapping is required.");
            return;
        }

        Console.WriteLine("Enter delimiter (comma, pipe, or tab) or press Enter for default (comma):");
        var delimiterInput = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(delimiterInput))
        {
            job.Delimiter = delimiterInput[0];
            if (!",|\t".Contains(job.Delimiter))
            {
                Console.WriteLine("Unsupported delimiter. Using default comma.");
                job.Delimiter = ',';
            }
        }

        job.RequestId = Guid.NewGuid();

        var finalUrl = _baseURL + "Submit";

        using (var content = new MultipartFormDataContent())
        using (var fileStream = new FileStream(job.FilePath, FileMode.Open, FileAccess.Read))
        {
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = Path.GetFileName(job.FilePath)
            };
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");

            #region Form Data

            content.Add(fileContent, "file", Path.GetFileName(job.FilePath));
            content.Add(new StringContent(job.ApiKey.ToString()), "api_key");
            content.Add(new StringContent(job.ProductName), "product_name");
            content.Add(new StringContent(job.ColumnMap), "column_map");
            content.Add(new StringContent(job.Delimiter.ToString()), "delimiter");
            content.Add(new StringContent(job.RequestId.ToString()), "request_id"); 

            #endregion

            try
            {
                var response = await _httpClient.PostAsync(finalUrl, content);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine("File uploaded successfully!");
                        Console.WriteLine($"Request ID: {job.RequestId}");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Your api key was not found or you are not a current subscriber.");
                        break;
                    case HttpStatusCode.BadRequest:
                        Console.WriteLine("Your request has invalid or missing parameters or could not be processed.");
                        break;
                    case HttpStatusCode.InternalServerError:
                        Console.WriteLine("We encountered and error while processing your request. Please try again.");
                        break;
                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Method to retrieve the job in a set of fix dates. 
    /// </summary>
    /// <remarks>
    /// Start date can't be greater than end date, 
    /// and the time span between dates can't greater than 30 days
    /// </remarks>
    static async Task ViewJobs()
    {
        const int WeeksToShow = 4;
        const int DaysPerRange = 7;

        var ranges = Enumerable.Range(0, WeeksToShow)
            .Select(i =>
            {
                var end = DateTime.Now.Date.AddDays(-DaysPerRange * i);
                return (Start: end.AddDays(-DaysPerRange + 1), End: end, Identifier: i + 1);
            })
            .ToList();

        Console.WriteLine("Select a recent date range:");
        ranges.ForEach(r => Console.WriteLine($"{r.Identifier}: {r.Start:MM/dd/yyyy} - {r.End:MM/dd/yyyy}"));

        if (!int.TryParse(Console.ReadLine(), out var selectedRange) ||
            selectedRange < 0 ||
            selectedRange >= WeeksToShow)
        {
            Console.WriteLine("Invalid input, returning...");
            return;
        }

        var selectedRangeData = ranges.First(i => i.Identifier == selectedRange);
        var finalUrl = $"{_baseURL}View?start={selectedRangeData.Start:M-d-yyyy}&end={selectedRangeData.End:M-d-yyyy}";
        
        try
        {
            var response = await _httpClient.GetAsync(finalUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API request failed with status: {response.StatusCode}");
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent == "[]")
            {
                Console.WriteLine("No jobs were found for the selected dates.");
                return;
            }

            var jobs = JsonSerializer.Deserialize<ViewJobsResponse>(responseContent);

            if (jobs?.Complete.Count > 0)
            {
                Console.WriteLine($"\nJobs completed found: {jobs?.Complete.Count}");
                foreach (var job in jobs?.Complete)
                {
                    DisplayJob(job);
                }
            }
            else
            {
                Console.WriteLine($"No jobs completed found for the selected dates.");
            }

            if (jobs?.InProcess.Count > 0)
            {
                Console.WriteLine($"\nJobs in process found: {jobs?.InProcess.Count}");
                foreach (var job in jobs?.InProcess)
                {
                    DisplayJob(job);
                }
            }
            else
            {
                Console.WriteLine($"No jobs in process found for the selected dates.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }
    }

    /// <summary>
    /// Method to retrieve the details of a single job.
    /// </summary>
    static async Task ViewJobDetails()
    {
        Console.WriteLine("\nEnter the job key (UUID format):");
        var jobKey = Console.ReadLine();

        if (!Guid.TryParse(jobKey, out _))
        {
            Console.WriteLine("Invalid UUID format. Returning...");
            return;
        }

        var finalUrl = $"{_baseURL}Detail/{jobKey.ToString()}";

        try
        {
            var response = await _httpClient.GetAsync(finalUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API request failed with status: {response.StatusCode}");
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var job = JsonSerializer.Deserialize<JobDetailResponse>(responseContent);

            DisplayJob(job);
        }
        catch (Exception ex)
        {

        }
    }

    static async Task DownloadJob()
    {
        Console.WriteLine("\nEnter the job key (UUID format):");
        var jobKeyInput = Console.ReadLine();
        if (!Guid.TryParse(jobKeyInput, out var jobKey))
        {
            Console.WriteLine("Invalid UUID format. Returning...");
            return;
        }

        var finalUrl = $"{_baseURL}GenerateAccess/{jobKey}";

        try
        {
            var response = await _httpClient.GetAsync(finalUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API request failed with status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
                return;
            }

            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                           ?? $"Job_{jobKey}.csv";

            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            Console.WriteLine($"Job downloaded successfully as '{fileName}' in executable folder.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Method to retrieve the list of configured products.
    /// </summary>
    /// <remarks>
    /// Useful as a reference to query what preconfigured products are available for your account to perform API based file batch append with.
    /// Provides the name and the field mappings required to use the Submit File endpoint.
    /// </remarks>
    static async Task ListConfiguredProducts()
    {
        var finalUrl = $"{_baseURL}ListConfiguredProducts";

        try
        {
            var response = await _httpClient.GetAsync(finalUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API request failed with status: {response.StatusCode}");
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent == "[]")
            {
                Console.WriteLine("No configured products were found.");
                return;
            }

            var listConfiguredProducts = JsonSerializer.Deserialize<ListConfiguredProductsResponse>(responseContent);

            if (listConfiguredProducts?.Products.Count>1)
            {
                Console.WriteLine($"\nProducts available: {listConfiguredProducts.Products.Count}");
                foreach (var product in listConfiguredProducts.Products)
                {
                    DisplayConfiguredProducts(product);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }
    }

    
    #endregion

    #region Helpers

    private static void DisplayJob(Job job)
    {
        Console.WriteLine("\nJob Details:");
        Console.WriteLine($"Job Key: {job.JobKey}");
        Console.WriteLine($"File Name: {job.FileName}");
        Console.WriteLine($"Date Submitted: {job.DateSubmitted}");
        Console.WriteLine($"Date Completed: {job.DateComplete}");
        Console.WriteLine($"Source: {job.Source}");
        Console.WriteLine($"Status: {job.Status}");
        Console.WriteLine($"Record Count: {job.RecordCount}");
        Console.WriteLine($"Matched Records: {job.MatchedRecords}");
        Console.WriteLine($"Average Match Rate: {job.AvgMatchRate:P2}");
        if (job._Ref.Detail != null) Console.WriteLine($"Ref: {job._Ref?.Detail}");
        if (job._Ref.Download != null) Console.WriteLine($"Ref: {job._Ref?.Download}");

        Console.WriteLine("------------------------\n");
    }

    private static void DisplayConfiguredProducts(Product product)
    {
        Console.WriteLine($"Product name: {product.Name}\n");
        Console.WriteLine("Fields required for mapping:\n");
        foreach (var reqMapping in product.RequiredMapping)
        {
            Console.WriteLine($"{reqMapping}");
        }
        Console.WriteLine("\nOptional fields:\n");
        foreach (var optionalMapping in product.OptionalMapping)
        {
            Console.WriteLine($"{optionalMapping}");
        }
        Console.WriteLine("\n------------------------\n");
    }

    #endregion
}
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Faker.Api.Middlewares
{
    public class RequestLogger
    {
        private readonly RequestDelegate next;

        public RequestLogger(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestBody = await getRequestBodyAsync(context);
            var (elapsed, body) = await this.getResponseParamsAsync(context);

            Console.WriteLine(new RequestLogEntry
                              {
                                  Method = context.Request.Method,
                                  Uri = context.Request.GetDisplayUrl(),
                                  Request = requestBody,
                                  ResponseCode = context.Response.StatusCode,
                                  ResponseBody = body,
                                  Elapsed = elapsed
                              });
        }


        private async Task<(TimeSpan Elapsed, string Body)> getResponseParamsAsync(HttpContext context)
        {
            var stopwatch = new Stopwatch();

            string responseBody;

            var originalResponseStream = context.Response.Body;

            await using (var responseStream = new MemoryStream())
            {
                context.Response.Body = responseStream;

                stopwatch.Start();

                await this.next(context);

                stopwatch.Stop();

                context.Response.Body.Seek(0, SeekOrigin.Begin);

                responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

                context.Response.Body.Seek(0, SeekOrigin.Begin);

                await responseStream.CopyToAsync(originalResponseStream);
            }

            return (stopwatch.Elapsed, responseBody);
        }

        private static async Task<string> getRequestBodyAsync(HttpContext context)
        {
            string requestBody;

            context.Request.EnableBuffering();

            await using (var ms = new MemoryStream())
            {
                await context.Request.Body.CopyToAsync(ms);
                ms.Position = 0;

                requestBody = await new StreamReader(ms).ReadToEndAsync();
            }

            context.Request.Body.Position = 0;

            return requestBody;
        }
    }


    public class RequestLogEntry
    {
        public string Method { get; set; }

        public string Uri { get; set; }

        public string Request { get; set; }

        public int ResponseCode { get; set; }

        public string ResponseBody { get; set; }

        public TimeSpan Elapsed { get; set; }


        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                                            new JsonSerializerOptions
                                            {
                                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                            });
        }
    }
}
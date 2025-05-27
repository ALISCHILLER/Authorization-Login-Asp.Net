using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class ResponseCompressionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseCompressionMiddleware> _logger;

        public ResponseCompressionMiddleware(RequestDelegate next, ILogger<ResponseCompressionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString();
            if (string.IsNullOrEmpty(acceptEncoding))
            {
                await _next(context);
                return;
            }

            if (acceptEncoding.Contains("gzip"))
            {
                context.Response.Headers.Add("Content-Encoding", "gzip");
                context.Response.Headers.Add("Vary", "Accept-Encoding");
                var originalBodyStream = context.Response.Body;

                using (var gzipStream = new GZipStream(originalBodyStream, CompressionLevel.Fastest))
                {
                    context.Response.Body = gzipStream;
                    await _next(context);
                }
            }
            else if (acceptEncoding.Contains("deflate"))
            {
                context.Response.Headers.Add("Content-Encoding", "deflate");
                context.Response.Headers.Add("Vary", "Accept-Encoding");
                var originalBodyStream = context.Response.Body;

                using (var deflateStream = new DeflateStream(originalBodyStream, CompressionLevel.Fastest))
                {
                    context.Response.Body = deflateStream;
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
} 
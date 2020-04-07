using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api
{


    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseWebSockets();

            app.UseRouting();



            // Mapping WebSocket with using IApplicationBuilder

            //app.Map("/ws", builder =>
            //{
            //    builder.Use(async (context, next) =>
            //    {
            //        if (context.WebSockets.IsWebSocketRequest)
            //        {
            //            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            //            await Echo(webSocket);
            //            return;
            //        }
            //        await next();
            //    });
            //});

            // Mapping WebSocket with using endpoint

            app.UseEndpoints(endpoints =>
            {
                //endpoints.Map("/ws", async (context) =>
                //{
                //    if (context.WebSockets.IsWebSocketRequest)
                //    {
                //        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                //        await Echo(webSocket);
                //    }
                //});

                // Mapping WebSocket with using your own extension method
                endpoints.MapWebSocket("/ws", async websocket => await Echo(websocket));

                endpoints.Map("/", async context => await context.Response.WriteAsync("Use websocket on /ws endpoint"));

            });
        }


        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }


    public static class WebSocketEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapWebSocket(
                 this IEndpointRouteBuilder endpoints,
                 string pattern,
                 Func<WebSocket, Task> execute = null)
        {

            return endpoints.Map(pattern, async (context) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    await execute?.Invoke(webSocket);
                }
            });
        }
    }
}

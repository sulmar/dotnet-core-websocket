# Websocket w .NET Core 3


## Wprowadzenie

Opis użycia WebSocket w .NET Core znajduje się w artykule [WebSockets support in ASP.NET Core](https://docs.microsoft.com/pl-pl/aspnet/core/fundamentals/websockets?view=aspnetcore-3.1)

Przedstawiony tam sposób używa przechwytywania   żądań za pomocą metody *Use()*. Postanowiłem przerobić to na endpoint, które w .NET Core 3 są zalecanym sposobem mapowania żądań.

Przedstawię poszczególne sposoby mapowania.

## Przykładowa metoda *Echo()*

~~~ csharp

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
~~~


## Mapowanie z użyciem IApplicationBuilder

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.Map("/ws", builder =>
            {
                builder.Use(async (context, next) =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(webSocket);
                        return;
                    }
                    await next();
                });
            });
}
~~~


## Mapowanie z użyciem endpoints

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
     app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/ws", async (context) =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                        await Echo(webSocket);
                    }
                });
}
~~~

## Utworzenie metody rozszerzającej

~~~ csharp
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
~~~

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
      endpoints.MapWebSocket("/ws", async websocket => await Echo(websocket));
}
~~~


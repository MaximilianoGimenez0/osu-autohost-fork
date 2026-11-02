using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Database.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Serilog;

namespace BanchoMultiplayerBot.Host.WebApi.Extensions;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddBotCookieAuthentication(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        return builder.AddCookie(options =>
        {
            // This will depend on the deployment environment, but generally in production you want to use strict
            options.Cookie.SameSite = configuration["Bot:AllowSameSiteNone"] == "true" ? SameSiteMode.None : SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
        
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
            };
        });
    }
    
    public static AuthenticationBuilder AddBotOsuAuth(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        return builder.AddOAuth("osu", options =>
        {
            options.ClientId = configuration["Osu:ClientId"]!;
            options.ClientSecret = configuration["Osu:ClientSecret"]!;
            options.CallbackPath = new PathString("/api/auth/osu-callback");

            options.AuthorizationEndpoint = "https://osu.ppy.sh/oauth/authorize";
            options.TokenEndpoint = "https://osu.ppy.sh/oauth/token";
            options.UserInformationEndpoint = "https://osu.ppy.sh/api/v2/me";

            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");

            options.SaveTokens = true;

            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                    var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    
                    var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                    context.RunClaimActions(user);

                    var username = context.Principal!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value ?? "Desconocido";
                    var userId = context.Principal!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "N/A";
                    var bot = context.HttpContext.RequestServices.GetService<Bot>();

                    if (!await HandleUserAuthentication(context.Principal!.Claims))
                    {
                        Log.Error(BotMessages.Logs.Host.AuthFailedNotAdmin, username, userId);
                        bot?.NotificationManager.Notify(BotMessages.Discord.WebAuthAttempt(username, userId, false));
                        context.Fail("User is not an administrator");
                        throw new Exception("User is not an administrator");
                    }
                    
                    Log.Information(BotMessages.Logs.Host.AuthSucceeded, username, userId);
                    bot?.NotificationManager.Notify(BotMessages.Discord.WebAuthAttempt(username, userId, true));
                },
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect($"{configuration["Bot:FrontendUrl"]}");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });
    }

    /// <summary>
    /// Handles user authentication with provided claims and checks if the user is an administrator.
    /// </summary>
    private static async Task<bool> HandleUserAuthentication(IEnumerable<Claim> claims)
    {
        var username = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        
        if (username == null)
        {
            throw new Exception("Could not find username claim");
        }
        
        var userRepo = new UserRepository();
        var user = await userRepo.FindOrCreateUserAsync(username);

        return user.Administrator;
    }
}

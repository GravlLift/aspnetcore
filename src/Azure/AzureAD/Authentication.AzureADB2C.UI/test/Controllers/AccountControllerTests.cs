﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Microsoft.AspNetCore.Authentication.AzureADB2C.UI.AzureADB2C.Controllers.Internal;

public class AccountControllerTests
{
    [Fact]
    public void SignInNoScheme_ChallengesAADAzureADB2CDefaultScheme()
    {
        // Arrange
        var controller = new AccountController(
            new OptionsMonitor(AzureADB2CDefaults.AuthenticationScheme, new AzureADB2COptions()
            {
                OpenIdConnectSchemeName = AzureADB2CDefaults.OpenIdScheme,
                CookieSchemeName = AzureADB2CDefaults.CookieScheme
            }))
        {
            Url = new TestUrlHelper("~/", "https://localhost/")
        };

        // Act
        var result = controller.SignIn(null);

        // Assert
        var challenge = Assert.IsAssignableFrom<ChallengeResult>(result);
        var challengedScheme = Assert.Single(challenge.AuthenticationSchemes);
        Assert.Equal(AzureADB2CDefaults.AuthenticationScheme, challengedScheme);
        Assert.NotNull(challenge.Properties.RedirectUri);
        Assert.Equal("https://localhost/", challenge.Properties.RedirectUri);
    }

    [Fact]
    public void SignInProvidedScheme_ChallengesCustomScheme()
    {
        // Arrange
        var controller = new AccountController(new OptionsMonitor("Custom", new AzureADB2COptions()));
        controller.Url = new TestUrlHelper("~/", "https://localhost/");

        // Act
        var result = controller.SignIn("Custom");

        // Assert
        var challenge = Assert.IsAssignableFrom<ChallengeResult>(result);
        var challengedScheme = Assert.Single(challenge.AuthenticationSchemes);
        Assert.Equal("Custom", challengedScheme);
    }

    [Fact]
    public void ResetPasswordNoScheme_ChallengesAADAzureADB2CDefaultSchemeWithResetPassworPolicyAsync()
    {
        // Arrange
        var controller = new AccountController(
            new OptionsMonitor(
                AzureADB2CDefaults.AuthenticationScheme,
                new AzureADB2COptions() { ResetPasswordPolicyId = "Reset" }))
        {
            Url = new TestUrlHelper("~/", "https://localhost/")
        };
        controller.ControllerContext = CreateControllerContext(
            CreateAuthenticatedPrincipal(AzureADB2CDefaults.AuthenticationScheme));

        // Act
        var result = controller.ResetPassword(null);

        // Assert
        var challenge = Assert.IsAssignableFrom<ChallengeResult>(result);
        var challengedScheme = Assert.Single(challenge.AuthenticationSchemes);
        Assert.Equal(AzureADB2CDefaults.AuthenticationScheme, challengedScheme);
        Assert.NotNull(challenge.Properties.RedirectUri);
        Assert.Equal("https://localhost/", challenge.Properties.RedirectUri);
        Assert.NotNull(challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
        Assert.Equal("Reset", challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
    }

    [Fact]
    public void ResetPasswordCustomScheme_ChallengesAADAzureADB2CDefaultSchemeWithResetPassworPolicyFromCustomSchemeAsync()
    {
        // Arrange
        var controller = new AccountController(
            new OptionsMonitor(
                "Custom",
                new AzureADB2COptions() { ResetPasswordPolicyId = "CustomReset" }))
        {
            Url = new TestUrlHelper("~/", "https://localhost/")
        };
        controller.ControllerContext = CreateControllerContext(
            CreateAuthenticatedPrincipal("Custom"));

        // Act
        var result = controller.ResetPassword("Custom");

        // Assert
        var challenge = Assert.IsAssignableFrom<ChallengeResult>(result);
        var challengedScheme = Assert.Single(challenge.AuthenticationSchemes);
        Assert.Equal("Custom", challengedScheme);
        Assert.NotNull(challenge.Properties.RedirectUri);
        Assert.Equal("https://localhost/", challenge.Properties.RedirectUri);
        Assert.NotNull(challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
        Assert.Equal("CustomReset", challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
    }

    [Fact]
    public async Task EditProfileNoScheme_ChallengesAADAzureADB2CCustomSchemeWithEditProfilePolicyAsync()
    {
        // Arrange
        var controller = new AccountController(
            new OptionsMonitor(
                AzureADB2CDefaults.AuthenticationScheme,
                new AzureADB2COptions() { EditProfilePolicyId = "EditProfile" }))
        {
            Url = new TestUrlHelper("~/", "https://localhost/")
        };
        controller.ControllerContext = CreateControllerContext(
            CreateAuthenticatedPrincipal(AzureADB2CDefaults.AuthenticationScheme));

        // Act
        var result = await controller.EditProfile(null);

        // Assert
        var challenge = Assert.IsAssignableFrom<ChallengeResult>(result);
        var challengedScheme = Assert.Single(challenge.AuthenticationSchemes);
        Assert.Equal(AzureADB2CDefaults.AuthenticationScheme, challengedScheme);
        Assert.NotNull(challenge.Properties.RedirectUri);
        Assert.Equal("https://localhost/", challenge.Properties.RedirectUri);
        Assert.NotNull(challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
        Assert.Equal("EditProfile", challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
    }

    private ClaimsPrincipal CreateAuthenticatedPrincipal(string scheme) =>
        new ClaimsPrincipal(new ClaimsIdentity(scheme));

    private static ControllerContext CreateControllerContext(ClaimsPrincipal principal = null)
    {
        principal = principal ?? new ClaimsPrincipal(new ClaimsIdentity());
        var mock = new Mock<IAuthenticationService>();
        mock.Setup(authS => authS.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync<HttpContext, string, IAuthenticationService, AuthenticateResult>(
                (ctx, scheme) =>
                {
                    if (principal.Identity.IsAuthenticated)
                    {
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, scheme));
                    }
                    else
                    {
                        return AuthenticateResult.NoResult();
                    }
                });
        return new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                RequestServices = new ServiceCollection()
                    .AddSingleton(mock.Object)
                    .BuildServiceProvider()
            }
        };
    }

    [Fact]
    public async Task EditProfileCustomScheme_ChallengesAADAzureADB2CCustomSchemeWithEditProfilePolicyFromCustomSchemeAsync()
    {
        // Arrange
        var controller = new AccountController(
            new OptionsMonitor(
                "Custom",
                new AzureADB2COptions() { EditProfilePolicyId = "CustomEditProfile" }))
        {
            Url = new TestUrlHelper("~/", "https://localhost/")
        };
        controller.ControllerContext = CreateControllerContext(
            CreateAuthenticatedPrincipal("Custom"));
        // Act
        var result = await controller.EditProfile("Custom");

        // Assert
        var challenge = Assert.IsAssignableFrom<ChallengeResult>(result);
        var challengedScheme = Assert.Single(challenge.AuthenticationSchemes);
        Assert.Equal("Custom", challengedScheme);
        Assert.NotNull(challenge.Properties.RedirectUri);
        Assert.Equal("https://localhost/", challenge.Properties.RedirectUri);
        Assert.NotNull(challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
        Assert.Equal("CustomEditProfile", challenge.Properties.Items[AzureADB2CDefaults.PolicyKey]);
    }

    [Fact]
    public async Task SignOutNoScheme_SignsOutDefaultCookiesAndDefaultOpenIDConnectAADAzureADB2CSchemesAsync()
    {
        // Arrange
        var options = new AzureADB2COptions()
        {
            CookieSchemeName = AzureADB2CDefaults.CookieScheme,
            OpenIdConnectSchemeName = AzureADB2CDefaults.OpenIdScheme
        };

        var controllerContext = CreateControllerContext(
            CreateAuthenticatedPrincipal(AzureADB2CDefaults.AuthenticationScheme));

        var descriptor = new PageActionDescriptor()
        {
            AttributeRouteInfo = new AttributeRouteInfo()
            {
                Template = "/Account/SignedOut"
            }
        };
        var controller = new AccountController(new OptionsMonitor(AzureADB2CDefaults.AuthenticationScheme, options))
        {
            Url = new TestUrlHelper(
                controllerContext.HttpContext,
                new RouteData(),
                descriptor,
                "/Account/SignedOut",
                "https://localhost/Account/SignedOut"),
            ControllerContext = new ControllerContext()
            {
                HttpContext = controllerContext.HttpContext
            }
        };
        controller.Request.Scheme = "https";

        // Act
        var result = await controller.SignOut(null);

        // Assert
        var signOut = Assert.IsAssignableFrom<SignOutResult>(result);
        Assert.Equal(new[] { AzureADB2CDefaults.CookieScheme, AzureADB2CDefaults.OpenIdScheme }, signOut.AuthenticationSchemes);
        Assert.NotNull(signOut.Properties.RedirectUri);
        Assert.Equal("https://localhost/Account/SignedOut", signOut.Properties.RedirectUri);
    }

    [Fact]
    public async Task SignOutProvidedScheme_SignsOutCustomCookiesAndCustomOpenIDConnectAADAzureADB2CSchemesAsync()
    {
        // Arrange
        var options = new AzureADB2COptions()
        {
            CookieSchemeName = "Cookie",
            OpenIdConnectSchemeName = "OpenID"
        };

        var controllerContext = CreateControllerContext(
            CreateAuthenticatedPrincipal(AzureADB2CDefaults.AuthenticationScheme));
        var descriptor = new PageActionDescriptor()
        {
            AttributeRouteInfo = new AttributeRouteInfo()
            {
                Template = "/Account/SignedOut"
            }
        };

        var controller = new AccountController(new OptionsMonitor("Custom", options))
        {
            Url = new TestUrlHelper(
                controllerContext.HttpContext,
                new RouteData(),
                descriptor,
                "/Account/SignedOut",
                "https://localhost/Account/SignedOut"),
            ControllerContext = new ControllerContext()
            {
                HttpContext = controllerContext.HttpContext
            }
        };
        controller.Request.Scheme = "https";

        // Act
        var result = await controller.SignOut("Custom");

        // Assert
        var signOut = Assert.IsAssignableFrom<SignOutResult>(result);
        Assert.Equal(new[] { "Cookie", "OpenID" }, signOut.AuthenticationSchemes);
    }

    private class OptionsMonitor : IOptionsMonitor<AzureADB2COptions>
    {
        public OptionsMonitor(string scheme, AzureADB2COptions options)
        {
            Scheme = scheme;
            Options = options;
        }

        public AzureADB2COptions CurrentValue => throw new NotImplementedException();

        public string Scheme { get; }
        public AzureADB2COptions Options { get; }

        public AzureADB2COptions Get(string name)
        {
            if (name == Scheme)
            {
                return Options;
            }

            return null;
        }

        public IDisposable OnChange(Action<AzureADB2COptions, string> listener)
        {
            throw new NotImplementedException();
        }
    }

    private class TestUrlHelper : IUrlHelper
    {
        public TestUrlHelper(string contentPath, string url)
        {
            ContentPath = contentPath;
            Url = url;
        }

        public TestUrlHelper(
            HttpContext context,
            RouteData routeData,
            ActionDescriptor descriptor,
            string contentPath,
            string url)
        {
            HttpContext = context;
            RouteData = routeData;
            ActionDescriptor = descriptor;
            ContentPath = contentPath;
            Url = url;
        }

        public ActionContext ActionContext =>
            new ActionContext(HttpContext, RouteData, ActionDescriptor);

        public string ContentPath { get; }
        public string Url { get; }
        public HttpContext HttpContext { get; }
        public RouteData RouteData { get; }
        public ActionDescriptor ActionDescriptor { get; }

        public string Action(UrlActionContext actionContext)
        {
            throw new NotImplementedException();
        }

        public string Content(string contentPath)
        {
            if (ContentPath == contentPath)
            {
                return Url;
            }
            return "";
        }

        public bool IsLocalUrl(string url)
        {
            throw new NotImplementedException();
        }

        public string Link(string routeName, object values)
        {
            throw new NotImplementedException();
        }

        public string RouteUrl(UrlRouteContext routeContext)
        {
            if (routeContext.Values is RouteValueDictionary dicionary &&
                dicionary.TryGetValue("page", out var page) &&
                page is string pagePath &&
                ContentPath == pagePath)
            {
                return Url;
            }

            return null;
        }
    }
}

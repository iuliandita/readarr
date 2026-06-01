using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Integration.Test.Client;
using RestSharp;

namespace NzbDrone.Integration.Test.ApiTests
{
    // Regression guard for JSON request body binding.
    //
    // Readarr replaces the default DI container with DryIoc, whose
    // IServiceProviderIsService reports any instantiable concrete type as
    // resolvable. Since .NET 7, MVC implicitly infers [FromServices] for complex
    // action parameters the container claims to provide. On the net6 -> net10
    // upgrade this combination silently bound every POST/PUT body (e.g.
    // CommandResource) from DI as an empty object instead of deserializing the
    // JSON body, breaking every write endpoint. Startup opts out via
    // ApiBehaviorOptions.DisableImplicitFromServicesParameters.
    [TestFixture]
    public class RequestBodyBindingFixture : IntegrationTest
    {
        [Test]
        public void should_bind_json_request_body_to_model()
        {
            var response = Commands.Post(new SimpleCommandResource { Name = "rsssync" });

            // Under the regression the body bound as an empty object, so Name was
            // null and the request failed validation with a 400 before reaching here.
            response.Id.Should().NotBe(0);
            response.Name.Should().Be("rsssync");
        }

        [Test]
        public void should_return_json_parse_error_for_malformed_body()
        {
            var request = new RestRequest("command")
            {
                Method = Method.POST
            };
            request.AddHeader("X-Api-Key", ApiKey);
            request.AddParameter("application/json", "{{{bad", ParameterType.RequestBody);

            var response = RestClient.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // The body must actually be read by the JSON input formatter, producing a
            // parse error -- not silently ignored and rejected by NotNull validation,
            // which is the signature of the FromServices binding regression.
            response.Content.Should().NotContain("NotNullValidator");
            response.Content.Should().Contain("LineNumber");
        }
    }
}

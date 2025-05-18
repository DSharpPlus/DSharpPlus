// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1031

using System.Net;
using System.Text.Json;

using DSharpPlus.Internal.Abstractions.Rest.Errors;
using DSharpPlus.Results;

namespace DSharpPlus.Extensions.Internal.BadRequestHelper;

public static class ResultExtensions
{
    public static Result<T> ExpandBadRequestError<T>(this Result<T> result, object payload)
    {
        return result.MapError
        (
            error =>
            {
                if (error is not HttpError { StatusCode: HttpStatusCode.BadRequest })
                {
                    return error;
                }

                try
                {
                    using JsonDocument document = JsonDocument.Parse(error.Message);

                    if (document.RootElement.TryGetProperty("errors", out JsonElement errors))
                    {
                        return error; //placeholder
                    }
                }
                catch
                {
                    return error;
                }

                return error; //placeholder
            }
        );
    }
}

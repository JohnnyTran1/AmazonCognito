using System;
using System.Net;

namespace AmazonCognito.Models
{
    public static class ErrorHelper
    {
        private static Amazon.Runtime.Internal.HttpErrorResponseException httpEx;

        public static HttpStatusCode GetHttpErrorCode(Exception ex)
        {
            try
            {
                httpEx = (ex != null) ? (Amazon.Runtime.Internal.HttpErrorResponseException)ex : null;

                return (httpEx != null) ? httpEx.Response.StatusCode : HttpStatusCode.InternalServerError;
            }
            catch(Exception)
            {
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}
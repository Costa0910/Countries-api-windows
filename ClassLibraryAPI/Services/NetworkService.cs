using ClassLibraryAPI.Models;

namespace ClassLibraryAPI.Services
{
    public static class NetworkService
    {
        public static async Task<Response> CheckConnection()
        {
            var client = new HttpClient() { BaseAddress = new Uri("http://clients3.google.com") };

            try
            {
                using (await client.GetAsync("/generate_204"))
                {
                    return new Response
                    {
                        Status = true
                    };
                }
            }
            catch
            {
                return new Response()
                {
                    Status = false,
                    Message = "Configure a sua ligação á Internet"
                };
            }
        }
    }
}
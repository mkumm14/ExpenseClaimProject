namespace ExpenseClaimProject.Utils
{
    public static class ImageDownloader
    {

        public static async Task<byte[]> DownloadImageBytesAsync(HttpClient httpClient, string imageUrl)
        {

            using var resp = await httpClient.GetAsync(imageUrl);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsByteArrayAsync();

        }



    }  
}

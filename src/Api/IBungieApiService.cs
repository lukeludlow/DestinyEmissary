namespace EmissaryApi
{
    public interface IBungieApiService
    {
        /// <summary>
        /// send a GET request to the specified URL to retrieve information from the bungie api. 
        /// </summary>
        string Get(string requestUrl);
    }
}
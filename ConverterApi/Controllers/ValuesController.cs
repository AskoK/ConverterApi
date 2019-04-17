using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace ConverterApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ValuesController : ControllerBase
  {
    private IMemoryCache _cache;
    private static readonly HttpClient HttpClient;

    static ValuesController() {
      HttpClient = new HttpClient();
    }

    public ValuesController(IMemoryCache memoryCache)
    {
      _cache = memoryCache;
    }

    [HttpGet]
    [Route("/{from}/{to}/{value}")]
    public async Task<IActionResult> Get(string from, string to, double value)
    {
      string f = from.ToUpper();
      string t = to.ToUpper();
      double rate = await GetChangeValue(f, t);
      double result = (rate * value);
     
      return Ok(result);
    }

    private async Task<double> GetChangeValue(string from, string to)
    {
      HttpResponseMessage response = await HttpClient.GetAsync($"https://api.exchangeratesapi.io/latest?base={from}&symbols={to}");
      response.EnsureSuccessStatusCode();
      var responseBody = await response.Content.ReadAsStringAsync();

      var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
      _cache.Set(from, responseBody, cacheEntryOptions);

      var resB = _cache.Get<string>(from);

      JObject json = JObject.Parse(resB);
      var rate = Double.Parse(json["rates"][to].ToString());

      return rate;
    }
  }
}


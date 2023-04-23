using crudApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace crudApi.Controllers
{
    [Route("")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly CachedMemory _cache;
        private readonly string apiUri = "https://reqres.in";

        public UserController(HttpClient client, CachedMemory cache)
        {
            _cache = cache;
            _client = client;
        }

        [HttpGet("getUsers/{page?}")]
        public async Task<IActionResult> Get(int? page)
        {
            try
            {
                var url = $"{apiUri}/api/users?page={page}";
                var response = await _client.GetAsync(url);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return StatusCode(StatusCodes.Status404NotFound);

                var body = await response.Content.ReadAsStringAsync();

                return Ok(body);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [HttpGet("getUser/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (_cache.Cache.TryGetValue($"user{id}", out UserDto? user))
            {
                return Ok(user);
            }
            try
            {
                var url = $"{apiUri}/api/users/{id}";
                var response = await _client.GetAsync(url);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return StatusCode(StatusCodes.Status404NotFound);
                var body = await response.Content.ReadFromJsonAsync<UserDataDto>();

                if (body != null && body.Data != null)
                {
                    // In case data object passes the limit, continue without caching.
                    try
                    {
                        _cache.Cache.Set<UserDto>($"user{id}", body.Data,
                        new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = new TimeSpan(1, 0, 0),
                            Size = 10
                        });

                        return Ok(body.Data);

                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
                return StatusCode(StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("createUser")]
        public async Task<IActionResult> Post(UserDto dto)
        {
            var url = $"{apiUri}/api/users/";
            try
            {
                var response = await _client.PostAsJsonAsync(url, dto);
                if (response.IsSuccessStatusCode)
                {
                    var user = response.Content.ReadFromJsonAsync<UserDto>().Result;
                    _cache.Cache.Set($"user{user?.id}",user, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = new TimeSpan(1, 0, 0),
                        Size = 10
                    });

                   var createdUrl = $"/getUser/{user?.id}";
                    return Created(createdUrl, user);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPut("updateUser/")]
        public async Task<IActionResult> Put(UserDto dto)
        {
            if (dto == null)
                return StatusCode(StatusCodes.Status404NotFound);

            var url = $"{apiUri}/api/users/{dto?.id}";
            try
            {
                var response = await _client.PutAsJsonAsync(url, dto);

                if (response.IsSuccessStatusCode)
                {
                    _cache.Cache.Set($"user{dto?.id}", dto, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = new TimeSpan(1, 0, 0),
                        Size = 10
                    });
                    return Ok(dto);
                }
                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("deleteUser/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var url = $"{apiUri}/api/users/{id}";
                var response = await _client.DeleteAsync(url);

                // The Dummy Api always return a success status code therefore I check the cache inside.
                // If it wouldn't return true status code every request, I'd make another check in the outer scope if there's no user with this id in the cache.
                if (response.IsSuccessStatusCode)
                {
                    // If no cache or user doesn't exist in the cache
                    if (_cache.Cache.Count == 0)
                         return NoContent();

                    if (_cache.Cache.TryGetValue($"user{id}", out UserDto? obj))
                         _cache.Cache.Remove($"user{id}");

                    return NoContent();
                }
                return StatusCode((int)response.StatusCode);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

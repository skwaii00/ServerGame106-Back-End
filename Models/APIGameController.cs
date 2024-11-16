using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerGame106.Data;
using ServerGame106.DTO;

using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.SignalR.Protocol;
using ServerGame106.ViewModel;

namespace ServerGame106.Models
{
    [Route("api/[controller]")]
    [ApiController]


    public class APIGameController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        protected ResponseApi _response;
        private readonly UserManager<ApplicationUser> _userManager;
        public APIGameController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager
            )
        {
            _db = db;
            _response = new();
            _userManager = userManager;
        }
        [HttpGet("GetAllGameLevel")]
        public async Task<IActionResult> GetAllGameLevel()
        {
            try
            {
                var gameLevel = await _db.GameLevels.ToListAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lay du lieu thanh cong";
                _response.Data = gameLevel;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }
        [HttpGet("GetAllQuestionGame")]
        public async Task<IActionResult> GetAllQuestionGame()
        {
            try
            {
                var questionGame = await _db.Questions.ToListAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lay du lieu thanh cong";
                _response.Data = questionGame;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }
        [HttpGet("GetAllQuestionGameByLevel/{levelId}")]
        public async Task<IActionResult> GetAllQuestionGameByLevel(int levelId)
        {
            try
            {
                var questionGame = await _db.Questions.Where(x => x.levelId == levelId).ToListAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lay du lieu thanh cong";
                _response.Data = questionGame;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpGet("SaveResult")]
        public async Task<IActionResult> SaveResult(LevelResultDTO levelResult)
        {
            try
            {
                var levelResultSave = new LevelResult
                {
                    UserId = levelResult.UserId,
                    LevelId = levelResult.LevelId,
                    Score = levelResult.Score,
                    CompletionDate = DateOnly.FromDateTime(DateTime.Now)
                };
                await _db.LevelResults.AddAsync(levelResultSave);
                await _db.SaveChangesAsync();
                _response.IsSuccess = true;
                _response.Notification = "Luu ket qua thanh cong";
                _response.Data = levelResult;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpGet("GetAllRegion")]
        public async Task<IActionResult> GetAllRegion()
        {
            try
            {
                var region = await _db.Regions.ToListAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lay du lieu thanh cong";
                _response.Data = region;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                var user = new ApplicationUser
                {
                    Email = registerDTO.Email,
                    UserName = registerDTO.Email,
                    Name = registerDTO.Email,
                    Avatar = registerDTO.LinkAvatar,
                    RegionId = registerDTO.RegionId
                };
                var result = await _userManager.CreateAsync(user, registerDTO.Password);
                if (result.Succeeded)
                {
                    _response.IsSuccess = true;
                    _response.Notification = "Dang ky thanh cong";
                    _response.Data = user;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Notification = "dang ky that bai";
                    _response.Data = result.Errors;
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var email = loginRequest.Email;
                var password = loginRequest.Password;
                var user = await _userManager.FindByNameAsync(email);

                if (user != null && await _userManager.CheckPasswordAsync(user, password))
                {
                    _response.IsSuccess = true;
                    _response.Notification = "Dang nhap thanh cong";
                    _response.Data = user;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Dang nhap that bai";
                    _response.Data = null;
                    return BadRequest(_response);

                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Co loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("Rating/{idRegion}")]
        public async Task<IActionResult> Rating(int idRegion)
        {
            try
            {
                string nameRegion = null; // Khai báo biến nameRegion với giá trị null  

                if (idRegion > 0)
                {
                    nameRegion = await _db.Regions.Where(x => x.RegionId == idRegion).Select(x => x.Name).FirstOrDefaultAsync();

                    if (nameRegion == null)
                    {
                        _response.IsSuccess = false;
                        _response.Notification = "Khong tim thay khu vuc";
                        _response.Data = null;
                        return BadRequest(_response);
                    }

                    // Lấy người dùng theo khu vực  
                    var userByRegion = await _db.Users.Where(x => x.RegionId == idRegion).ToListAsync();
                    var resultLevelByRegion = await _db.LevelResults.Where(x => userByRegion.Select(u => u.Id).Contains(x.UserId)).ToListAsync();

                    RatingVM ratingVM = new()
                    {
                        NameRegion = nameRegion,
                        userResultSums = new List<UserResultSum>()
                    };

                    foreach (var item in userByRegion)
                    {
                        var sumScore = resultLevelByRegion.Where(x => x.UserId == item.Id).Sum(x => x.Score);
                        var sumLevel = resultLevelByRegion.Where(x => x.UserId == item.Id).Count();
                        UserResultSum userResultSum = new()
                        {
                            NameUser = item.Name,
                            SumScore = sumScore,
                            SumLevel = sumLevel
                        };
                        ratingVM.userResultSums.Add(userResultSum);
                    }

                    _response.IsSuccess = true;
                    _response.Notification = "Lay du lieu thanh cong";
                    _response.Data = ratingVM;
                    return Ok(_response);
                }
                else
                {
                    var user = await _db.Users.ToListAsync();
                    var resultLevel = await _db.LevelResults.ToListAsync();
                    nameRegion = "Tat ca";

                    RatingVM ratingVM = new()
                    {
                        NameRegion = nameRegion,
                        userResultSums = new List<UserResultSum>()
                    };

                    foreach (var item in user)
                    {
                        var sumScore = resultLevel.Where(x => x.UserId == item.Id).Sum(x => x.Score);
                        var sumLevel = resultLevel.Where(x => x.UserId == item.Id).Count();
                        UserResultSum userResultSum = new()
                        {
                            NameUser = item.Name,
                            SumScore = sumScore,
                            SumLevel = sumLevel
                        };
                        ratingVM.userResultSums.Add(userResultSum); // Sửa lỗi đánh chính ở đây  
                    }

                    _response.IsSuccess = true;
                    _response.Notification = "Lay du lieu thanh cong";
                    _response.Data = ratingVM;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("GetUserInformation/{userId}")]
        public async Task<IActionResult> GetUserInformation(string userId)
        {
            try
            {
                var user = await _db.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Khong tim thay nguoi dung";
                    _response.Data = null;
                    return BadRequest(_response);
                }
                UserInformationVM userInformationVM = new();
                userInformationVM.Name = user.Name;
                userInformationVM.Email = user.Email;
                userInformationVM.avatar = user.Avatar;
                userInformationVM.Region = await _db.Regions.Where(x => x.RegionId == user.RegionId)
                    .Select(x => x.Name).FirstOrDefaultAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lay du lieu thanh cong";
                _response.Data = userInformationVM;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Loi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

    }
}

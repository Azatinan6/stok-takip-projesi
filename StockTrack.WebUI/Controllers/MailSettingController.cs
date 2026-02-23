using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockTrack.Business.Abstract;
using StockTrack.Dto.MailSettings;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;

namespace StockTrack.WebUI.Controllers
{
    [Authorize(Roles = RoleConsts.Admin)]
    public class MailSettingController : Controller
    {
        private readonly IMailSettingService _mailSettingService;
        private readonly UserManager<AppUser> _userManager;

        public MailSettingController(IMailSettingService mailSettingService, UserManager<AppUser> userManager)
        {
            _mailSettingService = mailSettingService;
            _userManager = userManager;
        }
        public async Task<IActionResult> SettingSmtp()
        {
            // Kullanımı:
            var mailSettings = await _mailSettingService.TGetListAsync();

            var mailList = mailSettings.Select(x => new ResultMailSetting
                                        {
                                            Id = x.Id,
                                            CreatedBy = x.CreatedBy,
                                            CreatedDate = x.CreatedDate,
                                            FromMail = x.FromMail,
                                            IsActive = x.IsActive,
                                            Password = x.Password,
                                            SmtpHost = x.SmtpHost,
                                            SmtpPort = x.SmtpPort
                                        }).ToList();
            return View(mailList);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMailSetting(CreateMailSettingsSmtpDto mailSetting)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid)
            {
                return RedirectToAction("SettingSmtp");
            }

            MailSetting newMailSetting = new();

            // Şifreyi hash'le
            var passwordHash = _mailSettingService.Encrypt(mailSetting.Password);


            // Eski mail kaydını pasif yap
            var oldMailSetting = await _mailSettingService.GetLastMailSettingAsync();
            if (oldMailSetting != null)
            {
                oldMailSetting.IsActive = false;
                oldMailSetting.DeletedDate = DateTime.Now;
                oldMailSetting.IsDeleted = true;
                oldMailSetting.DeletedBy = currentUser?.NameSurname;

                await _mailSettingService.TUpdateAsync(oldMailSetting);
            }

            // Yeni kaydı aktif olarak ayarla
            newMailSetting.IsActive = true;
            newMailSetting.CreatedDate = DateTime.Now;
            newMailSetting.ModifiedDate = DateTime.Now;
            newMailSetting.CreatedBy = currentUser?.NameSurname;
            newMailSetting.FromMail = mailSetting.FromMail;
            newMailSetting.SmtpPort = mailSetting.SmtpPort;
            newMailSetting.SmtpHost = mailSetting.SmtpHost;
            newMailSetting.Password = passwordHash;

            await _mailSettingService.TCreateAsync(newMailSetting);

            return RedirectToAction("SettingSmtp");
        }
    }
}

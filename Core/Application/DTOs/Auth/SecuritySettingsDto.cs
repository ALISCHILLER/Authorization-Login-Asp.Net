using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// تنظیمات امنیتی
    /// </summary>
    public class SecuritySettingsDto : BaseDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// آیا احراز هویت دو مرحله‌ای فعال است؟
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// نوع احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }

        /// <summary>
        /// آیا ورود با ایمیل فعال است؟
        /// </summary>
        public bool LoginWithEmailEnabled { get; set; }

        /// <summary>
        /// آیا ورود با شماره موبایل فعال است؟
        /// </summary>
        public bool LoginWithPhoneEnabled { get; set; }

        /// <summary>
        /// آیا ورود با نام کاربری فعال است؟
        /// </summary>
        public bool LoginWithUsernameEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Google فعال است؟
        /// </summary>
        public bool LoginWithGoogleEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Microsoft فعال است؟
        /// </summary>
        public bool LoginWithMicrosoftEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Facebook فعال است؟
        /// </summary>
        public bool LoginWithFacebookEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Twitter فعال است؟
        /// </summary>
        public bool LoginWithTwitterEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Apple فعال است؟
        /// </summary>
        public bool LoginWithAppleEnabled { get; set; }

        /// <summary>
        /// آیا ورود با GitHub فعال است؟
        /// </summary>
        public bool LoginWithGitHubEnabled { get; set; }

        /// <summary>
        /// آیا ورود با LinkedIn فعال است؟
        /// </summary>
        public bool LoginWithLinkedInEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Yahoo فعال است؟
        /// </summary>
        public bool LoginWithYahooEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Instagram فعال است؟
        /// </summary>
        public bool LoginWithInstagramEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Telegram فعال است؟
        /// </summary>
        public bool LoginWithTelegramEnabled { get; set; }

        /// <summary>
        /// آیا ورود با WhatsApp فعال است؟
        /// </summary>
        public bool LoginWithWhatsAppEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Viber فعال است؟
        /// </summary>
        public bool LoginWithViberEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Line فعال است؟
        /// </summary>
        public bool LoginWithLineEnabled { get; set; }

        /// <summary>
        /// آیا ورود با KakaoTalk فعال است؟
        /// </summary>
        public bool LoginWithKakaoTalkEnabled { get; set; }

        /// <summary>
        /// آیا ورود با WeChat فعال است؟
        /// </summary>
        public bool LoginWithWeChatEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ فعال است؟
        /// </summary>
        public bool LoginWithQQEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Weibo فعال است؟
        /// </summary>
        public bool LoginWithWeiboEnabled { get; set; }

        /// <summary>
        /// آیا ورود با VK فعال است؟
        /// </summary>
        public bool LoginWithVKEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Odnoklassniki فعال است؟
        /// </summary>
        public bool LoginWithOdnoklassnikiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Yandex فعال است؟
        /// </summary>
        public bool LoginWithYandexEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Mail.ru فعال است؟
        /// </summary>
        public bool LoginWithMailRuEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Baidu فعال است؟
        /// </summary>
        public bool LoginWithBaiduEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Naver فعال است؟
        /// </summary>
        public bool LoginWithNaverEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kakao فعال است؟
        /// </summary>
        public bool LoginWithKakaoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Line فعال است؟
        /// </summary>
        public bool LoginWithLineEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Paypal فعال است؟
        /// </summary>
        public bool LoginWithPaypalEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Amazon فعال است؟
        /// </summary>
        public bool LoginWithAmazonEnabled { get; set; }

        /// <summary>
        /// آیا ورود با eBay فعال است؟
        /// </summary>
        public bool LoginWithEbayEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Alibaba فعال است؟
        /// </summary>
        public bool LoginWithAlibabaEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Taobao فعال است؟
        /// </summary>
        public bool LoginWithTaobaoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Tmall فعال است؟
        /// </summary>
        public bool LoginWithTmallEnabled { get; set; }

        /// <summary>
        /// آیا ورود با JD فعال است؟
        /// </summary>
        public bool LoginWithJDEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Pinduoduo فعال است؟
        /// </summary>
        public bool LoginWithPinduoduoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Meituan فعال است؟
        /// </summary>
        public bool LoginWithMeituanEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Ele.me فعال است؟
        /// </summary>
        public bool LoginWithEleMeEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Didi فعال است؟
        /// </summary>
        public bool LoginWithDidiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Toutiao فعال است؟
        /// </summary>
        public bool LoginWithToutiaoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Douyin فعال است؟
        /// </summary>
        public bool LoginWithDouyinEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuaishou فعال است؟
        /// </summary>
        public bool LoginWithKuaishouEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Bilibili فعال است؟
        /// </summary>
        public bool LoginWithBilibiliEnabled { get; set; }

        /// <summary>
        /// آیا ورود با iQIYI فعال است؟
        /// </summary>
        public bool LoginWithIQIYIEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Youku فعال است؟
        /// </summary>
        public bool LoginWithYoukuEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Tencent Video فعال است؟
        /// </summary>
        public bool LoginWithTencentVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Mango TV فعال است؟
        /// </summary>
        public bool LoginWithMangoTVEnabled { get; set; }

        /// <summary>
        /// آیا ورود با PPTV فعال است؟
        /// </summary>
        public bool LoginWithPPTVEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Sohu Video فعال است؟
        /// </summary>
        public bool LoginWithSohuVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با LeTV فعال است؟
        /// </summary>
        public bool LoginWithLeTVEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 1905 فعال است؟
        /// </summary>
        public bool LoginWith1905Enabled { get; set; }

        /// <summary>
        /// آیا ورود با Xunlei فعال است؟
        /// </summary>
        public bool LoginWithXunleiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Thunder فعال است؟
        /// </summary>
        public bool LoginWithThunderEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ Music فعال است؟
        /// </summary>
        public bool LoginWithQQMusicEnabled { get; set; }

        /// <summary>
        /// آیا ورود با NetEase Cloud Music فعال است؟
        /// </summary>
        public bool LoginWithNetEaseCloudMusicEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kugou فعال است؟
        /// </summary>
        public bool LoginWithKugouEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuwo فعال است؟
        /// </summary>
        public bool LoginWithKuwoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Migu فعال است؟
        /// </summary>
        public bool LoginWithMiguEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 5sing فعال است؟
        /// </summary>
        public bool LoginWith5singEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Xiami فعال است؟
        /// </summary>
        public bool LoginWithXiamiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ Video فعال است؟
        /// </summary>
        public bool LoginWithQQVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Weibo Video فعال است؟
        /// </summary>
        public bool LoginWithWeiboVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Douyin Video فعال است؟
        /// </summary>
        public bool LoginWithDouyinVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuaishou Video فعال است؟
        /// </summary>
        public bool LoginWithKuaishouVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Bilibili Video فعال است؟
        /// </summary>
        public bool LoginWithBilibiliVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با iQIYI Video فعال است؟
        /// </summary>
        public bool LoginWithIQIYIVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Youku Video فعال است؟
        /// </summary>
        public bool LoginWithYoukuVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Tencent Video Video فعال است؟
        /// </summary>
        public bool LoginWithTencentVideoVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Mango TV Video فعال است؟
        /// </summary>
        public bool LoginWithMangoTVVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با PPTV Video فعال است؟
        /// </summary>
        public bool LoginWithPPTVVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Sohu Video Video فعال است؟
        /// </summary>
        public bool LoginWithSohuVideoVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با LeTV Video فعال است؟
        /// </summary>
        public bool LoginWithLeTVVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 1905 Video فعال است؟
        /// </summary>
        public bool LoginWith1905VideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Xunlei Video فعال است؟
        /// </summary>
        public bool LoginWithXunleiVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Thunder Video فعال است؟
        /// </summary>
        public bool LoginWithThunderVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ Music Video فعال است؟
        /// </summary>
        public bool LoginWithQQMusicVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با NetEase Cloud Music Video فعال است؟
        /// </summary>
        public bool LoginWithNetEaseCloudMusicVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kugou Video فعال است؟
        /// </summary>
        public bool LoginWithKugouVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuwo Video فعال است؟
        /// </summary>
        public bool LoginWithKuwoVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Migu Video فعال است؟
        /// </summary>
        public bool LoginWithMiguVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 5sing Video فعال است؟
        /// </summary>
        public bool LoginWith5singVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Xiami Video فعال است؟
        /// </summary>
        public bool LoginWithXiamiVideoEnabled { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی تنظیمات امنیتی
    /// </summary>
    public class UpdateSecuritySettingsRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// آیا ورود با ایمیل فعال است؟
        /// </summary>
        public bool? LoginWithEmailEnabled { get; set; }

        /// <summary>
        /// آیا ورود با شماره موبایل فعال است؟
        /// </summary>
        public bool? LoginWithPhoneEnabled { get; set; }

        /// <summary>
        /// آیا ورود با نام کاربری فعال است؟
        /// </summary>
        public bool? LoginWithUsernameEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Google فعال است؟
        /// </summary>
        public bool? LoginWithGoogleEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Microsoft فعال است؟
        /// </summary>
        public bool? LoginWithMicrosoftEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Facebook فعال است؟
        /// </summary>
        public bool? LoginWithFacebookEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Twitter فعال است؟
        /// </summary>
        public bool? LoginWithTwitterEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Apple فعال است؟
        /// </summary>
        public bool? LoginWithAppleEnabled { get; set; }

        /// <summary>
        /// آیا ورود با GitHub فعال است؟
        /// </summary>
        public bool? LoginWithGitHubEnabled { get; set; }

        /// <summary>
        /// آیا ورود با LinkedIn فعال است؟
        /// </summary>
        public bool? LoginWithLinkedInEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Yahoo فعال است؟
        /// </summary>
        public bool? LoginWithYahooEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Instagram فعال است؟
        /// </summary>
        public bool? LoginWithInstagramEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Telegram فعال است؟
        /// </summary>
        public bool? LoginWithTelegramEnabled { get; set; }

        /// <summary>
        /// آیا ورود با WhatsApp فعال است؟
        /// </summary>
        public bool? LoginWithWhatsAppEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Viber فعال است؟
        /// </summary>
        public bool? LoginWithViberEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Line فعال است؟
        /// </summary>
        public bool? LoginWithLineEnabled { get; set; }

        /// <summary>
        /// آیا ورود با KakaoTalk فعال است؟
        /// </summary>
        public bool? LoginWithKakaoTalkEnabled { get; set; }

        /// <summary>
        /// آیا ورود با WeChat فعال است؟
        /// </summary>
        public bool? LoginWithWeChatEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ فعال است؟
        /// </summary>
        public bool? LoginWithQQEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Weibo فعال است؟
        /// </summary>
        public bool? LoginWithWeiboEnabled { get; set; }

        /// <summary>
        /// آیا ورود با VK فعال است؟
        /// </summary>
        public bool? LoginWithVKEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Odnoklassniki فعال است؟
        /// </summary>
        public bool? LoginWithOdnoklassnikiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Yandex فعال است؟
        /// </summary>
        public bool? LoginWithYandexEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Mail.ru فعال است؟
        /// </summary>
        public bool? LoginWithMailRuEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Baidu فعال است؟
        /// </summary>
        public bool? LoginWithBaiduEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Naver فعال است؟
        /// </summary>
        public bool? LoginWithNaverEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kakao فعال است؟
        /// </summary>
        public bool? LoginWithKakaoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Line فعال است؟
        /// </summary>
        public bool? LoginWithLineEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Paypal فعال است؟
        /// </summary>
        public bool? LoginWithPaypalEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Amazon فعال است؟
        /// </summary>
        public bool? LoginWithAmazonEnabled { get; set; }

        /// <summary>
        /// آیا ورود با eBay فعال است؟
        /// </summary>
        public bool? LoginWithEbayEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Alibaba فعال است؟
        /// </summary>
        public bool? LoginWithAlibabaEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Taobao فعال است؟
        /// </summary>
        public bool? LoginWithTaobaoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Tmall فعال است؟
        /// </summary>
        public bool? LoginWithTmallEnabled { get; set; }

        /// <summary>
        /// آیا ورود با JD فعال است؟
        /// </summary>
        public bool? LoginWithJDEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Pinduoduo فعال است؟
        /// </summary>
        public bool? LoginWithPinduoduoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Meituan فعال است؟
        /// </summary>
        public bool? LoginWithMeituanEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Ele.me فعال است؟
        /// </summary>
        public bool? LoginWithEleMeEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Didi فعال است؟
        /// </summary>
        public bool? LoginWithDidiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Toutiao فعال است؟
        /// </summary>
        public bool? LoginWithToutiaoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Douyin فعال است؟
        /// </summary>
        public bool? LoginWithDouyinEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuaishou فعال است؟
        /// </summary>
        public bool? LoginWithKuaishouEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Bilibili فعال است؟
        /// </summary>
        public bool? LoginWithBilibiliEnabled { get; set; }

        /// <summary>
        /// آیا ورود با iQIYI فعال است؟
        /// </summary>
        public bool? LoginWithIQIYIEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Youku فعال است؟
        /// </summary>
        public bool? LoginWithYoukuEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Tencent Video فعال است؟
        /// </summary>
        public bool? LoginWithTencentVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Mango TV فعال است؟
        /// </summary>
        public bool? LoginWithMangoTVEnabled { get; set; }

        /// <summary>
        /// آیا ورود با PPTV فعال است؟
        /// </summary>
        public bool? LoginWithPPTVEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Sohu Video فعال است؟
        /// </summary>
        public bool? LoginWithSohuVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با LeTV فعال است؟
        /// </summary>
        public bool? LoginWithLeTVEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 1905 فعال است؟
        /// </summary>
        public bool? LoginWith1905Enabled { get; set; }

        /// <summary>
        /// آیا ورود با Xunlei فعال است؟
        /// </summary>
        public bool? LoginWithXunleiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Thunder فعال است؟
        /// </summary>
        public bool? LoginWithThunderEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ Music فعال است؟
        /// </summary>
        public bool? LoginWithQQMusicEnabled { get; set; }

        /// <summary>
        /// آیا ورود با NetEase Cloud Music فعال است؟
        /// </summary>
        public bool? LoginWithNetEaseCloudMusicEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kugou فعال است؟
        /// </summary>
        public bool? LoginWithKugouEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuwo فعال است؟
        /// </summary>
        public bool? LoginWithKuwoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Migu فعال است؟
        /// </summary>
        public bool? LoginWithMiguEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 5sing فعال است؟
        /// </summary>
        public bool? LoginWith5singEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Xiami فعال است؟
        /// </summary>
        public bool? LoginWithXiamiEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ Video فعال است؟
        /// </summary>
        public bool? LoginWithQQVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Weibo Video فعال است؟
        /// </summary>
        public bool? LoginWithWeiboVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Douyin Video فعال است؟
        /// </summary>
        public bool? LoginWithDouyinVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuaishou Video فعال است؟
        /// </summary>
        public bool? LoginWithKuaishouVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Bilibili Video فعال است؟
        /// </summary>
        public bool? LoginWithBilibiliVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با iQIYI Video فعال است؟
        /// </summary>
        public bool? LoginWithIQIYIVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Youku Video فعال است؟
        /// </summary>
        public bool? LoginWithYoukuVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Tencent Video Video فعال است؟
        /// </summary>
        public bool? LoginWithTencentVideoVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Mango TV Video فعال است؟
        /// </summary>
        public bool? LoginWithMangoTVVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با PPTV Video فعال است؟
        /// </summary>
        public bool? LoginWithPPTVVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Sohu Video Video فعال است؟
        /// </summary>
        public bool? LoginWithSohuVideoVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با LeTV Video فعال است؟
        /// </summary>
        public bool? LoginWithLeTVVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 1905 Video فعال است؟
        /// </summary>
        public bool? LoginWith1905VideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Xunlei Video فعال است؟
        /// </summary>
        public bool? LoginWithXunleiVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Thunder Video فعال است؟
        /// </summary>
        public bool? LoginWithThunderVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با QQ Music Video فعال است؟
        /// </summary>
        public bool? LoginWithQQMusicVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با NetEase Cloud Music Video فعال است؟
        /// </summary>
        public bool? LoginWithNetEaseCloudMusicVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kugou Video فعال است؟
        /// </summary>
        public bool? LoginWithKugouVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Kuwo Video فعال است؟
        /// </summary>
        public bool? LoginWithKuwoVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Migu Video فعال است؟
        /// </summary>
        public bool? LoginWithMiguVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با 5sing Video فعال است؟
        /// </summary>
        public bool? LoginWith5singVideoEnabled { get; set; }

        /// <summary>
        /// آیا ورود با Xiami Video فعال است؟
        /// </summary>
        public bool? LoginWithXiamiVideoEnabled { get; set; }
    }
} 